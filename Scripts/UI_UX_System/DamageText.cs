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
    /// <param name="type">0:일반, 1:크리티컬, 2:강화공격, 3:추가공격</param>
    public void Bounce(double damage, int type)
    {
        tmp.text = NumberFormatter.FormatNumber(damage);

        float duration = DataManager.instance.gameData.abilities[6].isActivate ? 0.3f : 0.15f;

        float randomX = 0f, randomY = 0f;

        switch (type)
        {
            case 1: // 크리티컬
                tmp.color = new Color(1f, 0.2f, 0.2f); // 진홍색
                randomX = Random.Range(-3f, 3f);
                randomY = Random.Range(5f, 7f);
                rectTransform.DOShakeScale(duration, 1.2f, 10, 90);
                break;
            case 2: // 강화 공격
                tmp.color = new Color(1f, 0.55f, 0f); // 주황색
                randomX = Random.Range(-2f, 2f);
                randomY = Random.Range(7f, 9f);
                rectTransform.localScale = Vector3.zero;
                rectTransform.DOScale(Vector3.one * 2.7f, duration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => rectTransform.DOScale(Vector3.one * 1.2f, 0.15f));
                break;
            case 3: // 추가 공격
                tmp.color = new Color(0.6f, 0.4f, 1f); // 보라색
                randomX = Random.Range(-4f, 4f);
                randomY = Random.Range(8f, 11f);
                rectTransform.DOScale(Vector3.one * 2.2f, duration).SetEase(Ease.OutBounce);
                rectTransform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
                break;
            default: // 일반
                tmp.color = Color.white;
                randomX = Random.Range(-2f, 2f);
                randomY = Random.Range(3f, 5f);
                rectTransform.DOScale(Vector3.one * 1.2f, duration).SetEase(Ease.Linear);
                rectTransform.DOShakeAnchorPos(0.2f, 5f, 10, 90);
                break;
        }

        rb.gravityScale = DataManager.instance.gameData.abilities[6].isActivate ? 2f : 1f;
        rb.linearVelocity = new Vector2(randomX, randomY);
    }
}
