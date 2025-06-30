using System.Text;
using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// 플레이어 캐릭터의 자동 공격 및 패시브 효과 처리
/// </summary>
[System.Serializable]
public class Stats
{
    public int damage_LV;
    public int attackSpeed_LV;
    public int criticalDamage_LV;
    public int criticalChance_LV;
    public Passive passive = Passive.None;
    public Rarity rarity = Rarity.None;

    public Stats(int damage_LV, int attackSpeed_LV, int criticalDamage_LV, int criticalChance_LV)
    {
        this.damage_LV = damage_LV;
        this.attackSpeed_LV = attackSpeed_LV;
        this.criticalDamage_LV = criticalDamage_LV;
        this.criticalChance_LV = criticalChance_LV;
    }
}

public class Player : MonoBehaviour
{
    public Stats stats;
    public bool isMainPlayer = false;

    private float attackTimer;
    private float attackSpeed;
    private int boostedAttackCount = 0;
    public bool isEffectActive = true;

    public GameObject statPanel;
    public GameObject textEffectTMP;
    private PartyManager partyManager => transform.parent.GetComponent<PartyManager>();
    private Animator playerAnimator => transform.GetChild(0).GetChild(0).GetComponent<Animator>();


    private void Awake()
    {
        stats = new Stats(1, 1, 1, 1);
    }

    void Update()
    {
        if (GameManager.currentState != GameState.Playing) return;
        if (partyManager.enemy.isDie || !CanAttack()) return;

        // 어빌리티에 따른 공격 속도 보정
        float multiplier = DataManager.instance.gameData.abilities[6].isActivate ? 2f : 1f;
        attackTimer += Time.deltaTime * multiplier;

        // 일정 주기마다 공격 수행
        if (attackTimer >= 1 / attackSpeed)
        {
            TryAttack();
            attackTimer = 0f;
        }
    }

    /// <summary>
    /// 현재 공격 가능한 상태인지 판단
    /// </summary>
    private bool CanAttack()
    {
        return (!isMainPlayer || (isMainPlayer && DataManager.instance.gameData.abilities[0].isActivate)) && stats.passive != Passive.PartyBuff;
    }

    /// <summary>
    /// 공격 실행: 대미지 계산 -> 적에게 전달 -> 골드 획득 -> 추가 공격 여부 판단
    /// </summary>
    public void TryAttack(bool isExtra = false)
    {
        if(isEffectActive) playerAnimator.SetTrigger("Attack");
        double damage = CalculateDamage(out int damageType);

        if (isExtra) damageType = 3;

        partyManager.enemy.TakeDamage(damage, damageType, isEffectActive);
        GainGold(damage);
        
        if (!isExtra)
            TryExtraAttack();
    }

    /// <summary>
    /// 패시브 및 스탯 기반의 대미지 계산 로직
    /// </summary>
    private double CalculateDamage(out int type)
    {
        type = 0;
        double damage = Mathf.Pow(2, stats.damage_LV - 1);

        // 패시브 적용 효과
        var passive = stats.passive;

        // [패시브]공격력 증가
        if (passive == Passive.DamageUp)
        {
            double multiplier = stats.rarity switch
            {
                Rarity.Common => 1.2,
                Rarity.Rare => 1.3,
                Rarity.Legendary => 1.4,
                _ => 1
            };
            damage *= multiplier;
        }

        // [패시브]파티원 공격력 증가
        damage += damage * partyManager.damageBuff;

        // [패시브]서포터
        damage += partyManager.extraDamage;

        // [패시브]돈의 힘
        if (passive == Passive.GoldToDamage)
        {
            damage += DataManager.instance.gameData.gold * 0.1;
        }

        // 크리티컬 여부 판단 및 적용
        float rand = Random.value;
        bool isCrit = false;

        // [패시브]고정 피해
        if (passive == Passive.CriticalX2)
        {
            float critThreshold = stats.rarity == Rarity.Rare ? 0.5f : 0.3f;
            isCrit = rand >= critThreshold;
        }
        else
        {
            isCrit = rand <= (0.2f + (stats.criticalChance_LV - 1) * 0.05f);
        }

        if (isCrit)
        {
            type = 1;
            float multiplier = passive == Passive.CriticalX2 ? 2f : 1f;
            damage *= 1.2 + (stats.criticalDamage_LV - 1) * 0.05f * multiplier;
        }

        // [패시브]강화 공격
        if (passive == Passive.BoostedAttack)
        {
            boostedAttackCount++;
            int threshold = stats.rarity == Rarity.Legendary ? 10 : 15;
            if (boostedAttackCount >= threshold)
            {
                boostedAttackCount = 0;
                type = 2;
                damage *= stats.rarity == Rarity.Legendary ? 5 : 3;
            }
        }

        return damage;
    }

    /// <summary>
    /// 대미지 기반 골드 획득 로직
    /// </summary>
    private void GainGold(double damage)
    {
        if (DataManager.instance.gameData.abilities[1].isActivate)
        {
            double Gain = damage * 0.05 * (partyManager.goldBuff + 1);
            DataManager.instance.gameData.gold += Gain;
            partyManager.upgradeUIManager.UpdateButtons();
            partyManager.gameManager.UpdateTexts();
        }
    }

    /// <summary>
    /// 추가 공격 확률 처리 (패시브 적용 시)
    /// </summary>
    private void TryExtraAttack()
    {
        if (stats.passive != Passive.BonusAttack) return;

        float chance = stats.rarity == Rarity.Legendary ? 0.4f : 0.2f;
        float rand = Random.value;

        if(rand <= chance)
        {
            TryAttack(true);
        }
    }

