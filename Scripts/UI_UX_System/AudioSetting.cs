/// <summary>
/// 사운드 설정을 저장하고 불러오는 UI 컴포넌트
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    public TextMeshProUGUI bgmVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    public Button bgmUpBtn;
    public Button bgmDownBtn;
    public Button sfxUpBtn;
    public Button sfxDownBtn;

    private void OnEnable()
    {
        UpdateUI();

        bgmUpBtn.onClick.AddListener(() => ChangeVolume("BGMVolume", 0.1f));
        bgmDownBtn.onClick.AddListener(() => ChangeVolume("BGMVolume", -0.1f));
        sfxUpBtn.onClick.AddListener(() => ChangeVolume("SFXVolume", 0.1f));
        sfxDownBtn.onClick.AddListener(() => ChangeVolume("SFXVolume", -0.1f));
    }

    private void ChangeVolume(string key, float delta)
    {
        float volume = Mathf.Clamp(PlayerPrefs.GetFloat(key, 1f) + delta, 0f, 1f);
        PlayerPrefs.SetFloat(key, volume);
        AudioManager.instance.UpdateVoulumn();
        UpdateUI();
    }

    private void UpdateUI()
    {
        bgmVolumeText.text = $"{(int)(PlayerPrefs.GetFloat("BGMVolumn", 1f) * 10)}";
        sfxVolumeText.text = $"{(int)(PlayerPrefs.GetFloat("SFXVolumn", 1f) * 10)}";
    }
}
