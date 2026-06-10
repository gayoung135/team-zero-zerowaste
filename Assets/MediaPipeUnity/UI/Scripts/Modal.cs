// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Unity.Sample.UI
{
  /*주석 추가함*/
  // 인게임 설정이나 팝업 UI 창을 중앙 캔버스 상에 띄우고, 
  // 창의 개폐 상태에 매핑하여 백엔드 AI 연산 러너(MediaPipe BaseRunner)를 동적으로 일시정지/재개시키는 UI 프레임워크 모달 스크립트
  public class Modal : MonoBehaviour
  {
    // 모달 활성화 연동 시 실시간 동결/해제 명령을 내릴 대상인 미디어파이프 베이스 런너 엔티티
    [SerializeField] private BaseRunner _baseRunner;

    private GameObject _contents; // 현재 모달 창 내부에 동적으로 생성되어 채워져 있는 실제 UI 컨텐츠(설정 창 등) 프리팹 인스턴스 저장 변수

    /*주석 추가함*/
    // 원하는 콘텐트 UI 요소를 내부에 주입(인스턴스화)하고 모달 오브젝트를 렌더링 활성화
    public void Open(GameObject contents)
    {
      _contents = Instantiate(contents, gameObject.transform); // 내 자식 노드 레벨로 설정 창 프리팹을 동적 생성
      _contents.transform.localScale = new Vector3(0.8f, 0.8f, 1); // 자연스러운 팝업 연출을 위한 80% 비율 스케일 미세 보정 적용
      gameObject.SetActive(true); // 모달 오브젝트 가시화 시동
    }

    /*주석 추가함*/
    // 팝업 창을 화면에 그림과 동시에, 
    // UI 조작 중 불필요한 AI 연산 과부하가 생기지 않도록 백엔드 추론 연산을 안전하게 일시정지
    public void OpenAndPause(GameObject contents)
    {
      Open(contents); // 기본 생성 및 오픈 처리 호출
      if (_baseRunner != null)
      {
        _baseRunner.Pause(); // 미디어파이프 백엔드 연산 스레드 즉각 동결 명령 유발
      }
    }

    /*주석 추가함*/
    // 모달 창을 비활성화하고, 자식으로 붙어있던 가비지 UI 컨텐츠를 메모리에서 완전 소멸(Destroy)
    public void Close()
    {
      gameObject.SetActive(false); // 팝업 시각 레이어 감추기

      if (_contents != null)
      {
        Destroy(_contents); // 런타임 메모리 청소를 위해 인스턴스화되었던 서브 UI 객체 영구 제거
      }
    }

    /*주석 추가함*/
    // 팝업 창을 해제함과 동시에, 설정 변경 여부 변수(`forceRestart`)를 수신하여 AI 파이프라인의 안전한 복구 또는 콜드 부팅을 가동
    public void CloseAndResume(bool forceRestart = false)
    {
      Close(); // UI 패널 오브젝트 및 내부 가비지 메모리 청소 선행

      if (_baseRunner == null)
      {
        return;
      }

      // 만약 사용자가 해상도나 카메라 장치 등 중대한 파라미터를 교체하여 강제 재부팅이 성립된 경우
      if (forceRestart)
      {
        if (_baseRunner != null)
        {
          _baseRunner.Play(); // 미디어파이프 초기화 파이프라인 프로세스 단계부터 완전히 새롭게 재실행(Cold Boot)
        }
      }
      // 단순히 수치 설정만 변경했거나 아무것도 바꾸지 않고 취소하여 창을 닫은 경우
      else
      {
        if (_baseRunner != null)
        {
          _baseRunner.Resume(); // 멈춰두었던 이전 시점의 AI 카메라 추론 상태를 그대로 이어서 진행(Warm Resume)
        }
      }
    }
  }
}