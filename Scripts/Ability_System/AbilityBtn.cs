using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 어빌리티 버튼에 대한 마우스 인터랙션 및 선택 처리
/// </summary>
[RequireComponent(typeof(Button))]
public class AbilityBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int index;
    private AbilityButtons abilityButtons;

    private void Awake()
    {
        //abilityButtons = transform.parent.parent.GetComponent<AbilityButtons>();
        abilityButtons = GetComponentInParent<AbilityButtons>();
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    /// <summary>
    /// 마우스를 버튼 위에 올렸을 때 설명 표시
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateHover(true);
    }

    /// <summary>
    /// 마우스가 버튼에서 벗어났을 때 설명 제거
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateHover(false);
    }

    /// <summary>
    /// 설명 텍스트를 마우스 호버 상태에 따라 갱신
    /// </summary>
    private void UpdateHover(bool isHovering)
    {
        if (abilityButtons.selectIndex == -1)
        {
            abilityButtons.index = isHovering ? index : -1;
            abilityButtons.UpdateExplainText();
        }
    }

    /// <summary>
    /// 버튼 클릭 시 어빌리티 선택
    /// </summary>
    public void OnButtonClick()
    {
        AudioManager.instance.PlaySfx(0);
        abilityButtons.selectIndex = index;
        abilityButtons.index = index;
        abilityButtons.UpdateExplainText();
    }
}
