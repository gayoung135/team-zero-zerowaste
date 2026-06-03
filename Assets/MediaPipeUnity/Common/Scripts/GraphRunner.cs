// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Rendering;
#endif

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mediapipe.Unity.Sample
{
  /*주석 추가함*/
  // MediaPipe AI 연산 그래프의 생명주기와 데이터 스트리밍을 제어하는 추상(Abstract) 기반 클래스
  public abstract class GraphRunner : MonoBehaviour
  {
    public enum ConfigType
    {
      None,
      CPU,
      GPU,
      OpenGLES,
    }

#pragma warning disable IDE1006
    // TODO: make it static
    protected string TAG => GetType().Name; // 디버깅 로그 식별을 위한 클래스 이름 태그
#pragma warning restore IDE1006

    [SerializeField] private TextAsset _cpuConfig = null; // CPU 연산용 프로토콜 버퍼 텍스트 설정 파일
    [SerializeField] private TextAsset _gpuConfig = null; // PC/콘솔 GPU 가속용 설정 파일
    [SerializeField] private TextAsset _openGlEsConfig = null; // 안드로이드 모바일 기기 전용 GPU 설정 파일
    [SerializeField] private long _timeoutMicrosec = 0; //AI 연산 패킷 처리를 대기할 제한 시간(마이크로초)

    private static readonly GlobalInstanceTable<int, GraphRunner> _InstanceTable = new GlobalInstanceTable<int, GraphRunner>(5);
    private static readonly Dictionary<IntPtr, int> _NameTable = new Dictionary<IntPtr, int>();

    protected RunningMode runningMode { get; private set; } = RunningMode.Async; // AI 그래프 동기/비동기 작동 모드
    private bool _isRunning = false; //현재 AI 연산 그래프가 루프를 돌며 동작 중인지 나타내는 플래그

    /*주석 추가함*/
    // 현재 기기의 하드웨어 성능을 감지하여 최적의 연산 모드(CPU/GPU/GLES)를 실시간 판정하는 프로퍼티
    public InferenceMode inferenceMode => configType == ConfigType.CPU ? InferenceMode.CPU : InferenceMode.GPU;
    public virtual ConfigType configType
    {
      get
      {
        if (GpuManager.IsInitialized)
        {
#if UNITY_ANDROID
          if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && _openGlEsConfig != null)
          {
            return ConfigType.OpenGLES;
          }
#endif
          if (_gpuConfig != null)
          {
            return ConfigType.GPU;
          }
        }
        return _cpuConfig != null ? ConfigType.CPU : ConfigType.None;
      }
    }

    // 판정된 연산 모드에 대응하는 텍스트 설정 데이터를 반환
    public TextAsset textConfig
    {
      get
      {
        switch (configType)
        {
          case ConfigType.CPU: return _cpuConfig;
          case ConfigType.GPU: return _gpuConfig;
          case ConfigType.OpenGLES: return _openGlEsConfig;
          case ConfigType.None:
          default: return null;
        }
      }
    }

    public long timeoutMicrosec
    {
      get => _timeoutMicrosec;
      set => _timeoutMicrosec = (long)Mathf.Max(0, value);
    }
    public long timeoutMillisec
    {
      get => timeoutMicrosec / 1000;
      set => timeoutMicrosec = value * 1000;
    }

    public RotationAngle rotation { get; private set; } = 0; // 영상 회전 상태값
    private Stopwatch _stopwatch; // 실질적인 네이티브 AI 연산 그래프 객체

    protected CalculatorGraph calculatorGraph { get; private set; } // 실질적인 네이티브 AI 연산 그래프 객체
    protected long latestTimestamp; // 최근 처리된 영상 프레임의 고유 타임스탬프

    /*주석 추가함*/
    // 오브젝트 생성 시 전역 인스턴스 테이블에 본인 ID를 등록하여 관리체계를 일원화
    protected virtual void Start()
    {
      _InstanceTable.Add(GetInstanceID(), this);
    }
    /*주석 추가함*/
    // 메모리 누수 방지를 위해 객체 파괴 시 실행 중인 AI 연산 프로세스를 강제 중지
    protected virtual void OnDestroy()
    {
      Stop();
    }

    /*주석 추가함*/
    // 외부에서 비동기 초기화를 간편하게 호출하고 대기할 수 있게 해주는 커스텀 래퍼 메서드
    public WaitForResult WaitForInit(RunningMode runningMode)
    {
      return new WaitForResult(this, Initialize(runningMode));
    }

    /*주석 추가함*/
    // 하드웨어 가속 세팅을 읽고, 연산 스톱워치를 가동하며 
    // 필수 AI 에셋(가중치 파일 등)을 비동기로 로드하는 초기화 핵심 루틴
    public virtual IEnumerator Initialize(RunningMode runningMode)
    {
      this.runningMode = runningMode;

      Debug.Log($"Config Type = {configType}");
      Debug.Log($"Running Mode = {runningMode}");

      InitializeCalculatorGraph();
      _stopwatch = new Stopwatch();
      _stopwatch.Start();

      Debug.Log("Loading dependent assets...");
      var assetRequests = RequestDependentAssets();
      yield return new WaitWhile(() => assetRequests.Any((request) => request.keepWaiting));

      var errors = assetRequests.Where((request) => request.isError).Select((request) => request.error).ToList();
      if (errors.Count > 0)
      {
        foreach (var error in errors)
        {
          Debug.LogError(error);
        }
        throw new InternalException("Failed to prepare dependent assets");
      }
    }

    /*주석 추가함*/
    // 카메라 장치(ImageSource)에 맞게 연산 스레드를 구동하는 자식 클래스 구현용 추상 메서드
    public abstract void StartRun(ImageSource imageSource);

    /*주석 추가함*/
    // 준비된 세팅 값 패킷들을 그래프 내부 인풋 노드에 삽입하고 실제 AI 추론 연산 루프를 가동
    protected void StartRun(PacketMap sidePacket)
    {
      calculatorGraph.StartRun(sidePacket);
      _isRunning = true;
    }

    /*주석 추가함*/
    // 작동 중인 네이티브 연산 채널들을 안전하게 폐쇄하고 메모리를 명시적으로 해제(Dispose)하는 메서드
    public virtual void Stop()
    {
      if (calculatorGraph != null)
      {
        if (_isRunning)
        {
          try
          {
            calculatorGraph.CloseAllPacketSources();
          }
          catch (BadStatusException exception)
          {
            Debug.LogError(exception);
          }

          try
          {
            calculatorGraph.WaitUntilDone();
          }
          catch (BadStatusException exception)
          {
            Debug.LogError(exception);
          }
        }

        _isRunning = false;
        var _ = _NameTable.Remove(calculatorGraph.mpPtr);
        calculatorGraph.Dispose();
        calculatorGraph = null;
      }

      if (_stopwatch != null && _stopwatch.IsRunning)
      {
        _stopwatch.Stop();
      }
    }

    /*주석 추가함*/
    // 제네릭 형식 데이터를 MediaPipe의 특정 입력 스트림 채널에 패킷 형태로 강제 주입
    protected void AddPacketToInputStream<T>(string streamName, Packet<T> packet)
    {
      calculatorGraph.AddPacketToInputStream(streamName, packet);
    }

    /*주석 추가함*/
    //유니티 카메라 렌더링 프레임 데이터를 타임스탬프와 매칭하여 네이티브 AI 분석 노드로 전달
    protected void AddTextureFrameToInputStream(string streamName, Experimental.TextureFrame textureFrame, GlContext glContext = null)
    {
      latestTimestamp = GetCurrentTimestampMicrosec();

      if (glContext != null)  // GPU 컨텍스트가 존재할 경우 하드웨어 텍스처 가속 버퍼로 직접 주입
      {
        var gpuBuffer = textureFrame.BuildGpuBuffer(glContext);
        AddPacketToInputStream(streamName, Packet.CreateGpuBufferAt(gpuBuffer, latestTimestamp));
        return;
      }

      var imageFrame = textureFrame.BuildImageFrame(); // CPU 연산일 경우 일반 이미지 프레임으로 가공하여 주입
      textureFrame.Release();

      AddPacketToInputStream(streamName, Packet.CreateImageFrameAt(imageFrame, latestTimestamp));
    }

    /*주석 추가함*/
    // 지정된 패킷 데이터가 유효한지 검증하고 내부 원본 값을 추출해 오는 안전장치 메서드
    protected bool TryGetValue<T>(Packet<T> packet, out T value, Func<Packet<T>, T> getter)
    {
      if (packet == null)
      {
        value = default;
        return false;
      }
      value = getter(packet);
      return true;
    }

    /*주석 추가함*/
    // AI 출력 스트림으로부터 결과물 패킷 수신이 
    // 정상적으로 처리되었는지 다중 검증하는 단언문 메서드셋(AssertResult 공통)
    protected void AssertResult<T>(OutputStream<T>.NextResult result)
    {
      if (!result.ok)
      {
        throw new Exception("Failed to get the next packet");
      }
    }


    protected void AssertResult<T1, T2>((OutputStream<T1>.NextResult, OutputStream<T2>.NextResult) result)
    {
      AssertResult(result.Item1);
      AssertResult(result.Item2);
    }

    protected void AssertResult<T1, T2, T3>((OutputStream<T1>.NextResult, OutputStream<T2>.NextResult, OutputStream<T3>.NextResult) result)
    {
      AssertResult(result.Item1);
      AssertResult(result.Item2);
      AssertResult(result.Item3);
    }

    protected void AssertResult<T1, T2, T3, T4>((OutputStream<T1>.NextResult, OutputStream<T2>.NextResult, OutputStream<T3>.NextResult, OutputStream<T4>.NextResult) result)
    {
      AssertResult(result.Item1);
      AssertResult(result.Item2);
      AssertResult(result.Item3);
      AssertResult(result.Item4);
    }

    protected void AssertResult<T1, T2, T3, T4, T5>(
      (
        OutputStream<T1>.NextResult,
        OutputStream<T2>.NextResult,
        OutputStream<T3>.NextResult,
        OutputStream<T4>.NextResult,
        OutputStream<T5>.NextResult
      ) result)
    {
      AssertResult(result.Item1);
      AssertResult(result.Item2);
      AssertResult(result.Item3);
      AssertResult(result.Item4);
      AssertResult(result.Item5);
    }

    protected void AssertResult<T1, T2, T3, T4, T5, T6>(
      (
        OutputStream<T1>.NextResult,
        OutputStream<T2>.NextResult,
        OutputStream<T3>.NextResult,
        OutputStream<T4>.NextResult,
        OutputStream<T5>.NextResult,
        OutputStream<T6>.NextResult
      ) result)
    {
      AssertResult(result.Item1);
      AssertResult(result.Item2);
      AssertResult(result.Item3);
      AssertResult(result.Item4);
      AssertResult(result.Item5);
      AssertResult(result.Item6);
    }

    protected void AssertResult<T1, T2, T3, T4, T5, T6, T7>(
      (
        OutputStream<T1>.NextResult,
        OutputStream<T2>.NextResult,
        OutputStream<T3>.NextResult,
        OutputStream<T4>.NextResult,
        OutputStream<T5>.NextResult,
        OutputStream<T6>.NextResult,
        OutputStream<T7>.NextResult
      ) result)
    {
      AssertResult(result.Item1);
      AssertResult(result.Item2);
      AssertResult(result.Item3);
      AssertResult(result.Item4);
      AssertResult(result.Item5);
      AssertResult(result.Item6);
      AssertResult(result.Item7);
    }

    protected void AssertResult<T1, T2, T3, T4, T5, T6, T7, T8>(
      (
        OutputStream<T1>.NextResult,
        OutputStream<T2>.NextResult,
        OutputStream<T3>.NextResult,
        OutputStream<T4>.NextResult,
        OutputStream<T5>.NextResult,
        OutputStream<T6>.NextResult,
        OutputStream<T7>.NextResult,
        OutputStream<T8>.NextResult
      ) result)
    {
      AssertResult(result.Item1);
      AssertResult(result.Item2);
      AssertResult(result.Item3);
      AssertResult(result.Item4);
      AssertResult(result.Item5);
      AssertResult(result.Item6);
      AssertResult(result.Item7);
      AssertResult(result.Item8);
    }

    /*주석 추가함*/
    //멀티 스레드 비동기 연산 작업(Task)들을 동시 대기하고 
    //처리 결과를 튜플 구조로 안전하게 묶어주는 비동기 유틸리티 메서드셋(whenAll 공통)
    protected async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> task1, Task<T2> task2)
    {
      await Task.WhenAll(task1, task2);
      return (task1.Result, task2.Result);
    }

    protected async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
    {
      await Task.WhenAll(task1, task2, task3);
      return (task1.Result, task2.Result, task3.Result);
    }

    protected async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
    {
      await Task.WhenAll(task1, task2, task3, task4);
      return (task1.Result, task2.Result, task3.Result, task4.Result);
    }

    protected async Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
    {
      await Task.WhenAll(task1, task2, task3, task4, task5);
      return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
    {
      await Task.WhenAll(task1, task2, task3, task4, task5, task6);
      return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7)
    {
      await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7);
      return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result);
    }

    protected async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8)
    {
      await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8);
      return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result);
    }

    /*주석 추가함*/
    // 고해상도 스톱워치 측정값을 기준으로 현재 하드웨어의 실행 경과 시간 타임스탬프를 마이크로초 단위로 계산
    protected long GetCurrentTimestampMicrosec()
    {
      return _stopwatch == null || !_stopwatch.IsRunning ? -1 : _stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);
    }

    /*주석 추가함*/
    // 텍스트 파일 세팅 검증을 수행하고, 
    // 감지된 하드웨어 사양(CPU/GPU)에 의거해 코어 런타임 계산 그래프 인스턴스를 동적 생성 및 빌드
    protected void InitializeCalculatorGraph()
    {
      calculatorGraph = new CalculatorGraph();
      _NameTable.Add(calculatorGraph.mpPtr, GetInstanceID());

      // NOTE: There's a simpler way to initialize CalculatorGraph.
      //
      //     calculatorGraph = new CalculatorGraph(config.text);
      //
      //   However, if the config format is invalid, this code does not initialize CalculatorGraph and does not throw exceptions either.
      //   The problem is that if you call ObserveStreamOutput in this state, the program will crash.
      //   The following code is not very efficient, but it will return Non-OK status when an invalid configuration is given.
      var baseConfig = textConfig == null ? null : CalculatorGraphConfig.Parser.ParseFromTextFormat(textConfig.text);
      if (baseConfig == null)
      {
        throw new InvalidOperationException("Failed to get the text config. Check if the config is set to GraphRunner");
      }
      ConfigureCalculatorGraph(baseConfig);

      if (inferenceMode != InferenceMode.CPU)
      {
        calculatorGraph.SetGpuResources(GpuManager.GpuResources); // GPU 모드일 때 하드웨어 가속 리소스 수동 연결
      }
    }

    /// <summary>
    ///   Configure and initialize the <see cref="CalculatorGraph" />.
    /// </summary>
    /// <remarks>
    ///   This is the main process in <see cref="InitializeCalculatorGraph" />.<br />
    ///   At least, <c>calculatorGraph.Initialize</c> must be called here.
    ///   In addition to that, <see cref="OutputStream" /> instances should be initialized.
    /// </remarks>
    /// <param name="config">
    ///   A <see cref="CalculatorGraphConfig" /> instance corresponding to <see cref="textConfig" />.<br />
    ///   It can be dynamically modified here.
    /// </param>
  
    /*주석 추가함*/
    // 파싱된 프로토콜 버퍼 설정을 기반으로 네이티브 CalculatorGraph 인스턴스를 최종 초기화 처리
    protected virtual void ConfigureCalculatorGraph(CalculatorGraphConfig config)
    {
      calculatorGraph.Initialize(config);
    }

    /*주석 추가함*/
    // 유니티 렌더링 좌표계와 MediaPipe 전용 이미지 좌표계의 모순을 맞추기 위해, 
    // 화면 회전값 및 좌우/상하 반전 플래그를 정밀 매칭 및 동적 변환하여 주입하는 핵심 연산 메서드
    protected void SetImageTransformationOptions(PacketMap sidePacket, ImageSource imageSource, bool expectedToBeMirrored = false)
    {
      // NOTE: The origin is left-bottom corner in Unity, and right-top corner in MediaPipe.
      rotation = imageSource.rotation.Reverse();
      var inputRotation = rotation;
      var isInverted = CoordinateSystem.ImageCoordinate.IsInverted(rotation);
      var shouldBeMirrored = imageSource.isHorizontallyFlipped ^ expectedToBeMirrored;
      var inputHorizontallyFlipped = isInverted ^ shouldBeMirrored;
      var inputVerticallyFlipped = !isInverted;

      if ((inputHorizontallyFlipped && inputVerticallyFlipped) || rotation == RotationAngle.Rotation180)
      {
        inputRotation = inputRotation.Add(RotationAngle.Rotation180);
        inputHorizontallyFlipped = !inputHorizontallyFlipped;
        inputVerticallyFlipped = !inputVerticallyFlipped;
      }

      Debug.Log($"input_rotation = {inputRotation}, input_horizontally_flipped = {inputHorizontallyFlipped}, input_vertically_flipped = {inputVerticallyFlipped}");

      sidePacket.Emplace("input_rotation", Packet.CreateInt((int)inputRotation));
      sidePacket.Emplace("input_horizontally_flipped", Packet.CreateBool(inputHorizontallyFlipped));
      sidePacket.Emplace("input_vertically_flipped", Packet.CreateBool(inputVerticallyFlipped));
    }

    /*주석 추가함*/
    // 비동기 파일 로딩 중 특정 에셋 데이터 준비 상태를 
    // 실시간 체크 및 핸들링하기 위한 비동기 연동 대기 처리 메서드셋(waitForResult 공통)
    protected WaitForResult WaitForAsset(string assetName, string uniqueKey, long timeoutMillisec, bool overwrite = false)
    {
      return new WaitForResult(this, AssetLoader.PrepareAssetAsync(assetName, uniqueKey, overwrite), timeoutMillisec);
    }

    protected WaitForResult WaitForAsset(string assetName, long timeoutMillisec, bool overwrite = false)
    {
      return WaitForAsset(assetName, assetName, timeoutMillisec, overwrite);
    }

    protected WaitForResult WaitForAsset(string assetName, string uniqueKey, bool overwrite = false)
    {
      return new WaitForResult(this, AssetLoader.PrepareAssetAsync(assetName, uniqueKey, overwrite));
    }

    protected WaitForResult WaitForAsset(string assetName, bool overwrite = false)
    {
      return WaitForAsset(assetName, assetName, overwrite);
    }

    /*주석 추가함*/
    // 자식 그래프(예: 손 인식, 얼굴 인식 등)들이 실행 시 요구할 독립적 의존성 에셋 리스트 정의용 추상 메서드
    protected abstract IList<WaitForResult> RequestDependentAssets();
  }
}
