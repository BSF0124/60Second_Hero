using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 어빌리티 패널 전체를 관리하는 클래스(설명, 버튼 상태, 해금/활성화)
/// </summary>
public class AbilityButtons : MonoBehaviour
{
    public Button[] abilityButtons;
    public Button activateButton;
    public Button cancelButton;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI abilityPointText;
    public TextMeshProUGUI titleText;

    public int index = -1;
    public int selectIndex = -1;

    private void Start()
    {
        activateButton.onClick.AddListener(Activate);
        cancelButton.onClick.AddListener(Cancel);
        UpdateButtons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Cancel();
    }

    /// <summary>
    /// 어빌리티 포인트 텍스트 갱신
    /// </summary>
    public void UpdateAbilityPointText()
    {
        int point = DataManager.instance.gameData.abilityPoint;
        abilityPointText.text = point > 0 ? $"어빌리티 <color=#00FF00>({point})</color>" : "어빌리티";
    }

    /// <summary>
    /// 어빌리티 설명 텍스트 갱신
    /// </summary>
    public void UpdateDescriptionText()
    {
        string[] titles =
        {
            "자동 공격", "미다스의 손", "패시브 스킬 획득", "자동 강화", "저금통",
            "패시브 스킬 리롤", "배속 모드", "추가 시간", "운수좋은날"
        };

        string[] descriptions =
        {
            "메인 용사가 <color=#FFD700>자동</color>으로 공격합니다.(공격 속도에 비례)",
            "용사가 입힌 데미지의 5%만큼 <color=#50C878>$</color>를 획득합니다.",
            "새로운 용사를 생성할 때, <color=#FFD700>패시브 스킬</color>을 획득합니다.",
            "능력이 <color=#FFD700>자동</color>으로 강화됩니다.\n(<color=#50C878>$</color> 소모량이 적은 능력 우선)",
            "<color=#50C878>$</color>가 초기화되지 않습니다.",
            "<color=#FFD700>패시브 스킬</color>을 <color=#00BFFF>3회</color> <color=#50C878>새로 고침</color>할 수 있습니다.",
            "게임 속도가 <color=#00BFFF>2배</color> 빨라집니다.",
            "적을 처치할 때 마다 시간이 <color=#00BFFF>3초</color> 연장됩니다.(최대 60초)",
            "높은 등급의 <color=#FFD700>패시브 스킬</color>이 등장할 확률이 <color=#FF0000>대폭 증가</color>합니다."
        };

        if (index >= 0 && index < titles.Length)
        {
            titleText.text = titles[index];
            descriptionText.text = descriptions[index];
        }
        else
        {
            titleText.text = "";
            descriptionText.text = "";
        }

        UpdateActivateText();
    }

    /// <summary>
    /// 어빌리티 활성화 텍스트 및 버튼 색상 갱신
    /// </summary>
    public void UpdateActivateText()
    {
        var abilities = DataManager.instance.gameData.abilities;
        var ability = index >= 0 && index < abilities.Count ? abilities[index] : null;

        if (ability == null)
        {
            SetActivateText("", Color.white);
            return;
        }

        if ((index == GameConstants.INDEX_REROLL || index == GameConstants.INDEX_LUCK) && !abilities[GameConstants.INDEX_PASSIVE].unlock)
        {
            SetActivateText("<color=#FFD700>< 패시브 스킬 ></color> 해금 후 이용 가능", Color.gray);
        }
        else if (!ability.unlock)
        {
            SetActivateText("어빌리티 해금\n(-1 포인트)", Color.white);
        }
        else if (ability.isActivate)
        {
            SetActivateText("활성화 됨", new Color(0.4f, 1f, 0.6f));
        }
        else
        {
            SetActivateText("비활성화 됨", Color.gray);
        }
    }
    private void SetActivateText(string text, Color color)
    {
        var label = activateButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        label.text = text;
        activateButton.GetComponent<Image>().color = color;
    }

    /// <summary>
    /// 어빌리티 버튼 색상 및 활성 상태 갱신
    /// </summary>
    public void UpdateButtons()
    {
        var abilities = DataManager.instance.gameData.abilities;
        for (int i = 0; i < abilityButtons.Length; i++)
        {
            abilityButtons[i].interactable = true;
            var img = abilityButtons[i].GetComponent<Image>();

            img.color = !abilities[i].unlock ? Color.gray :
                abilities[i].isActivate ? new Color(0.4f, 1f, 0.6f) : Color.white;
        }
        UpdateAbilityPointText();
    }

    /// <summary>
    /// 어빌리티 해금 및 활성화 처리
    /// </summary>
    public void Activate()
    {
        if (index < 0) return;

        var data = DataManager.instance.gameData;
        var ability = data.abilities[index];

        // 해금 조건 확인
        bool requirePassive = (index == GameConstants.INDEX_REROLL || index == GameConstants.INDEX_LUCK) &&
        !data.abilities[GameConstants.INDEX_PASSIVE].unlock;
        
        // 어빌리티 해금 및 활성화
        if (!ability.unlock && data.abilityPoint > 0 && !requirePassive)
        {
            data.abilityPoint--;
            ability.unlock = true;
            ability.isActivate = true;

            if (index == GameConstants.INDEX_SPEED)
            {
                AudioManager.instance.StopBgm();
                AudioManager.instance.PlayBgm(1);
            }
        }
        // 어빌리티 활성화/비활성화
        else if (ability.unlock)
        {
            ability.isActivate = !ability.isActivate;
            if (index == GameConstants.INDEX_SPEED)
            {
                AudioManager.instance.StopBgm();
                AudioManager.instance.PlayBgm(ability.isActivate ? 1 : 0);
            }
        }

        UpdateDescriptionText();
        UpdateButtons();
        UpdateAbilityPointText();
        AudioManager.instance.PlaySfx(0);
        DataManager.instance.SaveGameData();
    }

    /// <summary>
    /// 패널 닫기 및 일시정지 해제
    /// </summary>
    public void Cancel()
    {
        selectIndex = -1;
        index = -1;
        GameManager.SetState(GameState.Playing);
        gameObject.SetActive(false);
        AudioManager.instance.PlaySfx(2);
    }
}
