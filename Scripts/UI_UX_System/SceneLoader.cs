using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 씬 전환 및 페이드, BGM 로딩 효과 담당 클래스
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    public Image fadeImage;

    private float duration = 0.7f;
    private string sceneNames = "GameScene";

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
        }
    }

    private void Start()
    {
        StartCoroutine(AudioLoad());
    }

    public IEnumerator AudioLoad()
    {
        AudioManager.instance.bgmPlayer.volume = 0f;
        AudioManager.instance.PlayBgm(1);
        AudioManager.instance.PlayBgm(0);
        yield return new WaitForSeconds(0.2f);

        AudioManager.instance.StopBgm();
        AudioManager.instance.UpdateVolume();

        StartCoroutine(LoadScene());
    }

    /// <summary>
    /// 씬 비동기 로드
    /// </summary>
    public IEnumerator LoadScene()
    {
        yield return FadeIn();
        
        SceneManager.LoadScene(sceneNames, LoadSceneMode.Additive);

        yield return FadeOut();
    }

    /// <summary>
    /// 현재 씬을 언로드 후 다시 로드 (리셋 용도)
    /// </summary>
    public IEnumerator ReloadScene()
    {
        yield return FadeIn();
        
        SceneManager.UnloadSceneAsync(sceneNames);
        DataManager.instance.ResetGameData();
        SceneManager.LoadScene(sceneNames, LoadSceneMode.Additive);

        yield return FadeOut();
    }

    /// <summary>
    /// 페이드 인
    /// </summary>
    private IEnumerator FadeIn(bool fadeOutBgm = false)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        if (fadeOutBgm && AudioManager.instance.bgmPlayer.isPlaying)
        {
            AudioManager.instance.bgmPlayer.DOFade(0f, duration)
                .OnComplete(() =>
                {
                    AudioManager.instance.StopBgm();
                    AudioManager.instance.UpdateVolume();
                });
        }

        yield return fadeImage.DOFade(1f, duration).SetEase(Ease.OutExpo).WaitForCompletion();
        AudioManager.instance.StopAllSfx();
    }

    /// <summary>
    /// 페이드 아웃
    /// </summary>
    private IEnumerator FadeOut()
    {
        yield return fadeImage.DOFade(0f, duration).SetEase(Ease.InExpo).OnComplete(() =>
            fadeImage.gameObject.SetActive(false)).WaitForCompletion();
    }
}
