/// <summary>
/// 능력치 강화 버튼 UI 처리 및 강화 로직 제어
/// </summary>
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    [Header("Manager")]
    public GameManager gameManager;
    public PartyManager partyManager;

    [Header("Buttons")]
    public Button damageUpBtn;
    public Button attackSpeedUpBtn;
    public Button criticalDamageUpBtn;
    public Button criticalChanceUpBtn;
    public Button abilityBtn;

    [Header("Panels & UI")]
    public GameObject abilityPanel;
    public GameObject passivePanel;
    public TextMeshProUGUI abilityPointText;
    public TextMeshProUGUI timerText;
    public UpgradeText upgradeTextPrefab;

    private double damageUpCost;
    private double attackSpeedUpCost;
    private double criticalDamageUpCost;
    private double criticalChanceUpCost;

    private float autoUpgradeTimer;
    private readonly float autoUpgradeInterval = 0.5f;

    private void Awake()
    {
        damageUpBtn.onClick.AddListener(DamageUp);
        attackSpeedUpBtn.onClick.AddListener(AttackSpeedUp);
        criticalChanceUpBtn.onClick.AddListener(CriticalChanceUp);
        criticalDamageUpBtn.onClick.AddListener(CriticalDamageUp);
        abilityBtn.onClick.AddListener(ShowAbilityPanel);
    }

    private void Start()
    {
        UpdateAbilityPointText();
        abilityPointText.text = DataManager.instance.gameData.abilityPoint > 0 ? 
        $"어빌리티 <color=#00FF00>({DataManager.instance.gameData.abilityPoint})</color>" : "어빌리티";
    }

    private void Update()
    {
        if (GameManager.isPaused) return;
        
        if (DataManager.instance.gameData.abilities[3].isActivate)
        {
            float multiplier = DataManager.instance.gameData.abilities[6].isActivate ? 2f : 1f;
            autoUpgradeTimer += Time.deltaTime * multiplier;

            if (autoUpgradeTimer >= autoUpgradeInterval)
            {
                autoUpgradeTimer = 0f;
                AutoUpgrade();
                UpdateButtonTexts();
            }
        }
    }

    private void AutoUpgrade()
    {
        double minCost = damageUpCost;
        if (attackSpeedUpCost < minCost) minCost = attackSpeedUpCost;
        if (criticalDamageUpCost < minCost) minCost = criticalDamageUpCost;
        if (criticalChanceUpCost < minCost &&
        partyManager.mainPlayer.stats.criticalChance_LV < 17 &&
        partyManager.mainPlayer.stats.passive != Passive.CriticalX2)
            minCost = criticalChanceUpCost;

        if (minCost == damageUpCost) DamageUp();
        else if (minCost == attackSpeedUpCost) AttackSpeedUp();
        else if (minCost == criticalDamageUpCost) CriticalDamageUp();
        else CriticalChanceUp();
    }

    private void UpgradeStat(ref double cost, Action upgradeAction, string effectText)
    {
        if (DataManager.instance.gameData.gold < cost) return;

        DataManager.instance.gameData.gold -= cost;
        AudioManager.instance.PlaySfx(AudioManager.instance.sfxClips[7]);
        Instantiate(upgradeTextPrefab, transform).Setup(effectText);
        upgradeAction.Invoke();
        SavePlayerData();
        UpdateButtonTexts();
    }

    public void DamageUp()
    {
        UpgradeStat(ref damageUpCost, () =>
        {
            partyManager.mainPlayer.stats.damage_LV++;
            damageUpCost *= 2 + (partyManager.mainPlayer.stats.damage_LV / 20 * 0.5);
        }, "공격력 증가");
    }
    
    public void AttackSpeedUp()
    {
        UpgradeStat(ref attackSpeedUpCost, () => {
            partyManager.mainPlayer.stats.attackSpeed_LV++;
            attackSpeedUpCost *= 2 + (partyManager.mainPlayer.stats.attackSpeed_LV / 20 * 0.5);
            partyManager.mainPlayer.UpdateAttackSpeed();
        }, "공격 속도 증가");
    }
    
    public void CriticalDamageUp()
    {
        UpgradeStat(ref criticalDamageUpCost, () => {
            partyManager.mainPlayer.stats.criticalDamage_LV++;
            criticalDamageUpCost *= 2 + (partyManager.mainPlayer.stats.criticalDamage_LV / 20 * 0.5);
        }, "치명타 데미지 증가");
    }
    
    public void CriticalChanceUp()
    {
        UpgradeStat(ref criticalChanceUpCost, () => {
            partyManager.mainPlayer.stats.criticalChance_LV++;
            criticalChanceUpCost *= 2 + (partyManager.mainPlayer.stats.criticalChance_LV / 20 * 0.5);
            if (partyManager.mainPlayer.stats.criticalChance_LV >= 17)
                criticalChanceUp.interactable = false;
        }, "치명타 확률 증가");
    }

    public void UpdateGold()
    {
        Stats stats = partyManager.mainPlayer.stats;
        damageUpCost = 10 * Math.Pow(2, stats.damage_LV - 1);
        attackSpeedUpCost = 10 * Math.Pow(2, stats.attackSpeed_LV - 1);
        criticalDamageUpCost = 10 * Math.Pow(2, stats.criticalDamage_LV - 1);
        criticalChanceUpCost = 10 * Math.Pow(2, stats.criticalChance_LV - 1);

        UpdateButtonTexts();
    }

    public void UpdateButtonTexts()
    {
        Stats stats = partyManager.mainPlayer.stats;

        damageUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            $"공격력 +{NumberFormatter.FormatNumber(Math.Pow(2, stats.damage_LV - 1))} \n<color=#50C878>${NumberFormatter.FormatNumber(damageUpCost)}</color>";

        attackSpeedUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            $"공격 속도 +0.5 \n<color=#50C878>${NumberFormatter.FormatNumber(attackSpeedUpCost)}</color>";

        criticalDamageUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            $"치명타 데미지 +5% \n<color=#50C878>${NumberFormatter.FormatNumber(criticalDamageUpCost)}</color>";

        if (stats.criticalChance_LV >= 17 || stats.passive == Passive.CriticalX2)
        {
            criticalChanceUp.interactable = false;
            criticalChanceUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                stats.rarity == Rarity.Legendary ? "치명타 확률 70%" : "치명타 확률 50%";
        }
        else
        {
            criticalChanceUp.interactable = true;
            criticalChanceUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                $"치명타 확률 +5% \n<color=#50C878>${NumberFormatter.FormatNumber(criticalChanceUpCost)}</color>";
        }

        gameManager.UpdateTexts();
        UpdateButtonStates();
    }
    
    private void UpdateButtonStates()
    {
        SetButtonState(damageUp, damageUpCost);
        SetButtonState(attackSpeedUp, attackSpeedUpCost);
        SetButtonState(criticalDamageUp, criticalDamageUpCost);
        SetButtonState(criticalChanceUp, criticalChanceUpCost);
    }

    private void SetButtonState(Button button, double cost)
    {
        bool canAfford = DataManager.instance.gameData.gold >= cost;
        var image = button.GetComponent<Image>();
        var text = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        image.color = canAfford ? Color.white : Color.gray;
        text.color = canAfford ? Color.white : Color.gray;
    }

    public void ShowAbilityPanel()
    {
        if (passivePanel.activeSelf) return;
        
        GameManager.isPaused = true;
        AudioManager.instance.PlaySfx(AudioManager.instance.sfxClips[1]);
        abilityPanel.SetActive(true);
        timerText.text = "<color=#FFFFFF>일시 정지</color>";
    }

    private void SavePlayerData()
    {
        var data = DataManager.instance.gameData;
        data.players[data.players.Count - 1] = partyManager.mainPlayer.stats;
        DataManager.instance.SaveGameData();
    }

    private void UpdateAbilityPointText()
    {
        int points = DataManager.instance.gameData.abilityPoint;
        abilityPointText.text = points > 0 ? $"어빌리티 <color=#00FF00>({points})</color>" : "어빌리티";
    }
}
