// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.Sample.UI;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection.UI
{
  /*주석 추가함*/
  // 인게임 혹은 설정 캔버스에서 팝업되는 모달 윈도우(ModalContents)를 상속받아, 
  // 손 관절 추적 파라미터 세팅을 런타임에 제어하는 UI 뷰 컴포넌트
  public class HandLandmarkDetectionConfigWindow : ModalContents
  {
    /*주석 추가함*/
    // 인스펙터에서 수동 매핑할 UI 레퍼런스 요소
    [SerializeField] private Dropdown _delegateInput; // 연산 주체(CPU/GPU) 선택 드롭다운
    [SerializeField] private Dropdown _imageReadModeInput; // 이미지 판독 방식 선택 드롭다운
    [SerializeField] private Dropdown _runningModeInput; // AI 작동 스트리밍 모드 선택 드롭다운
    [SerializeField] private InputField _numHandsInput; // 추적할 손의 개수 입력 필드
    [SerializeField] private InputField _minHandDetectionConfidenceInput; // 손 탐지 최소 신뢰도 입력 필드
    [SerializeField] private InputField _minHandPresenceConfidenceInput; // 손 존재 실체 검증 신뢰도 입력 필드
    [SerializeField] private InputField _minTrackingConfidenceInput; // 연속 프레임 추적 최소 신뢰도 입력 필드

    private HandLandmarkDetectionConfig _config; // 실질적인 파라미터 값이 저장되는 백엔드 데이터 환경설정 객체 레퍼런스
    private bool _isChanged; // 사용자가 UI를 조작하여 초기 진입 시점 대비 설정값을 하나라도 변경했는지 추적하는 상태 플래그

    private void Start()
    {
      /*주석 추가함*/
      // 씬 내부의 메인 Solution 오브젝트에 부착된 런너 스크립트로부터 원본 설정(Config) 인스턴스를 동적 서칭하여 바인딩
      _config = GameObject.Find("Solution").GetComponent<HandLandmarkerRunner>().config;
      InitializeContents(); // 현재 원본 세팅값을 기반으로 UI 화면의 각 항목 요소 초기화 가동
    }

    /*주석 추가함*/
    // 사용자가 취소 혹은 확인을 눌러 나가려 할 때, 
    // 값의 변경 여부(`_isChanged`)를 모달 시스템에 통보하여 엔진 재부팅 혹은 갱신
    public override void Exit()
    {
      GetModal().CloseAndResume(_isChanged);
    }

    /*주석 추가함*/
    // 이하 Switch 및 Set 계열 메서드들은 유저가 UI 요소를 조작하는 순간 
    // 호출되어 백엔드 데이터 변수를 최신화하는 수신기(Listener) 내부 로직

    private void SwitchDelegate()
    {
      _config.Delegate = (Tasks.Core.BaseOptions.Delegate)_delegateInput.value;
      _isChanged = true;
    }

    private void SwitchImageReadMode()
    {
      _config.ImageReadMode = (ImageReadMode)_imageReadModeInput.value;
      _isChanged = true;
    }

    private void SwitchRunningMode()
    {
      _config.RunningMode = (Tasks.Vision.Core.RunningMode)_runningModeInput.value;
      _isChanged = true;
    }

    private void SetNumHands()
    {
      /*주석 추가함*/
      // 문자열 형태로 입력된 숫자를 정수형 데이터로 안전하게 예외 변환 파싱 후 주입
      if (int.TryParse(_numHandsInput.text, out var value))
      {
        _config.NumHands = value;
        _isChanged = true;
      }
    }

    private void SetMinHandDetectionConfidence()
    {
      /*주석 추가함*/
      // 문자열 형태로 입력된 정확도 수치를 실수형 데이터로 안전하게 예외 변환 파싱 후 주입
      if (float.TryParse(_minHandDetectionConfidenceInput.text, out var value))
      {
        _config.MinHandDetectionConfidence = value;
        _isChanged = true;
      }
    }

    private void SetMinHandPresenceConfidence()
    {
      if (float.TryParse(_minHandPresenceConfidenceInput.text, out var value))
      {
        _config.MinHandPresenceConfidence = value;
        _isChanged = true;
      }
    }

    private void SetMinTrackingConfidence()
    {
      if (float.TryParse(_minTrackingConfidenceInput.text, out var value))
      {
        _config.MinTrackingConfidence = value;
        _isChanged = true;
      }
    }

    /*주석 추가함*/
    // 백엔드 설정 객체에 담겨있는 최초 수치 데이터를 기반으로 화면상의 
    // 7가지 UI 서브 컴포넌트들을 전면 매칭 초기화합니다.
    private void InitializeContents()
    {
      InitializeDelegate();
      InitializeImageReadMode();
      InitializeRunningMode();
      InitializeNumHands();
      InitializeMinHandDetectionConfidence();
      InitializeMinHandPresenceConfidence();
      InitializeMinTrackingConfidence();
    }

    /*주석 추가함*/
    // 이하 Initialize 서브 메서드들은 현재 데이터 상태를 UI 그래픽 요소에 채워 넣고 
    // 변경 감지 이벤트 핸들러(`AddListener`)를 구독 연동

    private void InitializeDelegate()
    {
      InitializeDropdown<Tasks.Core.BaseOptions.Delegate>(_delegateInput, _config.Delegate.ToString());
      _delegateInput.onValueChanged.AddListener(delegate { SwitchDelegate(); }); // 유저가 드롭다운 목록 변경 시 데이터 동적 동기화
    }

    private void InitializeImageReadMode()
    {
      InitializeDropdown<ImageReadMode>(_imageReadModeInput, _config.ImageReadMode.GetDescription());
      _imageReadModeInput.onValueChanged.AddListener(delegate { SwitchImageReadMode(); });
    }

    private void InitializeRunningMode()
    {
      InitializeDropdown<Tasks.Vision.Core.RunningMode>(_runningModeInput, _config.RunningMode.ToString());
      _runningModeInput.onValueChanged.AddListener(delegate { SwitchRunningMode(); });
    }

    private void InitializeNumHands()
    {
      _numHandsInput.text = _config.NumHands.ToString();
      _numHandsInput.onValueChanged.AddListener(delegate { SetNumHands(); }); // 유저가 키보드로 텍스트 수정 시 데이터 파싱 및 갱신 트리거
    }

    private void InitializeMinHandDetectionConfidence()
    {
      _minHandDetectionConfidenceInput.text = _config.MinHandDetectionConfidence.ToString();
      _minHandDetectionConfidenceInput.onValueChanged.AddListener(delegate { SetMinHandDetectionConfidence(); });
    }

    private void InitializeMinHandPresenceConfidence()
    {
      _minHandPresenceConfidenceInput.text = _config.MinHandPresenceConfidence.ToString();
      _minHandPresenceConfidenceInput.onValueChanged.AddListener(delegate { SetMinHandPresenceConfidence(); });
    }

    private void InitializeMinTrackingConfidence()
    {
      _minTrackingConfidenceInput.text = _config.MinTrackingConfidence.ToString();
      _minTrackingConfidenceInput.onValueChanged.AddListener(delegate { SetMinTrackingConfidence(); });
    }
  }
}