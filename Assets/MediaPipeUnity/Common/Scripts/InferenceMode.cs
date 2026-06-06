// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Mediapipe.Unity
{
  [System.Serializable]
  /*주석 추가함*/
  // MediaPipe 심층 신경망(AI 구조)의 추론 연산을 처리할 타겟 하드웨어 칩셋 아키텍처를 정의하는 열거형
  public enum InferenceMode
  {
  // 그래픽 카드의 대규모 병렬 연산 코어를 사용하여 실시간 트래킹 프레임을 고속으로 추론하는 하드웨어 가속 모드
    GPU,
    // 메인 프로세서의 논리 연산 코어를 사용하여 AI 추론을 실행하는 모드로, 
    // GPU 가속이 불가능한 플랫폼에서의 안정적인 범용 우회(Fallback) 모드
    CPU,
  }
}
