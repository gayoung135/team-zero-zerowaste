using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.Sample.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

public class Setting : MonoBehaviour
{
    public GameObject ModalObject; //게임 중 설정 켤 때 정지용 오브젝트
    public GameObject ImageObject; //게임 중 설정 끌 때 재개용 오브젝트
    public GameObject settingMenu;
    public Dropdown languageDropdown;    
    public Slider volumeSlider;


    private bool isLoading = false; //설정 충돌 방지용
    void Start()
    {
        Application.targetFrameRate = 60; //프레임 지정
        isLoading = true;
        
        // 저장된 설정 불러오기
        float volume = PlayerPrefs.GetFloat("Volume", 100f);
        int language = PlayerPrefs.GetInt("Language", 0);

        // 실제 적용
        AudioListener.volume = volume / 100f;
        ApplyLanguage(language);

        languageDropdown.value = language;
        volumeSlider.value = volume;

        isLoading = false;

        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        Debug.Log("설정 적용 볼륨 : " + AudioListener.volume);
    }

    // 볼륨 변경
    public void OnVolumeChanged(float value)
    {
        if (isLoading) return;
        Debug.Log("볼륨 저장 : " + value);
        AudioListener.volume = value / 100f;

        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }

    // 언어 변경
    public void OnLanguageChanged(int index)
    {
        if (isLoading) return;
        Debug.Log("언어 저장 : " + index);
        ApplyLanguage(index);

        PlayerPrefs.SetInt("Language", index);
        PlayerPrefs.Save();
    }

    // 언어 적용
    void ApplyLanguage(int index)
    {
        switch(index)
        {
            case 0:
                Debug.Log("영어 적용");
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
                break;

            case 1:
                Debug.Log("한국어 적용");
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("ko");
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!settingMenu.activeSelf) //설정 켜기
            {
                settingMenu.SetActive(true);

                if (ModalObject != null) {
                    Modal modal = ModalObject.GetComponent<Modal>();
                    modal.OpenAndPause();
                }
            }
            else //설정 끄기
            {
                if (ImageObject != null) {
                    ImageSourceConfig config = ImageObject.GetComponent<ImageSourceConfig>();
                    config.Exit();
                }

                settingMenu.SetActive(false);
            }
        }
    }
}
