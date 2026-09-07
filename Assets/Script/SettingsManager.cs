using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("--- UI Panel Settings ---")]
    [Tooltip("기본 설정창 패널 오브젝트")]
    public GameObject settingsPanel;
    [Tooltip("메인/시작 화면에 위치한 설정 버튼")]
    public Button openSettingsButton;
    [Tooltip("설정창 내부의 EXIT 버튼")]
    public Button exitButton;

    [Header("--- Webcam Settings ---")]
    [Tooltip("웹캠 목록을 보여줄 드롭다운 (TMP)")]
    public TMP_Dropdown webcamDropdown;
    [Tooltip("좌우반전을 토글할 체크박스")]
    public Toggle flipToggle;
    [Tooltip("웹캠 화면이 출력되는 RawImage (있을 경우 할당)")]
    public RawImage webcamDisplay;

    private WebCamTexture activeWebCamTexture;

    [Header("--- Language Settings ---")]
    [Tooltip("언어 선택 드롭다운 (TMP)")]
    public TMP_Dropdown languageDropdown;

    [Header("--- Volume Settings ---")]
    [Tooltip("음량 조절 슬라이더 (Min: 0, Max: 100 권장)")]
    public Slider volumeSlider;
    [Tooltip("현재 음량 수치를 표시할 텍스트 (선택사항)")]
    public TextMeshProUGUI volumeValueText;

    private void Awake()
    {
        // 1. 버튼 클릭 이벤트 자동 연결
        if (openSettingsButton != null)
            openSettingsButton.onClick.AddListener(OpenSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(CloseSettings);
    }

    private void Start()
    {
        // 2. 초기 UI 상태 및 기능 세팅
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        InitWebcamDropdown();
        InitLanguageDropdown();
        InitVolumeSlider();

        // 3. UI 변경 이벤트 리스너 등록
        if (webcamDropdown != null)
            webcamDropdown.onValueChanged.AddListener(OnWebcamChanged);

        if (flipToggle != null)
            flipToggle.onValueChanged.AddListener(OnFlipToggled);

        if (languageDropdown != null)
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    // ==========================================
    // 🟢 1. 설정창 열기 / 닫기 (EXIT) Logic
    // ==========================================
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ==========================================
    // 📷 2. 웹캠 선택 & 좌우반전 (Flip) Logic
    // ==========================================
    private void InitWebcamDropdown()
    {
        if (webcamDropdown == null) return;

        webcamDropdown.ClearOptions();
        List<string> options = new List<string>();

        // 연결된 장치 중 웹캠 검색
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            options.Add("웹캠을 찾을 수 없음");
            webcamDropdown.AddOptions(options);
            webcamDropdown.interactable = false;
            return;
        }

        foreach (WebCamDevice device in devices)
        {
            options.Add(device.name);
        }

        webcamDropdown.AddOptions(options);
        webcamDropdown.value = 0;
        webcamDropdown.RefreshShownValue();

        // 첫 번째 웹캠 자동 시작
        StartWebcam(devices[0].name);
    }

    private void StartWebcam(string deviceName)
    {
        if (activeWebCamTexture != null && activeWebCamTexture.isPlaying)
        {
            activeWebCamTexture.Stop();
        }

        activeWebCamTexture = new WebCamTexture(deviceName);

        if (webcamDisplay != null)
        {
            webcamDisplay.texture = activeWebCamTexture;
            activeWebCamTexture.Play();
            ApplyWebcamFlip(flipToggle != null && flipToggle.isOn);
        }
    }

    private void OnWebcamChanged(int index)
    {
        if (WebCamTexture.devices.Length > index)
        {
            StartWebcam(WebCamTexture.devices[index].name);
        }
    }

    private void OnFlipToggled(bool isFlipped)
    {
        ApplyWebcamFlip(isFlipped);
    }

    private void ApplyWebcamFlip(bool isFlipped)
    {
        if (webcamDisplay != null)
        {
            // Y축 Scale을 -1로 변경하여 좌우 반전 처리
            Vector3 scale = webcamDisplay.rectTransform.localScale;
            scale.x = isFlipped ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            webcamDisplay.rectTransform.localScale = scale;
        }
    }

    // ==========================================
    // 🌐 3. 언어 설정 (한국어 / 영어) Logic
    // ==========================================
    private void InitLanguageDropdown()
    {
        if (languageDropdown == null) return;

        languageDropdown.ClearOptions();
        List<string> options = new List<string> { "한국어", "English" };
        languageDropdown.AddOptions(options);
        
        // 기본값: 한국어 (Index 0)
        languageDropdown.value = PlayerPrefs.GetInt("SelectedLanguage", 0);
        languageDropdown.RefreshShownValue();
    }

    private void OnLanguageChanged(int index)
    {
        PlayerPrefs.SetInt("SelectedLanguage", index);
        PlayerPrefs.Save();

        if (index == 0)
        {
            Debug.Log("[Language Changed] 한국어로 설정되었습니다.");
            // TODO: 다국어 지원 매니저 연동 시 한국어 변환 로직 호출
        }
        else if (index == 1)
        {
            Debug.Log("[Language Changed] Set to English.");
            // TODO: 다국어 지원 매니저 연동 시 영어 변환 로직 호출
        }
    }

    // ==========================================
    // 🔊 4. 음량 조절 (0 ~ 100) Logic
    // ==========================================
    private void InitVolumeSlider()
    {
        if (volumeSlider == null) return;

        // Slider 최소/최대 범위 설정 (0 ~ 100)
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 100f;

        // 기존 저장된 음량이 있다면 로드 (기본값: 80)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 80f);
        volumeSlider.value = savedVolume;

        UpdateVolume(savedVolume);
    }

    private void OnVolumeChanged(float value)
    {
        UpdateVolume(value);
    }

    private void UpdateVolume(float value)
    {
        // 0~100 수치를 유니티 오디오 볼륨 범위(0.0 ~ 1.0)로 변환
        AudioListener.volume = value / 100f;

        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(value).ToString();
        }

        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        // 게임 종료 시 웹캠 자원 해제
        if (activeWebCamTexture != null && activeWebCamTexture.isPlaying)
        {
            activeWebCamTexture.Stop();
        }
    }
}
