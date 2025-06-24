using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 패시브 목록
/// </summary>
public enum Passive
{
    None,
    DamageUp,
    AttackSpeedUp,
    PartyDamageUp,
    PartyAttackSpeedUp,
    IncreasesGoldGain,
    CriticalX2,
    BonusAttack,
    BoostedAttack,
    PartyBuff,
    GoldToDamage
}

/// <summary>
/// 패시브 희귀도
/// </summary>
public enum Rarity
{
    None,
    Common,
    Rare,
    Legendary
}

/// <summary>
/// 유닛 생성 시 선택하는 패시브 스킬 UI 및 기능 처리
/// </summary>
public class PassiveSkill : MonoBehaviour
{
    public PartyManager partyManager;
    public TextMeshProUGUI timerText;
    public Button[] buttons;
    public Button refreshButton;

    private int refreshCount;
    private bool isCoroutineRunning = false;

    public Passive[] passiveArr = new Passive[3];
    public Rarity[] rarityArr = new Rarity[3];

    private void OnEnable()
    {
        partyManager.mainPlayer.gameObject.SetActive(false);
        timerText.text = "<color=#FFFFFF>패시브 스킬 선택</color>";

        if (DataManager.instance.gameData.abilities[5].isActivate)
        {
            refreshCount = 3;
            refreshButton.gameObject.SetActive(true);
            refreshButton.GetComponent<RectTransform>().localScale = Vector3.zero;
            refreshButton.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"({refreshCount})";
        }

        else
        {
            refreshButton.gameObject.SetActive(false);
        }

        StartCoroutine(ShowPassiveSkillPanel());
    }

    /// <summary>
    /// 패시브 목록 생성
    /// </summary>
    private void SetPassiveSkill()
    {
        List<int> common = new List<int>() { 1, 2, 3, 4, 5 };
        List<int> rare = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
        List<int> legendary = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        for (int i = 0; i < 3; i++)
        {
            float rand = Random.value;
            bool isLucky = DataManager.instance.gameData.abilities[8].isActivate;

            if ((isLucky && rand >= 0.75f) || (!isLucky && rand >= 0.9f))
            {
                int idx = Random.Range(0, legendary.Count);
                passiveArr[i] = (Passive)legendary[idx];
                rarityArr[i] = Rarity.Legendary;
                legendary.RemoveAt(idx);
            }
            else if ((isLucky && rand >= 0.45f) || (!isLucky && rand >= 0.7f))
            {
                int idx = Random.Range(0, rare.Count);
                passiveArr[i] = (Passive)rare[idx];
                rarityArr[i] = Rarity.Rare;
                rare.RemoveAt(idx);
            }
            else
            {
                int idx = Random.Range(0, common.Count);
                passiveArr[i] = (Passive)common[idx];
                rarityArr[i] = Rarity.Common;
                common.RemoveAt(idx);
            }
        }
    }

