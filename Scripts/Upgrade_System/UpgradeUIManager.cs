using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 강화 비용 계산 유틸리티. 레벨에 따른 지수 및 추가 보정 적용
/// </summary>
public static class UpgradeCostCalculator
{
    public static double GetUpgradeCost(int level, double baseCost, double scale = 2)
    {
        return baseCost * (int)Math.Pow(scale, level - 1);
    }
}

/// <summary>
/// 강화 버튼 및 어빌리티 패널 등의 UI 처리 담당
/// </summary>
public class UpgradeUIManager : MonoBehaviour
{
    [Header("Manager")]
    public GameManager gameManager;
    public PartyManager partyManager;

    [Header("Buttons")]
    public Button damageUp;
    public Button attackSpeedUp;
    public Button criticalDamageUp;
    public Button criticalChanceUp;
    public Button abilityButton;

    [Header("Panels")]
    public GameObject abilityPanel;
    public GameObject passivePanel;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI abilityPointText;

    public UpgradeText upgradeTextPrefab;

    private double damageUpCost;
    private double attackSpeedUpCost;
    private double criticalDamageUpCost;
    private double criticalChanceUpCost;

    private float autoUpgradeTimer = 0f;
    private float AutoUpgradeInterval => DataManager.instance.gameData.abilities[6].isActivate ? 0.25f : 0.5f;

    private void Awake()
    {
        damageUp.onClick.AddListener(() => TryUpgrade(StatType.Damage));
        attackSpeedUp.onClick.AddListener(() => TryUpgrade(StatType.AttackSpeed));
        criticalDamageUp.onClick.AddListener(() => TryUpgrade(StatType.CriticalDamage));
        criticalChanceUp.onClick.AddListener(() => TryUpgrade(StatType.CriticalChance));
        abilityButton.onClick.AddListener(ShowAbilityPanel);
    }

    private void Start()
    {
        abilityPointText.text = DataManager.instance.gameData.abilityPoint > 0 ?
        $"어빌리티 <color=#00FF00>({DataManager.instance.gameData.abilityPoint})</color>" : "어빌리티";

        UpdateGold();
    }

    private void Update()
    {
        if (GameManager.currentState == GameState.Paused || !DataManager.instance.gameData.abilities[3].isActivate)
            return;

        // 어빌리티 3: 자동 업그레이드
        autoUpgradeTimer += Time.deltaTime * (DataManager.instance.gameData.abilities[6].isActivate ? 2f : 1f);
        if (autoUpgradeTimer >= AutoUpgradeInterval)
        {
            autoUpgradeTimer = 0f;
            AutoUpgrade();
        }
    }

    private enum StatType { Damage, AttackSpeed, CriticalDamage, CriticalChance }

    /// <summary>
    /// 스탯 업그레이드 시도(비용 체크 후 적용)
    /// </summary>
    private void TryUpgrade(StatType type)
    {
        if (partyManager.mainPlayer == null) return;

        var stats = partyManager.mainPlayer.stats;

        double currentCost = type switch
        {
            StatType.Damage => damageUpCost,
            StatType.AttackSpeed => attackSpeedUpCost,
            StatType.CriticalDamage => criticalDamageUpCost,
            StatType.CriticalChance => criticalChanceUpCost,
            _ => 0
        };

        if (DataManager.instance.gameData.gold < currentCost) return;
        DataManager.instance.gameData.gold -= currentCost;
        AudioManager.instance.PlaySfx(7);

        string label = type switch
        {
            StatType.Damage => "공격력 증가",
            StatType.AttackSpeed => "공격 속도 증가",
            StatType.CriticalDamage => "치명타 데미지 증가",
            StatType.CriticalChance => "치명타 확률 증가",
            _ => ""
        };

        Instantiate(upgradeTextPrefab, GetButton(type).transform).Setup(label);

        switch (type)
        {
            case StatType.Damage:
                stats.damage_LV++;
                damageUpCost = UpgradeCostCalculator.GetUpgradeCost(stats.damage_LV, GameConstants.GoldBaseUpgradeCost);
                break;
            case StatType.AttackSpeed:
                stats.attackSpeed_LV++;
                attackSpeedUpCost = UpgradeCostCalculator.GetUpgradeCost(stats.attackSpeed_LV, GameConstants.GoldBaseUpgradeCost);
                break;
            case StatType.CriticalDamage:
                stats.criticalDamage_LV++;
                criticalDamageUpCost = UpgradeCostCalculator.GetUpgradeCost(stats.criticalDamage_LV, GameConstants.GoldBaseUpgradeCost);
                break;
            case StatType.CriticalChance:
                stats.criticalChance_LV++;
                criticalChanceUpCost = UpgradeCostCalculator.GetUpgradeCost(stats.criticalChance_LV, GameConstants.GoldBaseUpgradeCost);
                if (stats.criticalChance_LV >= GameConstants.MaxCriticalChanceLevel)
                    criticalChanceUp.interactable = false;
                break;
        }

        SavePlayerData();
        UpdateGold();
    }

    private Button GetButton(StatType type) => type switch
    {
        StatType.Damage => damageUp,
        StatType.AttackSpeed => attackSpeedUp,
        StatType.CriticalDamage => criticalDamageUp,
        StatType.CriticalChance => criticalChanceUp,
        _ => null
    };

