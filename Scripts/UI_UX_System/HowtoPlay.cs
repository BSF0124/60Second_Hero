using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// How to Play 튜토리얼 연출 관리
/// </summary>
public class HowtoPlay : MonoBehaviour
{
    public GameManager gameManager;
    public CameraMove cameraMove;
    public Image htpPanel_1;
    public Image htpPanel_2;
    public GameObject htpPanel_3;

    public TextMeshProUGUI[] texts;
    public Image[] images;
    public Sprite[] htpSprites;

    public int stepIndex = 0;
    private float duration = 0.5f;
    public bool isEffectRunning = true;
    private bool isStartGame = false;

    private void OnEnable()
    {
        if (isStartGame) cameraMove.MovetoInit();
        
        isEffectRunning = true; 
        stepIndex = 0;
        GameManager.SetState(GameState.Paused);
        htpPanel_1.gameObject.SetActive(true);
        htpPanel_1.DOFade(0.9f, duration).OnComplete(() => StartCoroutine(PlayStep()));
    }

    private void Update()
    {
        if (!isEffectRunning && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape)))
        {
            StartCoroutine(PlayStep());
        }
    }

    /// <summary>
    /// 튜토리얼 단계별 연출 처리
    /// </summary>
    private IEnumerator PlayStep()
    {
        yield return null;
        isEffectRunning = true;
        AudioManager.instance.PlaySfx(0);

        switch (stepIndex)
        {
            case 0:
                htpPanel_1.sprite = htpSprites[0];
                yield return texts[0].DOFade(1f, duration).WaitForCompletion();
                break;

            case 1:
                htpPanel_1.sprite = htpSprites[1];
                images[0].DOFade(1f, duration);
                yield return texts[1].DOFade(1f, duration).WaitForCompletion();

                break;

            case 2:
                htpPanel_1.sprite = htpSprites[2];
                images[1].DOFade(1f, duration);
                yield return texts[2].DOFade(1f, duration).WaitForCompletion();
                break;

            case 3:
                htpPanel_1.color = new Color(1f, 1f, 1f, 0.9f);
                htpPanel_1.gameObject.SetActive(false);
                htpPanel_2.gameObject.SetActive(true);
                yield return texts[3].DOFade(1f, duration).WaitForCompletion();
                break;

            case 4:
                yield return texts[4].DOFade(1f, duration).WaitForCompletion();
                break;

            case 5:
                images[2].DOFade(1f, duration);
                images[3].DOFade(1f, duration);
                yield return texts[5].DOFade(1f, duration).WaitForCompletion();
                break;

            case 6:
            default:
                if (isStartGame)
                {
                    yield return GetComponent<CanvasGroup>().DOFade(0f, duration).OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        GameManager.SetState(GameState.Playing);
                    }).WaitForCompletion();
                }
                else
                {
                    htpPanel_2.gameObject.SetActive(false);
                    htpPanel_3.gameObject.SetActive(true);
                    yield return texts[6].DOFade(1f, duration).WaitForCompletion();
                }
                break;
            case 7:
                yield return GetComponent<CanvasGroup>().DOFade(0f, duration).OnComplete(() =>
                {
                    gameManager.StartGame();
                    gameObject.SetActive(false);
                    isStartGame = true;
                }).WaitForCompletion();
                break;
        }

        stepIndex++;
        isEffectRunning = false;
    }

    /// <summary>
    /// 비활성화 시 모든 요소 초기화
    /// </summary>
    private void OnDisable()
    {
        htpPanel_1.sprite = null;
        htpPanel_1.color = new Color(0f, 0f, 0f, 0f);
        htpPanel_1.gameObject.SetActive(false);
        htpPanel_2.gameObject.SetActive(false);
        htpPanel_3.gameObject.SetActive(false);

        foreach (var text in texts)
            text.color = new Color(1f, 1f, 1f, 0f);

        foreach (var img in images)
            img.color = new Color(1f, 1f, 1f, 0f);

        GetComponent<CanvasGroup>().alpha = 1f;
    }
}
