// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Mediapipe.Unity
{
  [System.Serializable]
  /*주석 추가함*/
  // MediaPipe AI 연산 파이프라인의 데이터 처리 동기화 방식을 정의하는 열거형 상수
  public enum RunningMode
  {
    // 이미지 프레임 입력과 AI 연산 결과 출력을 별도의 독립된 스레드에서 처리하여, 
    // 메인 렌더링 루프의 지연을 차단하는 모드(비동기)
    Async,
    // 입력 프레임에 대한 AI 추론이 완결될 때까지 
    // 다음 프레임 전송 및 메인 루프를 일시 대기시키는 순차적 처리 모드(동기)
    Sync,
  }

  /*주석 추가함*/
  // RunningMode 열거형의 조건부 판별을 지원하는 정적 확장(Extension) 클래스
  public static class RunningModeExtension
  {
    /*주석 추가함*/
    // 현재 지정된 작동 모드가 동기(Synchronous) 모드인지 여부를 논리값(bool)으로 즉시 반환하는 확장 메서드
    public static bool IsSynchronous(this RunningMode runningMode)
    {
      return runningMode == RunningMode.Sync;
    }
  }
}