    /// <summary>
    /// 패시브 및 스탯 기반의 공격 속도 계산 로직
    /// </summary>
    public void UpdateAttackSpeed()
    {
        attackSpeed = 1 + (stats.attackSpeed_LV - 1) * GameConstants.DefaultAttackSpeedMultiplier;

        // [패시브]공격 속도 증가
        if (stats.passive == Passive.AttackSpeedUp)
        {
            float multiplier = stats.rarity switch
            {
                Rarity.Common => 1.3f,
                Rarity.Rare => 1.4f,
                Rarity.Legendary => 1.5f,
                _ => 1f
            };
            attackSpeed *= multiplier;
        }

        // [패시브]파티원 공격 속도 증가
        attackSpeed *= 1 + partyManager.attackSpeedBuff;

        // [패시브]서포터
        attackSpeed += partyManager.extraAttackSpeed;
    }

    public void OnMouseEnter()
    {
        if (GameManager.currentState == GameState.Ended) return;

        Vector3 mousePosition = Input.mousePosition;
        float screenMidPoint = Screen.width / 2;

        RectTransform panelRect = statPanel.GetComponent<RectTransform>();
        Vector3 panelPosition = panelRect.anchoredPosition;

        panelPosition.x = mousePosition.x < screenMidPoint ?
        Mathf.Abs(panelPosition.x) : -Mathf.Abs(panelPosition.x);
        panelRect.anchoredPosition = panelPosition;

        double baseDamage = Mathf.Pow(2, stats.damage_LV - 1);
        double damageBuff = transform.parent.GetComponent<PartyManager>().damageBuff;
        if (stats.passive == Passive.DamageUp)
        {
            switch (stats.rarity)
            {
                case Rarity.Common:
                    damageBuff += 0.2;
                    break;
                case Rarity.Rare:
                    damageBuff += 0.3;
                    break;
                case Rarity.Legendary:
                    damageBuff += 0.4;
                    break;
            }
        }

        double extraDamage = transform.parent.GetComponent<PartyManager>().extraDamage;
        if (stats.passive == Passive.GoldToDamage)
        {
            extraDamage += DataManager.instance.gameData.gold * 0.1;
        }

        float attackSpeedBuff = transform.parent.GetComponent<PartyManager>().attackSpeedBuff;
        if (stats.passive == Passive.AttackSpeedUp)
        {
            switch (stats.rarity)
            {
                case Rarity.Common:
                    attackSpeedBuff += 0.3f;
                    break;
                case Rarity.Rare:
                    attackSpeedBuff += 0.4f;
                    break;
                case Rarity.Legendary:
                    attackSpeedBuff += 0.5f;
                    break;
            }
        }

        float extraAttackSpeed = transform.parent.GetComponent<PartyManager>().extraAttackSpeed;
        float criticalDamage = (1.2f + (stats.criticalDamage_LV - 1) * 0.05f) * 100;
        float criticalChance = (0.2f + (stats.criticalChance_LV - 1) * 0.05f) * 100;
        if (stats.passive == Passive.CriticalX2)
        {
            criticalChance = stats.rarity == Rarity.Rare ? 50 : 70;
        }

        string criticalDamageStr = stats.passive == Passive.CriticalX2 ? criticalDamage + "%<color=#FF0000>X2</color>" : criticalDamage + "%";


        StringBuilder damageStr = new StringBuilder();
        damageStr.Append(NumberFormatter.FormatNumber(baseDamage));
        if (damageBuff > 0) damageStr.Append("+<color=#00B705>" + System.Math.Floor(damageBuff * 100) + "%</color>");
        if (extraDamage > 0) damageStr.Append("+<color=#FF5555>" + NumberFormatter.FormatNumber(extraDamage) + "</color>");

        StringBuilder attackSpeedStr = new StringBuilder();
        attackSpeedStr.Append(1 + (stats.attackSpeed_LV - 1) * 0.5f);
        if (attackSpeedBuff > 0) attackSpeedStr.Append("+<color=#00B705>" + Mathf.FloorToInt(attackSpeedBuff * 100) + "%</color>");
        if (extraAttackSpeed > 0) attackSpeedStr.Append("+<color=#FF5555>" + Mathf.FloorToInt(extraAttackSpeed) + "</color>");

        StringBuilder passiveStr = new StringBuilder();
        if (stats.passive != Passive.None) passiveStr.Append("패시브 스킬:" + PassiveSkill.GetPassiveSkillDescription(stats.passive, stats.rarity));
        statPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
        $"공격력:{damageStr}\n" +
        $"공격 속도:{attackSpeedStr}\n" +
        $"치명타 데미지:{criticalDamageStr}\n" +
        $"치명타 확률:{criticalChance}%\n" +
        passiveStr;
        statPanel.SetActive(true);
    }

    public void OnMouseExit()
    {
        statPanel.SetActive(false);
    }

    public void OnMouseDown()
    {
        if (GameManager.currentState == GameState.Ended) return;

        float duration = DataManager.instance.gameData.abilities[6].isActivate ? 0.25f : 0.5f;
        if (isEffectActive)
        {
            isEffectActive = false;
        }
        else
        {
            isEffectActive = true;
        }
        transform.GetChild(0).GetComponent<SetRandomItem>().SetAlpha(isEffectActive);
        GameObject text = Instantiate(textEffectTMP, transform.GetChild(1));
        text.GetComponent<TextMeshProUGUI>().text = isEffectActive ? "텍스트 효과 ON" : "텍스트 효과 OFF";
        text.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 100f);
        text.GetComponent<RectTransform>().DOAnchorPosY(200f, duration).OnComplete(() => Destroy(text));
    }
}
