// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Unity.Sample.UI
{
  /*주석 추가함*/
  // 유니티 내장 UI 버튼 컴포넌트와 연동되어, 클릭 이벤트 발생 시 지정된 타겟 모달 창을 띄우고 
  // 내부 설정 콘텐츠를 동적으로 주입하는 트리거(스위치) 스크립트
  public class ModalButton : MonoBehaviour
  {
    // 화면에 띄울 부모 모달 컨테이너 팝업 창 오브젝트 레퍼런스
    [SerializeField] private GameObject _modalPanel;
    // 해당 버튼을 눌렀을 때 모달 내부에 새로 팝업되어 채워질 실제 UI 내용물(ex: 이미지 소스 설정, 핸드 랜드마크 설정 패널 등) 프리팹
    [SerializeField] private GameObject _contents;

    /*주석 추가함*/
    // 타겟 모달 오브젝트로부터 실질적인 제어 기능을 담당하는 'Modal' 컴포넌트를 C# 프로퍼티 문법으로 동적 참조
    private Modal modal => _modalPanel.GetComponent<Modal>();

    /*주석 추가함*/
    // 버튼 클릭 시 백엔드 AI 연산 스트림을 멈추지 않고, 단순히 설정 UI 화면만 오버레이하여 띄울 때 호출하는 래퍼 메서드
    public void Open()
    {
      if (_contents != null)
      {
        modal.Open(_contents); // 모달 매니저의 인스턴싱 오픈 파이프라인 가동
      }
    }

    /*주석 추가함*/
    //  버튼 클릭 시 UI 내용물을 생성함과 동시에, 
    //  조작 중 자원 낭비를 방지하기 위해 MediaPipe 연산 스레드를 동결 상태로 전환시키는 오픈 메서드
    public void OpenAndPause()
    {
      if (_contents != null)
      {
        modal.OpenAndPause(_contents); // 모달 매니저의 엔진 일시정지형 오픈 파이프라인 가동
      }
    }
  }
}