// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Mediapipe.Unity.Sample.UI
{
  /*주석 추가함*/
  // 모달 윈도우(ModalContents) 시스템을 상속받아, 
  // 런타임 환경에서 입력 비디오 소스의 타입(웹캠/비디오 파일 등), 장치 종류, 해상도, 좌우 반전 옵션을 실시간 제어하는 UI 컴포넌트
  public class ImageSourceConfig : ModalContents
  {
    /*주석 추가함*/
    // 프리랩 내부에 깊게 중첩된 하위 UI 컴포넌트를 정확히 조인하기 위한 고정 노드 경로 문자열
    private const string _SourcePath = "Menu/Contents/Source/Dropdown";
    private const string _ResolutionPath = "Menu/Contents/Resolution/Dropdown";
    private const string _IsHorizontallyFlippedPath = "Menu/Contents/IsHorizontallyFlipped/Toggle";

    private Dropdown _sourceInput; // 연결된 다중 웹캠 디바이스(내장 카메라, USB 캠 등) 리스트 선택 드롭다운
    private Dropdown _resolutionInput; // 해당 장치가 지원하는 해상도 포맷 목록 선택 드롭다운
    private Toggle _isHorizontallyFlippedInput; // 거울 모드 연출을 위한 가로축 좌우 반전 활성화 토글

    private bool _isChanged; // 사용자가 이미지 소스 사양을 변경하여 백엔드 파이프라인 리부팅이 필요한지 추적하는 갱신 체크 플래그

    private void Start()
    {
      InitializeContents(); // UI 로드 시점에 데이터와 프론트엔드 인터페이스 요소를 최초 맵핑
    }

    /*주석 추가함*/
    // 팝업이 닫힐 때 하드웨어 변경 사항 유무(`_isChanged`)를 부모 모달 매니저에 알려 비디오 파이프라인 재부팅을 유발
    public override void Exit()
    {
      GetModal().CloseAndResume(_isChanged);
    }

    /*주석 추가함*/
    // 소스 타입이 변동되면 지원 하드웨어가 완전히 바뀌므로, 
    // 연쇄적 오류 방지를 위해 하위 4대 옵션 UI 요소를 일제히 재초기화합니다.
    private void InitializeContents()
    {
      InitializeSource();
      InitializeResolution();
      InitializeIsHorizontallyFlipped();
    }

    /*주석 추가함*/
    //선택된 소스 타입 안에서 구동 가능한 실제 장치 하드웨어 이름 목록을 바인딩
    private void InitializeSource()
    {
      _sourceInput = gameObject.transform.Find(_SourcePath).gameObject.GetComponent<Dropdown>();
      _sourceInput.ClearOptions();
      _sourceInput.onValueChanged.RemoveAllListeners();

      var imageSource = ImageSourceProvider.ImageSource;
      var sourceNames = imageSource.sourceCandidateNames; // 현재 장치 시스템에서 스캔된 가용한 물리 하드웨어 이름 배열 스캔

      if (sourceNames == null) // 가용한 후보 장치(웹캠 등)가 존재하지 않는 특수 환경일 경우 UI 상호작용 동결 후 가드 해제
      {
        _sourceInput.enabled = false;
        return;
      }

      var options = new List<string>(sourceNames);
      _sourceInput.AddOptions(options); // 드롭다운에 장치 이름 리스트 주입

      var currentSourceName = imageSource.sourceName;
      var defaultValue = options.FindIndex(option => option == currentSourceName);

      if (defaultValue >= 0)
      {
        _sourceInput.value = defaultValue;
      }

      // 유저가 웹캠 장비(ex: 노트북 내장 캠 -> USB 캠)를 명시적으로 교체 선택했을 때 가동하는 리스너
      _sourceInput.onValueChanged.AddListener(delegate
      {
        imageSource.SelectSource(_sourceInput.value); // 미디어파이프 물리 이미지 소스 타겟 인덱스 변경 제어
        _isChanged = true;
        InitializeResolution(); // 장치별로 지원 해상도가 상이하므로 하위 해상도 UI 항목만 즉시 동적 새로고침
      });
    }

    /*주석 추가함*/
    // 선택한 디바이스 장치가 물리적으로 출력 가능한 픽셀 해상도 규격 행렬을 동적 바인딩
    private void InitializeResolution()
    {
      _resolutionInput = gameObject.transform.Find(_ResolutionPath).gameObject.GetComponent<Dropdown>();
      _resolutionInput.ClearOptions();
      _resolutionInput.onValueChanged.RemoveAllListeners();

      var imageSource = ImageSourceProvider.ImageSource;
      var resolutions = imageSource.availableResolutions; // 선택 장치의 런타임 가용 해상도 프리셋 리스트 인출

      if (resolutions == null) // 정적 파일 이미지 소스처럼 해상도 개념이 고정형일 경우 UI 차단 처리
      {
        _resolutionInput.enabled = false;
        return;
      }

      var options = resolutions.Select(resolution => resolution.ToString()).ToList(); // 해상도 구조체를 문자열(ex: "640 x 480")로 컨버팅
      _resolutionInput.AddOptions(options);

      var currentResolutionStr = imageSource.resolution.ToString();
      var defaultValue = options.FindIndex(option => option == currentResolutionStr);

      if (defaultValue >= 0)
      {
        _resolutionInput.value = defaultValue;
      }

      // 유저가 UI 상에서 특정 출력 해상도로 변경 피킹 시 구동하는 리스너
      _resolutionInput.onValueChanged.AddListener(delegate
      {
        imageSource.SelectResolution(_resolutionInput.value); // 미디어파이프 카메라 디바이스 세팅 픽셀 해상도 재반영 명령
        _isChanged = true;
      });
    }

    /*주석 추가함*/
    // 사용자의 화면 시점을 거울 모드로 좌우 대칭 반전시킬지 여부를 제어
    private void InitializeIsHorizontallyFlipped()
    {
      _isHorizontallyFlippedInput = gameObject.transform.Find(_IsHorizontallyFlippedPath).gameObject.GetComponent<Toggle>();
      _isHorizontallyFlippedInput.onValueChanged.RemoveAllListeners(); // 안전을 위한 기본 청소

      var imageSource = ImageSourceProvider.ImageSource;
      _isHorizontallyFlippedInput.isOn = imageSource.isHorizontallyFlipped; // 현재 데이터 상태를 토글 체크에 대입
      
      // 유저가 체크박스를 키거나 끌 때 즉시 미디어파이프 프레임 트랜스폼 데이터 구조에 True/False 상태 투사
      _isHorizontallyFlippedInput.onValueChanged.AddListener(delegate
      {
        imageSource.isHorizontallyFlipped = _isHorizontallyFlippedInput.isOn;
        _isChanged = true; // 트랜스폼 행렬 변경 유발로 인한 플래그 활성화
      });
    }
  }
}