// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
  /*주석 추가함*/
  // VisionTaskApiRunner를 상속받아 실시간 카메라 스트리밍 프레임 버퍼를 미디어파이프 네이티브 AI 모델에 주입하고, 
  // 도출된 관절 좌표를 인게임 핸드 매니저로 포워딩하는 최종 실행 스크립트
  public class HandLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
  {
    [SerializeField] private HandLandmarkerResultAnnotationController _handLandmarkerResultAnnotationController; // 화면상에 21개 손 관절 기하학 스켈레톤 라인을 그려주는 시각화 렌더 제어 컨트롤러입니다.
    private Experimental.TextureFramePool _textureFramePool; // 매 프레임 발생하는 가비지 컬렉터(GC) 부하를 방지하기 위해 텍스처 가상 메모리를 재사용 큐 형식으로 관리하는 메모리 풀입니다.
    public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig(); // 20번 스크립트에서 선언한 추론 신뢰도 및 작동 옵션 환경설정 데이터 인스턴스입니다.

    /*주석 추가함*/
    // 파이프라인 중단 시 부모의 리소스 해제 연산을 수행하고, 
    // 렌더링 스레드 단에 묶여있던 텍스처 프레임 풀 메모리를 영구 폐기
    public override void Stop()
    {
      base.Stop();
      _textureFramePool?.Dispose();
      _textureFramePool = null;
    }

    /*주석 추가함*/
    // 비동기 데이터 셋업, 이미지 소스 캘리브레이션 및 프레임 리드 모드별 분기 처리를 영구 반복 수행하는 메인 코루틴
    protected override IEnumerator Run()
    {
      // 현재 바인딩된 시스템 콘필그 매개변수를 초기 구동 시 디버그 로그에 전사 출력
      Debug.Log($"Delegate = {config.Delegate}");
      Debug.Log($"Image Read Mode = {config.ImageReadMode}");
      Debug.Log($"Running Mode = {config.RunningMode}");
      Debug.Log($"NumHands = {config.NumHands}");
      Debug.Log($"MinHandDetectionConfidence = {config.MinHandDetectionConfidence}");
      Debug.Log($"MinHandPresenceConfidence = {config.MinHandPresenceConfidence}");
      Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");

      // 에셋로더 시스템을 호출하여 셋업된 경로 상의 손 인식 딥러닝 신경망 가중치 바이너리(.bytes)를 비동기 에셋 로딩
      yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

      // 라이브 스트림 구동 방식일 경우 백엔드 추론이 끝나는 순간 비동기로 호출될 콜백 메서드(`OnHandLandmarkDetectionOutput`)를 결합 바인딩
      var options = config.GetHandLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
      taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources); // 네이티브 C++ 미디어파이프 핸드 랜드마커 객체 인스턴싱
      var imageSource = ImageSourceProvider.ImageSource; // 현재 씬에 활성화된 카메라(웹캠/비디오) 소스 룩업

      yield return imageSource.Play(); // 카메라 캡처 하드웨어 디바이스 가동 시작

      if (!imageSource.isPrepared) // 카메라 소스가 정상 작동하지 않을 경우 예외 탈출 처리
      {
        Debug.LogError("Failed to start ImageSource, exiting...");
        yield break;
      }

      /*주석 추가함*/
      // 메모리 오버헤드를 막기 위해 카메라 원본 해상도 규격을 수용하는 32비트 고정 RGBA 텍스처 버퍼 캐싱 풀을 10개 사이즈로 사전 할당
      _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

      screen.Initialize(imageSource); // 출력 디스플레이 해상도 화면 비율 정렬 가동

      SetupAnnotationController(_handLandmarkerResultAnnotationController, imageSource); // 시각화 오버레이 캔버스 좌표 동기화

      /*주석 추가함*/
      // 카메라 디바이스의 하드웨어 회전값 및 좌우/상하 반전 여부 메타데이터를 추출하여 이미지 프로세싱 보정 옵션 구조체에 이식
      var transformationOptions = imageSource.GetTransformationOptions();
      var flipHorizontally = transformationOptions.flipHorizontally;
      var flipVertically = transformationOptions.flipVertically;
      var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

      AsyncGPUReadbackRequest req = default; // GPU 메모리를 비동기 판독하기 위한 요청 구조체
      var waitUntilReqDone = new WaitUntil(() => req.done); // 비동기 GPU 하드웨어 복사가 완결될 때까지 코루틴을 블로킹 대기시키는 명령어
      var waitForEndOfFrame = new WaitForEndOfFrame(); // 유니티 내부 렌더링 스케줄러가 한 프레임의 드로우핏을 끝마칠 때까지 대기하는 명령어
      var result = HandLandmarkerResult.Alloc(options.numHands); // 동적 할당 가비지를 줄이기 위해 최대 손 개수 크기만큼 추론 결과물 구조체를 사전 메모리 할당

      /*주석 추가함*/
      // 안드로이드 환경이면서 OpenGLES3 그래픽스 API 기반일 경우 GPU 네이티브 텍스처 자원을 CPU 복사 없이 다이렉트 쉐어링하기 위한 컨텍스트 검증
      var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && GpuManager.GpuResources != null;
      using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

      while (true)
      {
        if (isPaused) // 상위 시스템 레이어에서 일시정지 명령 인입 시 루틴 동결 처리
        {
          yield return new WaitWhile(() => isPaused);
        }

        // 재사용 메모리 풀에서 놀고 있는 가용 프레임 버퍼 인스턴스 인출 프로세스 수행 (풀이 고갈되었을 시 프레임 스킵 가드)
        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          yield return waitForEndOfFrame;
          continue;
        }

        /*주석 추가함*/
        // 셋업된 ImageReadMode 정책에 맞춰 물리 하드웨어 버퍼를 미디어파이프 전용 인풋 Image 포맷으로 가공
        Image image;
        switch (config.ImageReadMode)
        {
          case ImageReadMode.GPU: // 완전 네이티브 GPU 연산 모드 분기 (안드로이드 특정 그래픽스 환경 한정 지원)
            if (!canUseGpuImage)
            {
              throw new System.Exception("ImageReadMode.GPU is not supported");
            }
            textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildGPUImage(glContext);
            yield return waitForEndOfFrame; // 텍스처 버퍼 동기화가 완결될 때까지 렌더 스레드 안전 대기 주입
            break;
            
          case ImageReadMode.CPU: // 전통적인 동기식 CPU 텍스처 판독 분기 (메인 렌더링 스레드 병목 유발 가능성 존재)
            yield return waitForEndOfFrame;
            textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildCPUImage();
            textureFrame.Release(); // 판독 완결 후 버퍼 락 해제 및 풀 반환
            break;
            
          case ImageReadMode.CPUAsync: // 멀티스레드 기반 비동기 GPU-to-CPU 버퍼 카피 하드웨어 가속 분기 (병목 무력화)
          default:
            req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically); // 하드웨어 단 비동기 복사 커맨드 큐 명령 투하
            yield return waitUntilReqDone; // 복사 트랜잭션이 완료될 때까지 메인 루프 지연 무중단 대기

            if (req.hasError)
            {
              Debug.LogWarning($"Failed to read texture from the image source");
              continue;
            }
            image = textureFrame.BuildCPUImage(); // 비동기 완결된 원시 데이터를 기반으로 추론용 이미지 구조체 래핑
            textureFrame.Release(); // 풀 반환
            break;
        }

        /*주석 추가함*/
        // 생성된 인풋 이미지를 러닝 모드 스케줄링에 맞춰 연산 주입하고 시각화 드로잉을 트리거합니다.
        switch (taskApi.runningMode)
        {
          case Tasks.Vision.Core.RunningMode.IMAGE: // 단일 정적 스틸 이미지 추론 분기 (동기식 즉시 처리)
            if (taskApi.TryDetect(image, imageProcessingOptions, ref result)) _handLandmarkerResultAnnotationController.DrawNow(result);
            else _handLandmarkerResultAnnotationController.DrawNow(default);
            break;
            
          case Tasks.Vision.Core.RunningMode.VIDEO: // 파일 기반 녹화 비디오 프레임 추론 분기 (타임스탬프 동기식 동당 처리)
            if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result)) _handLandmarkerResultAnnotationController.DrawNow(result);
            else _handLandmarkerResultAnnotationController.DrawNow(default);
            break;
            
          case Tasks.Vision.Core.RunningMode.LIVE_STREAM: // 웹캠 및 카메라 실시간 비동기 백엔드 스레드 추론 분기 (결과는 하단의 온백 콜백으로 전사 분리)
            taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions); // 명령 투하 후 즉각 다음 프레임 루프로 전환 (연산 랙 전면 차단)
            break;
        }
      }
    }

    /*주석 추가함*/
    //  MediaPipe 백엔드 백그라운드 스레드에서 특정 프레임의 손 관절 인지가 완결되는 순간 독립적으로 포획되는 수신 인터페이스
    private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
    {
      _handLandmarkerResultAnnotationController.DrawLater(result); // 메인 렌더링 프레임과 동기화하여 화면에 기하학 선 드로우 예약
      
      if (result.handLandmarks == null || result.handLandmarks.Count == 0) return; // 카메라 내에 유효한 손 형체가 포착되지 않은 경우 인게임 데이터 갱신 생략

      // 화면 내에 검출된 손의 개수(최대 2개)만큼 루프를 순회하며 인게임 상호작용에 절대적으로 필요한 5개 핵심 랜드마크 마디 수치만 필터링
      for(int i=0; i<result.handLandmarks.Count; i++)
      {
        var landmarks = result.handLandmarks[i];
        
        var thumbTip = landmarks.landmarks[4]; // 4번: 엄지 손가락 손톱 끝 포인트
        var indexTip = landmarks.landmarks[8]; // 8번: 검지 손가락 손톱 끝 포인트 
        var ringTip = landmarks.landmarks[16]; // 16번: 약지 손가락 끝 포인트 
        var wristTip = landmarks.landmarks[0]; // 0번: 손목 정중앙 기저 포인트 
        var middle = landmarks.landmarks[9];  // 9번: 중지 손가락이 손바닥과 연결되는 MCP 관절 마디 포인트 (물체 쥐었을 때 안착 지점)
      
        // 인게임 상호작용 컨트롤러 싱글톤 인프라 창구에 실시간 좌표(X, Y 정규화 스트림)를 주입 및 동기화 매핑
        HandManager.Instance.UpdateHandData(
          i,
          thumbTip.x, thumbTip.y,
          indexTip.x, indexTip.y,
          ringTip.x, ringTip.y,
          wristTip.x, wristTip.y,
          middle.x, middle.y
        );
      }
    }
  }
}