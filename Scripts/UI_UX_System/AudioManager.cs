using UnityEngine;

/// <summary>
/// 게임 내 BGM 및 SFX 재생을 관리하는 싱글톤 오디오 매니저
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Clips")]
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;

    [Header("BGM")]
    public float bgmVolume;
    public AudioSource bgmPlayer;

    [Header("SFX")]
    public float sfxVolume;
    public int channels = 16;
    AudioSource[] sfxPlayers;
    private int channelIndex = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
            return;
        }

        bgmVolume = PlayerPrefs.GetInt(GameConstants.BGM_KEY, 5) / 10f;
        sfxVolume = PlayerPrefs.GetInt(GameConstants.SFX_KEY, 10) / 10f;
        InitAudioSources();
    }

    /// <summary>
    /// BGM 및 SFX용 AudioSource 초기화
    /// </summary>
    private void InitAudioSources()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.SetParent(transform);
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.SetParent(transform);
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < channels; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBgm(int index)
    {
        if (index < 0 || index >= bgmClips.Length || bgmClips[index] == null) return;
        bgmPlayer.Stop();
        bgmPlayer.clip = bgmClips[index];
        bgmPlayer.Play();
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBgm()
    {
        bgmPlayer.Stop();
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlaySfx(int index)
    {
        if (index < 0 || index >= sfxClips.Length || sfxClips[index] == null) return;

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int playerIndex = (channelIndex + i) % sfxPlayers.Length;

            if (!sfxPlayers[playerIndex].isPlaying)
            {
                sfxPlayers[playerIndex].clip = sfxClips[index];
                sfxPlayers[playerIndex].Play();
                channelIndex = (playerIndex + 1) % sfxPlayers.Length;
                break;
            }
        }
    }

    /// <summary>
    /// 모든 효과음 정지
    /// </summary>
    public void StopAllSfx()
    {
        foreach (var sfx in sfxPlayers)
        {
            sfx.Stop();
        }
    }

    /// <summary>
    /// PlayerPrefs에 따른 볼륨 재설정 및 BGM 상태 조정
    /// </summary>
    public void UpdateVolume()
    {
        bgmVolume = PlayerPrefs.GetInt(GameConstants.BGM_KEY, 5) / 10f;
        sfxVolume = PlayerPrefs.GetInt(GameConstants.SFX_KEY, 5) / 10f;

        bgmPlayer.volume = bgmVolume;
        foreach (var sfx in sfxPlayers)
        {
            sfx.volume = sfxVolume;
        }
    }
}
