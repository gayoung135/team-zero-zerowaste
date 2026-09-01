// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Vision.HandLandmarker;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
  /*주석 추가함*/
  // 손 관절 검출(Hand Landmark Detection) 모델 구동에 필요한 
  // 하드웨어 자원 설정 및 AI 추론 신뢰도 파라미터를 통합 관리하는 구성 설정 클래스
  public class HandLandmarkDetectionConfig
  {
    /*주석 추가함*/
    // 운영체제 및 플랫폼 환경에 맞춰 크래시를 방지하기 위해 CPU 또는 GPU 연산 대상을 기본 분기 매핑
    public Tasks.Core.BaseOptions.Delegate Delegate { get; set; } =
   // 호환성 안정을 위해 데스크톱(윈도우/맥) 유니티 에디터 및 빌드 환경에서는 CPU 연산을 기본값으로 채택 
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
      Tasks.Core.BaseOptions.Delegate.CPU;
#else
    // 퍼포먼스 극대화를 위해 모바일(Android/iOS) 및 기타 플랫폼 환경에서는 GPU 하드웨어 가속을 기본값으로 채택
      Tasks.Core.BaseOptions.Delegate.GPU;
#endif
    /*주석 추가함*/
    // 이미지 프레임 판독 모드임. 
    // 메인 렌더링 프레임 드랍을 막기 위해 'CPU 비동기 래핑 모드'를 기본값으로 지정
    public ImageReadMode ImageReadMode { get; set; } = ImageReadMode.CPUAsync;

    /*주석 추가함*/
    // AI 모델의 구동 스트리밍 방식을 설정합니다. 
    // 실시간 웹캠 비디오 입력 처리에 최적화된 'LIVE_STREAM'을 기본 정책으로 설정
    public Tasks.Vision.Core.RunningMode RunningMode { get; set; } = Tasks.Vision.Core.RunningMode.LIVE_STREAM;


    public int NumHands { get; set; } = 2; // 화면 내에서 동시 추적을 허용할 최대 손의 개수입니다. (기본값: 양손인 2개)
    public float MinHandDetectionConfidence { get; set; } = 0.5f; // 최초로 손을 탐지(Detection)해 내기 위한 최소 AI 예측 신뢰도 임계값(0.0 ~ 1.0)
    public float MinHandPresenceConfidence { get; set; } = 0.5f; // 탐지된 사물이 실제 '손'이 맞는지 재검증(Presence)하기 위한 신뢰도 커트라인
    public float MinTrackingConfidence { get; set; } = 0.5f; // 연속적인 프레임 추적(Tracking) 시 관절 포인트를 놓치지 않고 유지하기 위한 최소 신뢰도 임계값
    public string ModelPath => "hand_landmarker.bytes"; // 유니티 StreamingAssets 또는 에셋 레이어에 내장된 실제 신경망 가중치 바이너리 파일명

    /*주석 추가함*/
    // 본 클래스에 세팅된 개별 파라미터 정보들을 취합하여, 
    // 네이티브 MediaPipe API 레이어 주입용 최종 'HandLandmarkerOptions' 인스턴스를 빌드 후 반환
    public HandLandmarkerOptions GetHandLandmarkerOptions(HandLandmarkerOptions.ResultCallback resultCallback = null)
    {
      return new HandLandmarkerOptions(
        new Tasks.Core.BaseOptions(Delegate, modelAssetPath: ModelPath), // 하드웨어 연산 위임 정보 및 모델 바이너리 경로 바인딩
        runningMode: RunningMode, // 작동 방식 주입
        numHands: NumHands, // 최대 탐색 자원 제약 주입
        minHandDetectionConfidence: MinHandDetectionConfidence, // 최초 탐지 임계율 매핑
        minHandPresenceConfidence: MinHandPresenceConfidence, // 존재 검증 임계율 매핑
        minTrackingConfidence: MinTrackingConfidence, // 트래킹 추종 임계율 매핑
        resultCallback: resultCallback // 실시간 결과 패킷을 가로챌 비동기 콜백 함수 수신 인터페이스 등록
      );
    }
  }
}
