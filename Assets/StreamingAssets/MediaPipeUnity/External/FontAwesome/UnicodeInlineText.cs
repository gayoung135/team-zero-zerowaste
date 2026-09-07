// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine.UI;
using System.Text.RegularExpressions;

/*주석 추가함*/
// 유니티 레거시 Text 컴포넌트를 상속받아, 
// 텍스트 문자열 내에 포함된 '\uXXXX' 형태의 유니코드 이스케이프 시퀀스를 
// 실시간 문자 기호로 변환하여 출력하는 커스텀 UI 텍스트 스크립트
public class UnicodeInlineText : Text
{
  /*주석 추가함*/
  // 텍스트 변환 프로세스 도중 텍스트 변경 이벤트가 
  // 역으로 트리거되어 시스템 무한 루프가 발생하는 것을 방지하는 안전 가드 플래그
  private bool _disableDirty = false;
  /*주석 추가함*/
  // 문자열에서 유니코드 패턴('\u' 뒤에 오는 16진수 문장군)을 정밀 필터링 및 캡처하기 위한 
  // 컴파일된 정규표현식 객체
  private readonly Regex _regexp = new Regex(@"\\u(?<Value>[a-zA-Z0-9]+)");

  /*주석 추가함*/
  // UI 그래픽 메쉬(Mesh)를 빌드할 때 호출되는 가상 메서드를 오버라이드하여, 
  // 화면 드로우 직전에 유니코드를 실시간 디코딩 주입
  protected override void OnPopulateMesh(VertexHelper vh)
  {
    var cache = text; // 원본 입력 텍스트 스트링 백업
    _disableDirty = true; // 문자 교체 과정에서 발생할 레이아웃/메테리얼 갱신 무한 루프 차단 가드 가동
    text = Decode(text); // 정규식 기반 유니코드 -> 특수문자 실시간 번역 주입
    base.OnPopulateMesh(vh); // 번역된 문자를 바탕으로 실제 화면 픽셀 메쉬 생성 가동 (부모 UI 렌더러 호출)
    text = cache; // 인스펙터 창 데이터 유지를 위해 원본 코드 문장으로 복구
    _disableDirty = false; // 가드 해제
  }

  /*주석 추가함*/
  // 문자열 내부에 숨겨진 유니코드 패턴 전체를 검색하여 
  // 16진수 수치를 C# 문자형(`char`) 데이터로 완벽하게 복원 및 변환
  private string Decode(string value)
  {
    return _regexp.Replace(value, m => ((char)int.Parse(m.Groups["Value"].Value, System.Globalization.NumberStyles.HexNumber)).ToString());
  }

  /*주석 추가함*/
  // 아래의 Dirty 오버라이드 메서드들은 문자 교체 연산 도중 유니티 엔진이 "UI 구조가 변경되었다"고 
  // 오인하여 연쇄적인 화면 갱신 명령을 폭포수처럼 쏟아내는 병목을 무시

  // 레이아웃 정보 재연산 요청 유입 시 가드 상태에 따라 처리를 유보하거나 부모 인터페이스로 포워딩
  public override void SetLayoutDirty()
  {
    if (_disableDirty)
    {
      return;
    }
    base.SetLayoutDirty();
  }

  /*주석 추가함*/
  // 버텍스(정점) 데이터 재연산 요청 유입 시 가드 상태에 따라 처리를 유보하거나 부모 인터페이스로 포워딩
  public override void SetVerticesDirty()
  {
    if (_disableDirty)
    {
      return;
    }
    base.SetVerticesDirty();
  }

  /*주석 추가함*/
  // UI 그래픽 메테리얼/재질 재연산 요청 유입 시 가드 상태에 따라 처리를 유보하거나 부모 인터페이스로 포워딩
  public override void SetMaterialDirty()
  {
    if (_disableDirty)
    {
      return;
    }
    base.SetMaterialDirty();
  }
}
