// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Mediapipe.Unity.Sample.UI
{
#pragma warning disable IDE0065
  /*주석 추가함*/
  // 유니티 내장 클래스와 네임스페이스가 겹치는 모호성을 해소하기 위해 컬러 구조체의 출처를 래핑 명시
  using Color = UnityEngine.Color;
#pragma warning restore IDE0065

  /*주석 추가함*/
  // ModalContents.cs를 상속받아 프로젝트 내부의 다양한 AI 데모 씬(손 추적, 얼굴 추적 등) 목록을 
  // 메뉴 버튼 형태로 스크린에 자동 전개하는 메인 로비 메뉴 관리 스크립트
  public class SolutionMenu : ModalContents
  {
    // 한 행(Row)에 2개의 가로 배치 버튼을 품고 있는 템플릿 레이아웃 프리팹
    [SerializeField] private GameObject _solutionRowPrefab;

    // 메뉴 버튼 행 프리팹들이 아래로 연쇄 누적 생성될 스크롤 뷰 컨테이너의 절대 경로 상수
    private const string _GridPath = "Scroll View/Viewport/Contents/Solution Grid";

    private Transform _solutionGrid; // 생성된 레이아웃 행 오브젝트들을 자식으로 수용할 그리드 부모 폼 트랜스폼

    private void Start()
    {
      // 계층 트리 탐색을 통해 버튼이 생성 및 적재될 최종 그리드 캔버스 위치를 확정
      _solutionGrid = transform.Find(_GridPath);

      // 프로젝트 Build Settings 플랫폼 맵에 포함 등록된 총 씬의 개수를 런타임에 동적 추출
      var solutionCount = SceneManager.sceneCountInBuildSettings;
      Transform currentRow = null;

      // 등록된 씬 개수만큼 반복하며 메뉴 선택 단추를 동적으로 바인딩하고 레이아웃을 자동 증축
      for (var i = 0; i < solutionCount; i++)
      {
        // 2의 배수 인덱스 시점마다(0, 2, 4...) 새로운 가로 행 프리팹을 스폰하여 아래로 누적 배치
        if (i % 2 == 0)
        {
          // 한 줄에 2개의 버튼을 세팅하기 위한 행 컴포넌트 프리팹 런타임 인스턴싱 가동
          currentRow = InitializeRow();
        }
        
        // 생성된 현재 행 트랜스폼 구조에서 왼쪽(0) 또는 오른쪽(1)에 위치한 버튼 객체 레퍼런스를 인출
        var button = GetButtonInRow(currentRow, i % 2);

        var buildIndex = i; // 클로저(Closure) 현상으로 인한 인덱스 참조 꼬임 방지를 위해 로컬 빌드 스냅샷 변수를 고정 캐싱
        
        // 유니티 에셋 패스로부터 확장자를 제외한 순수 씬 파일명을 추출하여 버튼 텍스트 레이블에 매핑 주입
        button.transform.GetComponentInChildren<Text>().text = GetSceneNameByBuildIndex(buildIndex);
        
        // 사용자가 메뉴 버튼을 터치/클릭하는 순간 해당 빌드 인덱스의 씬으로 화면 전환을 유발하는 비동기 코루틴 이벤트를 구독 연동
        button.onClick.AddListener(() => { var _ = StartCoroutine(LoadSceneAsync(buildIndex)); });
      }

      // 빌드 세팅된 총 씬의 개수가 홀수일 경우, 
      // 마지막 행의 우측(1번 인덱스)에 남겨진 공백 가비지 버튼을 화면에서 완전 은폐 처리
      if (solutionCount % 2 == 1)
      {
        var unusedButton = GetButtonInRow(currentRow, 1);
        HideButton(unusedButton);
      }
    }

    /*주석 추가함*/
    //  사용자가 취소나 창 닫기 선택 시, 
    // 상위 모달 베이스 프레임의 Close 제어권을 호출하여 안전하게 화면을 해제
    public override void Exit()
    {
      GetModal().CloseAndResume();
    }

    /*주석 추가함*/
    // 스크롤 뷰 컨텐츠 레이아웃 하위 자식 노드로 가로형 행(Row) 오브젝트를 할당 생성하고 트랜스폼 주소를 반환
    private Transform InitializeRow()
    {
      return Instantiate(_solutionRowPrefab, _solutionGrid).transform;
    }

    /*주석 추가함*/
    // 부모 가로 행 트랜스폼 레이어 구조 내부에서 좌(0번째 자식)/우(1번째 자식) 계층 요소를 탐색하여 
    // 유니티 UI Button 컴포넌트를 정밀 반환
    private Button GetButtonInRow(Transform row, int index)
    {
      var child = row.GetChild(index);
      return (child == null || child.gameObject == null) ? null : child.gameObject.GetComponent<Button>();
    }

    /*주석 추가함*/
    // 홀수 배치로 인해 미사용 상태로 방치된 짝꿍 버튼의 투명도(Alpha)를 
    // 0으로 강제 소거하고 내부 가짜 글씨를 지워 UI 레이아웃 정렬을 완벽하게 수호
    private void HideButton(Button button)
    {
      var image = button.gameObject.GetComponent<UnityEngine.UI.Image>();
      image.color = new Color(0, 0, 0, 0); // 완전 투명 투사 처리
      button.transform.GetComponentInChildren<Text>().text = null; // 텍스트 찌꺼기 제거
    }

    /*주석 추가함*/
    // 빌드 인덱스 경로를 추적하여 추출된 문자열에서 
    // 디렉토리 주소와 .unity 확장자를 잘라내고 순수 씬 이름만 정제 인출
    private string GetSceneNameByBuildIndex(int buildIndex)
    {
      var path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
      return Path.GetFileNameWithoutExtension(path); // ex: "Assets/Scenes/HandTracking.unity" -> "HandTracking" 정제
    }

    /*주석 추가함*/
    // 새로운 AI 데모 씬 전환 시 메인 프레임이 얼어붙는 현상을 방지하기 위해 
    // 유니티 비동기 오퍼레이션(`LoadSceneAsync`)을 통해 무중단으로 다음 씬 로드를 완
    private IEnumerator LoadSceneAsync(int buildIndex)
    {
      var sceneLoadReq = SceneManager.LoadSceneAsync(buildIndex);
      yield return new WaitUntil(() => sceneLoadReq.isDone); // 백그라운드 로딩 가동율이 100%에 도달해 완결될 때까지 코루틴 지연 무한 대기
    }
  }
}