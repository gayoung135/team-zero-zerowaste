// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.ComponentModel;

namespace Mediapipe.Unity.Sample
{
  /*주석 추가함*/
  // C#의 모든 열거형(Enum) 데이터에 편의 기능을 추가해 주는 정적(Static) 확장 클래스
  public static class EnumExtension
  {
    /*주석 추가함*/
    //열거형(Enum) 내부의 속성 데이터를 역추적하여 텍스트 설명(Description)을 추출하는 확장 메서드
    public static string GetDescription(this Enum value)
    {
      // 대상 인스턴스의 런타임 형식(Type) 구조를 가져옴.
      var type = value.GetType();
      // 해당 타입 내에서 현재 값과 일치하는 필드의 식별자 이름을 문자열로 획득함.
      var name = Enum.GetName(type, value);
      if (name == null)
      {
        return null;
      }
      // 메타데이터 리플렉션(Reflection)을 이용해 해당 필드의 상세 정보 객체를 탐색
      var field = type.GetField(name);
      if (field == null)
      {
        return null;
      }

      // 필드에 부여된 커스텀 어트리뷰트(Attribute) 중 'DescriptionAttribute'를 검색
      var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
      // 해당 속성이 존재하면 기획 단계에서 명시한 설명 문구(Description) 문자열을 최종 반환
      if (attr is DescriptionAttribute descriptionAttribute)
      {
        return descriptionAttribute.Description;
      }
      // 어트리뷰트가 없을 경우 예외 처리를 위해 시스템 내부 변수명(Name)을 그대로 반환
      return name;
    }
  }
}
