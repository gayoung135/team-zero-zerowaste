// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Mediapipe.Unity.Sample.UI
{
  /*주석 추가함*/
  // 다양한 종류의 서브 설정 창 패널들(이미지 소스 설정, 관절 추적 콘필그 등)이 공통으로 상속받는 기저 부모(Base) 클래스
  // 모달 뼈대 인터페이스와의 통신 창구 및 UI 컴포넌트 자동화 유틸리티를 제공
  public class ModalContents : MonoBehaviour
  {
    /*주석 추가함*/
    //  런타임에 이 콘텐츠 패널을 컴포넌트로 품고 있는 직속 부모 오브젝트의 'Modal' 스크립트 레퍼런스를 
    //  역산하여 인출
    protected Modal GetModal()
    {
      return gameObject.transform.parent.gameObject.GetComponent<Modal>();
    }

    /*주석 추가함*/
    // 설정 창 내의 닫기 혹은 X 버튼과 연결되는 가상 메서드 
    // 기본적으로 부모 모달에게 자신을 파괴하고 화면을 닫으라는 명령을 전달
    // 자식 클래스에서 세부 제어(변경 사항 전파 등)가 필요할 경우 'override'하여 커스텀 확장
    public virtual void Exit()
    {
      GetModal().Close();
    }

    /*주석 추가함*/
    //  C# 제네릭(<T>)과 링크(Linq) 기능을 융합하여, 다양한 형태의 Enum(열거형) 데이터를 
    // 유니티 UI 드롭다운 옵션 목록으로 자동 변환 및 초기 세팅해 주는 고성능 유틸리티 함수
    protected void InitializeDropdown<T>(Dropdown dropdown, string defaultValue) where T : Enum
    {
      dropdown.ClearOptions(); // 드롭다운에 남아있던 이전 임시 가비지 항목들 청소

      // 주입된 Enum(T) 구조체의 실제 값 전체를 스캔한 뒤, 각 열거형에 매핑된 한글/영어 설명 데이터(GetDescription)를 
      // 문자열 리스트로 일괄 추출 및 컨버팅
      var options = Enum.GetValues(typeof(T)).Cast<T>().Select(x => x.GetDescription()).ToList();
      dropdown.AddOptions(options); // 추출된 텍스트 목록을 유니티 드롭다운의 실제 데이터 옵션 리스트로 주입

      // 셋업된 옵션 목록 중에서 현재 백엔드 시스템에 세팅되어 있는 기본값(defaultValue) 문자열이 몇 번째 인덱스에 위치하는지 정밀 서칭
      var defaultValueIndex = options.FindIndex(option => option == defaultValue);

      if (defaultValueIndex >= 0) // 유효한 인덱스 위치를 정상 포획했을 경우
      {
        dropdown.value = defaultValueIndex; // 유니티 UI 화면상에 해당 항목이 선택된 상태로 드로잉 세팅
      }
    }
  }
}