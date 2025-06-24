using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 배경음악(BGM), 효과음(SFX) 볼륨 설정 및 UI 연동
/// </summary>
public class AudioSetting : MonoBehaviour
{
    public TextMeshProUGUI bgmText;
    public TextMeshProUGUI sfxText;

    public Button bgmUpBtn;
    public Button bgmDownBtn;
    public Button sfxUpBtn;
    public Button sfxDownBtn;

    private void Start()
    {
        bgmUpBtn.onClick.AddListener(() => AdjustVolume(GameConstants.BGM_KEY, true, bgmText));
        bgmDownBtn.onClick.AddListener(() => AdjustVolume(GameConstants.BGM_KEY, false, bgmText));
        sfxUpBtn.onClick.AddListener(() => AdjustVolume(GameConstants.SFX_KEY, true, sfxText));
        sfxDownBtn.onClick.AddListener(() => AdjustVolume(GameConstants.SFX_KEY, false, sfxText));

        
        int bgm = PlayerPrefs.GetInt(GameConstants.SFX_KEY, 5);
        int sfx = PlayerPrefs.GetInt(GameConstants.BGM_KEY, 5);
        bgmText.text = bgm.ToString();
        sfxText.text = sfx.ToString();
    }

    /// <summary>
    /// 볼륨 증가/감소
    /// </summary>
    private void AdjustVolume(string key, bool increase, TextMeshProUGUI label)
    {
        int current = PlayerPrefs.GetInt(key, 5);
        int next = current + (increase ? 1 : -1);

        if (next < GameConstants.min_Volume || next > GameConstants.max_Volume) return;

        PlayerPrefs.SetInt(key, next);
        PlayerPrefs.Save();
        label.text = next.ToString();

        AudioManager.instance.PlaySfx(0);
        AudioManager.instance.UpdateVolume();
    }
}
