using UnityEngine;

/// <summary>
/// 파티 구성 및 플레이어 생성/버프 관리
/// </summary>
public class PartyManager : MonoBehaviour
{
    public GameManager gameManager;
    public Enemy enemy;
    public GameObject passiveSkill;
    public UpgradeUIManager upgradeUIManager;

    public GameObject playerPrefab;
    public Player mainPlayer;

    // 곱연산 버프
    public double damageBuff = 0;
    public float attackSpeedBuff = 0;
    public double goldBuff = 0;

    // 합연산 버프
    public double extraDamage = 0;
    public float extraAttackSpeed = 0;

    public bool isPartyMove = false;

    private void Update()
    {
        if (isPartyMove)
        {
            transform.position += Vector3.right * Time.deltaTime * 2f;
        }
    }

    /// <summary>
    /// 메인 플레이어 생성 및 등록
    /// </summary>
    public void CreateNewPlayer()
    {
        GameManager.SetState(GameState.Paused);

        if (mainPlayer != null)
        {
            mainPlayer.transform.position = new Vector3(
                -10.5f + ((DataManager.instance.gameData.players.Count - 1) / 8 * -18) + ((DataManager.instance.gameData.players.Count - 1) % 8 * -2f),
                -1.5f,
                0);
            mainPlayer.isMainPlayer = false;
        }

        GameObject newPlayer = Instantiate(playerPrefab, new Vector3(-5f, -1.5f, 0f), Quaternion.identity, transform);
        SetRandomItem setItem = newPlayer.transform.GetChild(0).GetComponent<SetRandomItem>();
        setItem.RandomItem();

        Player player = newPlayer.GetComponent<Player>();
        mainPlayer = player;

        DataManager.instance.gameData.players.Add(player.stats);
        DataManager.instance.gameData.playeritems.Add(setItem.items);
        DataManager.instance.SaveGameData();

        if (DataManager.instance.gameData.abilities[2].isActivate)
            passiveSkill.SetActive(true);
        else
            GameManager.SetState(GameState.Playing);

        UpdatePlayersAttackSpeed();
    }

    /// <summary>
    /// 저장된 플레이어 데이터 로드
    /// </summary>
    public void LoadPlayer(Stats stats, EquippedItems items, Vector3 targetPos)
    {
        GameObject loadedPlayer = Instantiate(playerPrefab, targetPos, Quaternion.identity, transform);
        SetRandomItem setItem = loadedPlayer.transform.GetChild(0).GetComponent<SetRandomItem>();
        setItem.items = items;
        setItem.LoadItems();

        Player player = loadedPlayer.GetComponent<Player>();
        player.stats = stats;
        player.isMainPlayer = false;
    }

    /// <summary>
    /// 파티 전체의 버프 값 계산
    /// </summary>
    public void UpdateBuffVal()
    {
        damageBuff = 0;
        attackSpeedBuff = 0;
        goldBuff = 0;
        extraDamage = 0;
        extraAttackSpeed = 0;

        foreach (Transform child in transform)
        {
            Player player = child.GetComponent<Player>();
            switch (player.stats.passive)
            {
                case Passive.PartyDamageUp:
                    damageBuff += player.stats.rarity switch
                    {
                        Rarity.Common => 0.1,
                        Rarity.Rare => 0.15,
                        Rarity.Legendary => 0.25,
                        _ => 0
                    };
                    break;

                case Passive.PartyAttackSpeedUp:
                    attackSpeedBuff += player.stats.rarity switch
                    {
                        Rarity.Common => 0.1f,
                        Rarity.Rare => 0.2f,
                        Rarity.Legendary => 0.3f,
                        _ => 0
                    };
                    break;

                case Passive.IncreasesGoldGain:
                    goldBuff += player.stats.rarity switch
                    {
                        Rarity.Common => 0.1,
                        Rarity.Rare => 0.2,
                        Rarity.Legendary => 0.4,
                        _ => 0
                    };
                    break;

                case Passive.PartyBuff:
                    extraDamage += 1 * Mathf.Pow(2, player.stats.damage_LV - 1) * 0.15;
                    extraAttackSpeed += (1 + (player.stats.attackSpeed_LV - 1) * GameConstants.DefaultAttackSpeedMultiplier) * 0.15f;
                    break;
            }
        }
    }

    /// <summary>
    /// 파티 내 모든 플레이어의 공격속도 재적용
    /// </summary>
    public void UpdatePlayersAttackSpeed()
    {
        foreach (Transform item in transform)
        {
            item.GetComponent<Player>().UpdateAttackSpeed();
        }
    }
}