    /// <summary>
    /// 자동 업그레이드(강화 비용이 낮은 스탯을 우선으로 업그레이드)
    /// </summary>
    private void AutoUpgrade()
    {
        double minCost = damageUpCost;
        StatType cheapest = StatType.Damage;

        if (attackSpeedUpCost < minCost || minCost < 0) { minCost = attackSpeedUpCost; cheapest = StatType.AttackSpeed; }

        if (criticalDamageUpCost < minCost || minCost < 0) { minCost = criticalDamageUpCost; cheapest = StatType.CriticalDamage; }
        
        if (criticalChanceUpCost < minCost &&
            partyManager.mainPlayer.stats.criticalChance_LV < GameConstants.MaxCriticalChanceLevel &&
            partyManager.mainPlayer.stats.passive != Passive.CriticalX2)
        {
            minCost = criticalChanceUpCost;
            cheapest = StatType.CriticalChance;
        }

        if(minCost > 0)
            TryUpgrade(cheapest);
    }

    /// <summary>
    /// 강화 비용 텍스트 및 버튼 상태 갱신
    /// </summary>
    public void UpdateGold()
    {
        if (partyManager.mainPlayer == null) return;

        var stats = partyManager.mainPlayer.stats;

        damageUpCost = UpgradeCostCalculator.GetUpgradeCost(stats.damage_LV, GameConstants.GoldBaseUpgradeCost);
        attackSpeedUpCost = UpgradeCostCalculator.GetUpgradeCost(stats.attackSpeed_LV, GameConstants.GoldBaseUpgradeCost);
        criticalDamageUpCost = UpgradeCostCalculator.GetUpgradeCost(stats.criticalDamage_LV, GameConstants.GoldBaseUpgradeCost);
        criticalChanceUpCost = UpgradeCostCalculator.GetUpgradeCost(stats.criticalChance_LV, GameConstants.GoldBaseUpgradeCost);

        UpdateButtons();
        partyManager.UpdateBuffVal();
        partyManager.UpdatePlayersAttackSpeed();
    }

    /// <summary>
    /// 골드 보유량 기준으로 버튼 색상 및 텍스트 컬러 변경
    /// </summary>
    public void UpdateButtons()
    {
        double gold = DataManager.instance.gameData.gold;

        if (damageUpCost >= 0)
            UpdateButtonVisual(damageUp, gold >= damageUpCost, $"공격력 +{NumberFormatter.FormatNumber(1 * Mathf.Pow(2, partyManager.mainPlayer.stats.damage_LV - 1))} \n<color=#50C878>${NumberFormatter.FormatNumber(damageUpCost)}</color>");
        else
        {
            damageUp.interactable = false;
            UpdateButtonVisual(damageUp, false, $"공격력 MAX");
        }

        if (attackSpeedUpCost >= 0)
            UpdateButtonVisual(attackSpeedUp, gold >= attackSpeedUpCost, $"공격 속도 +0.5 \n<color=#50C878>${NumberFormatter.FormatNumber(attackSpeedUpCost)}</color>");
        else
        {
            attackSpeedUp.interactable = false;
            UpdateButtonVisual(attackSpeedUp, false, $"공격 속도 MAX");
        }

        if (criticalDamageUpCost >= 0)
            UpdateButtonVisual(criticalDamageUp, gold >= criticalDamageUpCost, $"치명타 데미지 +5% \n<color=#50C878>${NumberFormatter.FormatNumber(criticalDamageUpCost)}</color>");
        else
        {
            criticalDamageUp.interactable = false;
            UpdateButtonVisual(criticalDamageUp, false, $"치명타 데미지 MAX");
        }

        if (partyManager.mainPlayer.stats.criticalChance_LV >= GameConstants.MaxCriticalChanceLevel)
        {
            criticalChanceUp.interactable = false;
            UpdateButtonVisual(criticalChanceUp, false, "치명타 확률 MAX");
        }
        else if (partyManager.mainPlayer.stats.passive == Passive.CriticalX2)
        {
            criticalChanceUp.interactable = false;
            string label = partyManager.mainPlayer.stats.rarity switch
            {
                Rarity.Rare => "치명타 확률 50%",
                Rarity.Legendary => "치명타 확률 70%",
                _ => "치명타 확률"
            };
            UpdateButtonVisual(criticalChanceUp, false, label);
        }
        else
        {
            criticalChanceUp.interactable = true;
            UpdateButtonVisual(criticalChanceUp, gold >= criticalChanceUpCost, $"치명타 확률 +5% \n<color=#50C878>${NumberFormatter.FormatNumber(criticalChanceUpCost)}</color>");
        }

        gameManager.UpdateTexts();
    }

    private void UpdateButtonVisual(Button button, bool activate, string labelText)
    {
        var color = activate ? Color.white : Color.gray;
        button.GetComponent<Image>().color = color;

        var tmp = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        tmp.color = color;
        tmp.text = labelText;
    }

    /// <summary>
    /// 능력치 강화 후 유저 데이터 저장
    /// </summary>
    private void SavePlayerData()
    {
        var playerList = DataManager.instance.gameData.players;
        playerList[playerList.Count - 1] = partyManager.mainPlayer.stats;
        DataManager.instance.SaveGameData();
    }

    /// <summary>
    /// 어빌리티 패널 표시
    /// </summary>
    public void ShowAbilityPanel()
    {
        if (passivePanel.activeSelf) return;

        GameManager.SetState(GameState.Paused);
        AudioManager.instance.PlaySfx(1);
        abilityPanel.SetActive(true);
        timerText.text = "<color=#FFFFFF>일시 정지</color>";
    }
    /// <summary>
    /// 어빌리티 패널 숨기기
    /// </summary>
    public void HideAbilityPanel()
    {
        abilityPanel.SetActive(false);
        GameManager.SetState(GameState.Playing);
    }

    /// <summary>
    /// 현재 유닛을 파티원으로 추가
    /// </summary>
    public void AddParty()
    {
        DataManager.instance.SaveGameData();
        gameManager.ResetGame();
    }
}
