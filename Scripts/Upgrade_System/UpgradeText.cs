using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// 강화 시 출력되는 텍스트 애니메이션 처리 클래스
/// </summary>
public class UpgradeText : MonoBehaviour
{
    public TextMeshProUGUI label;
    
    public void Setup(string message)
    {
        float duration = DataManager.instance.gameData.abilities[6].isActivate ? 0.25f : 0.5f;
        label.text = message;

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPosY(100f, duration))
            .Join(label.DOFade(0f, duration))
            .OnComplete(() => Destroy(gameObject));
    }
}
