// Copyright (c) 2025 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.
using System.ComponentModel;

namespace Mediapipe.Unity
{
  [System.Serializable]
  /*주석 추가함*/
  // 웹캠 장치로부터 이미지 프레임 데이터를 렌더링 파이프라인으로 읽어오는 전송 방식을 정의하는 열거형 상수군
  public enum ImageReadMode
  {
    // 메인 CPU 스레드에서 정형화된 동기(Sync) 방식으로 이미지 픽셀 데이터를 직접 판독하는 모드
    CPU,
    // 메인 렌더링 프레임의 지연(병목 현상)을 방지하기 위해, 
    // CPU 비동기 스레드 풀을 활용해 데이터를 우회 판독하는 모드
    [Description("CPU Async")]
    CPUAsync,
    // 그래픽 카드의 고속 비디오 메모리(VRAM) 상에서 텍스처 버퍼 변환을 직접 실행하는 하드웨어 가속 처리 모드
    GPU,
  }
}
