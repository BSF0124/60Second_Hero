/// <summary>
/// 파티 내 패시브 스킬의 총합을 계산하여 버프에 반영
/// </summary>
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public double damageBuff{ get; private set; }
    public float attackSpeedBuff{ get; private set; }
    public double goldBuff { get; private set; }
    public double extraDamage { get; private set; }
    public float extraAttackSpeed { get; private set; }

    public void UpdateBuffValues()
    {
        damageBuff = 0;
        attackSpeedBuff = 0;
        goldBuff = 0;
        extraDamage = 0;
        extraAttackSpeed = 0;

        foreach (Transform unit in transform)
        {
            var player = unit.GetComponent<Player>();
            switch (player.stats.passive)
            {
                case Passive.PartyDamageUp:
                    damageBuff += GetPassiveValue(player.stats.rarity, 0.1, 0.15, 0.25);
                    break;
                case Passive.PartyAttackSpeedUp:
                    attackSpeedBuff += (float)GetPassiveValue(player.stats.rarity, 0.1, 0.2, 0.3);
                    break;
                case Passive.IncreasesGoldGain:
                    goldBuff += GetPassiveValue(player.stats.rarity, 0.1, 0.2, 0.4);
                    break;
                case Passive.PartyBuff:
                    extraDamage += 10 * Mathf.Pow(2, player.stats.damage_LV - 1) * 0.15;
                    extraAttackSpeed += (1 +  (player.stats.damage_LV - 1) * 0.5f) * 0.15f;
                    break;
            }
        }
    }

    private double GetPassiveValue(Rarity rarity, double common, double rare, double legendary)
    {
        return rarity switch
        {
            Rarity.Common => common,
            Rarity.Rare => rare,
            Rarity.Legendary => legendary,
            _ => 0
        };
    }
}