    /// <summary>
    /// 패시브 스킬 패널 등장 애니메이션
    /// </summary>
    private IEnumerator ShowPassiveSkillPanel()
    {
        float duration = DataManager.instance.gameData.abilities[6].isActivate ? 0.15f : 0.3f;
        isCoroutineRunning = true;
        SetPassiveSkill();
        for (int i = 0; i < 3; i++)
        {
            buttons[i].GetComponent<RectTransform>().localScale = Vector3.zero;
            buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GetPassiveSkillTitle(passiveArr[i], rarityArr[i]);
            buttons[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = GetPassiveSkillDescription(passiveArr[i], rarityArr[i]);
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < 3; i++)
        {
            AudioManager.instance.PlaySfx(8);
            yield return buttons[i].GetComponent<RectTransform>().DOScale(Vector3.one, duration).SetEase(Ease.OutBack).WaitForCompletion();
            yield return new WaitForSeconds(duration/2);
        }

        if (refreshButton.gameObject.activeSelf && refreshCount > 0)
        {
            yield return refreshButton.GetComponent<RectTransform>().DOScale(Vector3.one, duration).SetEase(Ease.OutBack).WaitForCompletion();
        }

        isCoroutineRunning = false;
    }

    /// <summary>
    /// 패시브 스킬 선택
    /// </summary>
    public void SelectPassive(int index)
    {
        if (isCoroutineRunning) return;

        AudioManager.instance.PlaySfx(6);
        var stats = partyManager.mainPlayer.stats;
        stats.passive = passiveArr[index];
        stats.rarity = rarityArr[index];

        DataManager.instance.SaveGameData();
        partyManager.UpdateBuffVal();
        partyManager.UpdatePlayersAttackSpeed();
        GameManager.SetState(GameState.Playing);
        partyManager.mainPlayer.gameObject.SetActive(true);
        partyManager.upgradeUIManager.UpdateGold();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 패시브 스킬 새로고침
    /// </summary>
    public void RefreshPassive()
    {
        if (isCoroutineRunning) return;

        AudioManager.instance.PlaySfx(0);
        refreshCount--;
        refreshButton.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"({refreshCount})";

        StartCoroutine(ShowPassiveSkillPanel());

        if (refreshCount == 0)
            refreshButton.gameObject.SetActive(false);
    } 
    
    public string GetPassiveSkillTitle(Passive passive, Rarity rarity)
    {
        StringBuilder str = new StringBuilder();

        string rarityColor = rarity switch
        {
            Rarity.Rare => "#1E90FF",
            Rarity.Legendary => "#FFA500",
            _ => null
        };

        string passiveName = passive switch
        {
            Passive.DamageUp => "공격력 증가",
            Passive.AttackSpeedUp => "공격 속도 증가",
            Passive.PartyDamageUp => "파티원 공격력 증가",
            Passive.PartyAttackSpeedUp => "파티원 공격 속도 증가",
            Passive.IncreasesGoldGain => "$ 획득량 증가",
            Passive.CriticalX2 => "고정 피해",
            Passive.BonusAttack => "추가 공격",
            Passive.BoostedAttack => "강화 공격",
            Passive.PartyBuff => "서포터",
            Passive.GoldToDamage => "돈의 힘",
            _ => ""
        };

        return rarityColor != null ? $"<color={rarityColor}>< {passiveName} ></color>" : $"< {passiveName} >";
    }

    public static string GetPassiveSkillDescription(Passive passive, Rarity rarity)
    {
        return passive switch
        {
            Passive.DamageUp => rarity switch
            {
                Rarity.Common => "공격력이 20% 증가합니다.",
                Rarity.Rare => "공격력이 <color=#1E90FF>30%</color> 증가합니다.",
                Rarity.Legendary => "공격력이 <color=#FFA500>40%</color> 증가합니다.",
                _ => ""
            },
            Passive.CriticalX2 => rarity switch
            {
                Rarity.Rare => "치명타 확률 50% 고정, 데미지 2배",
                Rarity.Legendary => "치명타 확률 70% 고정, 데미지 2배",
                _ => ""
            },
            Passive.BoostedAttack => rarity switch
            {
                Rarity.Rare => "15회 공격 시 다음 공격 300% 데미지",
                Rarity.Legendary => "10회 공격 시 다음 공격 500% 데미지",
                _ => ""
            },
            Passive.BonusAttack => rarity switch
            {
                Rarity.Rare => "20% 확률로 추가 공격",
                Rarity.Legendary => "40% 확률로 추가 공격",
                _ => ""
            },
            Passive.AttackSpeedUp => rarity switch
            {
                Rarity.Common => "공격 속도 30% 증가",
                Rarity.Rare => "공격 속도 <color=#1E90FF>40%</color> 증가",
                Rarity.Legendary => "공격 속도 <color=#FFA500>50%</color> 증가",
                _ => ""
            },
            Passive.PartyDamageUp => rarity switch
            {
                Rarity.Common => "모든 파티원의 공격력 10% 증가",
                Rarity.Rare => "모든 파티원의 공격력 <color=#1E90FF>15%</color> 증가",
                Rarity.Legendary => "모든 파티원의 공격력 <color=#FFA500>25%</color> 증가",
                _ => ""
            },
            Passive.PartyAttackSpeedUp => rarity switch
            {
                Rarity.Common => "모든 파티원의 공격 속도 10% 증가",
                Rarity.Rare => "모든 파티원의 공격 속도 <color=#1E90FF>20%</color> 증가",
                Rarity.Legendary => "모든 파티원의 공격 속도 <color=#FFA500>30%</color> 증가",
                _ => ""
            },
            Passive.IncreasesGoldGain => rarity switch
            {
                Rarity.Common => "$ 획득량 10% 증가",
                Rarity.Rare => "$ 획득량 <color=#1E90FF>20%</color> 증가",
                Rarity.Legendary => "$ 획득량 <color=#FFA500>40%</color> 증가",
                _ => ""
            },
            Passive.GoldToDamage => "보유한 골드의 10% 만큼 공격력 증가",
            Passive.PartyBuff => "공격하지 않고 공격력/공속의 15%를 파티에 제공",
            _ => ""
        };
    }
}
