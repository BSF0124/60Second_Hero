/// <summary>
/// 강화 효과 텍스트를 위로 띄우며 표시하는 컴포넌트
/// </summary>
using UnityEngine;
using DG.Tweening;
using TMPro;

public class UpgradeText : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    
    public void Setup(string message)
    {
        if (DataManager.instance.gameData.abilities[6].isActivate)
            duration *= 0.5f;

        var rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        var text = GetComponent<TextMeshProUGUI>().
        text.text = message;

        rect.DOAnchorPosY(100f, duration)
            .OnComplete(() => Destroy(gameObject));
    }
}
