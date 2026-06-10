// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using UnityEngine;

namespace Mediapipe.Unity.Sample
{
  /*주석 추가함*/
  // 게임이 실행될 때 가장 먼저 작동하여 MediaPipe AI 연동에 필요한
  // 모든 초기 설정(로그, 자원, GPU, 입력 소스)을 순서대로 부팅해 주는 마스터 스크립트
  public class Bootstrap : MonoBehaviour
  {
    [SerializeField] private AppSettings _appSettings; //앞서 정의한 환경 설정 에셋 파일

    public InferenceMode inferenceMode { get; private set; } //최종 결정된 추론 모드 (CPU 혹은 GPU)
    public bool isFinished { get; private set; } //모든 부팅/초기화 과정이 완료되었는지 여부
    private bool _isGlogInitialized; //로그 시스템이 성공적으로 켜졌는지 기록하는 플래그

    /*주석 추가함*/
    //유니티 오브젝트가 활성화될 때 자동으로 실행되는 메서드
    private void OnEnable()
    {
      var _ = StartCoroutine(Init());
    }

    /*주석 추가함*/
    //전반적인 시스템 초기화를 단계별로 실행하는 메인 메서드
    //로그 레벨 설정 -> 시스템 플래그 초기화 -> 리소스 로더 등록 -> 연산 모드 결정 -> 카메라 세팅 순으로 진행
    private IEnumerator Init()
    {
      Debug.Log("The configuration for the sample app can be modified using AppSettings.asset.");
#if !DEBUG && !DEVELOPMENT_BUILD
      Debug.LogWarning("Logging for the MediaPipeUnityPlugin will be suppressed. To enable logging, please check the 'Development Build' option and build.");
#endif
      // 1. 로그 시스템 세팅
      Logger.MinLogLevel = _appSettings.logLevel;

      Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);

      Debug.Log("Setting global flags...");
      _appSettings.ResetGlogFlags();
      Glog.Initialize("MediaPipeUnityPlugin");
      _isGlogInitialized = true;

      // 2. 설정된 타입에 따라 자원 관리 매니저(AssetLoader)에 실제 매니저 객체 주입
      Debug.Log("Initializing AssetLoader...");
      switch (_appSettings.assetLoaderType)
      {
        case AppSettings.AssetLoaderType.AssetBundle:
          {
            AssetLoader.Provide(new AssetBundleResourceManager("mediapipe"));
            break;
          }
        case AppSettings.AssetLoaderType.StreamingAssets:
          {
            AssetLoader.Provide(new StreamingAssetsResourceManager());
            break;
          }
        case AppSettings.AssetLoaderType.Local:
          {
#if UNITY_EDITOR
            AssetLoader.Provide(new LocalResourceManager());
            break;
#else
            Debug.LogError("LocalResourceManager is only supported on UnityEditor." +
              "To avoid this error, consider switching to the StreamingAssetsResourceManager and copying the required resources under StreamingAssets, for example.");
            yield break;
#endif
          }
        default:
          {
            Debug.LogError($"AssetLoaderType is unknown: {_appSettings.assetLoaderType}");
            yield break;
          }
      }

      // 3. 연산 모드(CPU/GPU)를 결정하고 GPU 모드일 경우 하드웨어 가속 리소스 초기화
      DecideInferenceMode();
      if (inferenceMode == InferenceMode.GPU)
      {
        Debug.Log("Initializing GPU resources...");
        yield return GpuManager.Initialize();

        if (!GpuManager.IsInitialized)
        {
          Debug.LogWarning("If your native library is built for CPU, change 'Preferable Inference Mode' to CPU from the Inspector Window for AppSettings");
        }
      }

      // 4. 입력 장치(웹캠, 비디오, 이미지)의 공급자를 초기화하고 기본 카메라 소스로 전환
      Debug.Log("Preparing ImageSource...");
      ImageSourceProvider.Initialize(
        _appSettings.BuildWebCamSource(), _appSettings.BuildStaticImageSource(), _appSettings.BuildVideoSource());
      ImageSourceProvider.Switch(_appSettings.defaultImageSource);

      // 5. 모든 부팅 과정 완료 처리
      isFinished = true;
    }

  /*주석 추가함*/
  //현재 게임이 실행 중인 플랫폼 환경(OS)에 맞춰 AI 연산을 CPU로 돌릴지, GPU로 돌릴지 최종 판정하는 메서드
  //이때,윈도우/맥 유니티 에디터 환경에서는 안전성을 위해 GPU 설정을 강제로 CPU 모드로 우회함.
    private void DecideInferenceMode()
    {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
      if (_appSettings.preferableInferenceMode == InferenceMode.GPU) {
        Debug.LogWarning("Current platform does not support GPU inference mode, so falling back to CPU mode");
      }
      inferenceMode = InferenceMode.CPU;
#else
      inferenceMode = _appSettings.preferableInferenceMode;
#endif
    }
    /*주석 추가함*/
    // 게임(애플리케이션)이 완전히 종료될 때 호출되는 메서드
    // 켜져 있던 GPU 자원 및 시스템 로깅 시스템을 안전하게 차단(Shutdown)하여 메모리 누수를 방지
    private void OnApplicationQuit()
    {
      GpuManager.Shutdown();

      if (_isGlogInitialized)
      {
        Glog.Shutdown();
      }

      Protobuf.ResetLogHandler();
    }
  }
}
