using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rigidbody2D rb;
    private TextMeshProUGUI tmp;

    private Vector2 savedVelocity = Vector2.zero;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rb = GetComponent<Rigidbody2D>();
        tmp = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        HandlePauseState();
        
        if(rectTransform.anchoredPosition.y <= GameConstants.DestroyThresholdY)
            Destroy(gameObject);
    }
    
    /// <summary>
    /// 게임 상태에 따른 텍스트의 물리 작용 설정
    /// </summary>
    private void HandlePauseState()
    {
        if (GameManager.currentState == GameState.Paused)
        {
            rb.gravityScale = 0f;
            if (savedVelocity == Vector2.zero)
            {
                savedVelocity = rb.linearVelocity;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rb.gravityScale = DataManager.instance.gameData.abilities[6].isActivate ? 2f : 1f;
            if (savedVelocity != Vector2.zero)
            {
                rb.linearVelocity = savedVelocity;
                savedVelocity = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// 데미지 텍스트 효과 출력
    /// </summary>
    /// <param name="damage">표시할 데미지 수치</param>
    /// <param name="type">0:기본, 1:크리티컬, 2:강화공격, 3:추가공격</param>
    public void Bounce(double damage, int type)
    {
        tmp.text = NumberFormatter.FormatNumber(damage);

        float duration = DataManager.instance.gameData.abilities[6].isActivate ? 0.3f : 0.15f;

        float randomX = 0f, randomY = 0f;

        switch (type)
        {
            case 1:
                tmp.color = Color.red;
                randomX = Random.Range(-3f, 3f);
                randomY = Random.Range(4f, 6f);
                rectTransform.DOScale(Vector3.one * 1.7f, duration).SetEase(Ease.OutBack);
                break;
            case 2:
                tmp.color = new Color(1f, 0.27f, 0f);
                randomX = Random.Range(-3f, 3f);
                randomY = Random.Range(6f, 8f);
                rectTransform.DOScale(Vector3.one * 2.5f, duration).SetEase(Ease.OutBack);
                break;
            case 3:
                tmp.color = new Color(0.66f, 0.33f, 1f);
                randomX = Random.Range(-3f, 3f);
                randomY = Random.Range(7f, 10f);
                rectTransform.DOScale(Vector3.one * 1.7f, duration).SetEase(Ease.OutBack);
                break;
            default:
                randomX = Random.Range(-2f, 2f);
                randomY = Random.Range(3f, 4f);
                break;
        }

        rb.gravityScale = DataManager.instance.gameData.abilities[6].isActivate ? 2f : 1f;
        rb.linearVelocity = new Vector2(randomX, randomY);
    }
}
