using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 적 유닛의 체력, 피격 처리, 사망 연출, 보상, 배경 전환 기능
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("참조 컴포넌트")]
    public GameManager gameManager;
    public PartyManager partyManager;
    public BackgroundManager backgroundManager;
    public AbilityButtons abilityButtons;
    public RectTransform abilityText;
    public DamageText damagetTextPrefab;
    public UpgradeUIManager upgradeUIManager;

    [Header("HP UI")]
    public Slider hpSlider;
    public Slider easeHPSlider;
    public float lerpSpeed = 0.1f;

    [Header("전투 데이터")]
    public double maxHP = 10;
    public double currentHP;
    public bool isDie = false;

    private Transform enemyRoot => transform.GetChild(0);
    private Animator enemyAnimator => enemyRoot.GetChild(0).GetComponent<Animator>();
    private Transform damageEffectRoot => transform.GetChild(1);

    private void Update()
    {
        if (GameManager.currentState == GameState.Paused || isDie) return;

        // 체력 슬라이더 지연 효과
        if (hpSlider.value != easeHPSlider.value)
        {
            easeHPSlider.value = Mathf.Lerp(easeHPSlider.value, hpSlider.value, lerpSpeed);
        }
    }

    /// <summary>
    /// 적 셋업 - 외형, 스케일, 체력 초기화
    /// </summary>
    public void Setup()
    {
        float duration = GetEaseDuration();
        int floor = DataManager.instance.gameData.floor;

        bool isBoss = floor % 10 == 0;
        int scale = isBoss ? GetBossScale(floor) : 2;

        enemyRoot.GetComponent<SetRandomItem>().EnemyRandomItem(floor, isBoss);
        enemyRoot.DOScale(Vector3.one * scale, duration).WaitForCompletion();

        currentHP = maxHP;
        hpSlider.value = 1f;
        easeHPSlider.value = 1f;
        isDie = false;
    }

    /// <summary>
    /// 적 유닛 피격
    /// </summary>
    public void TakeDamage(double damage, int type, bool showEffect)
    {
        if (isDie) return;

        enemyAnimator.SetTrigger("_Damaged");
        currentHP -= damage;
        hpSlider.value = (float)(currentHP / maxHP);

        if (showEffect)
        {
            var damageText = Instantiate(damagetTextPrefab, damageEffectRoot);
            damageText.Bounce(damage, type);
        }

        if (currentHP <= 0) Die();
    }

    /// <summary>
    /// 적 처치
    /// </summary>
    private void Die()
    {
        int floor = DataManager.instance.gameData.floor;

        isDie = true;
        GameManager.SetState(GameState.GameOver);
        enemyAnimator.SetTrigger("_Death");

        if (floor >= 100)
        {
            StartCoroutine(gameManager.GameOver(true));
            return;
        }

        AudioManager.instance.PlaySfx(4);
        RewardGold(floor);
        gameManager.UpdateTexts();
        upgradeUIManager.UpdateButtons();

        if (floor % 10 == 0 && floor > DataManager.instance.gameData.highScore)
        {
            DataManager.instance.gameData.abilityPoint++;
            AudioManager.instance.PlaySfx(9);
            abilityText.DOShakeAnchorPos(0.5f, 30, 30);
            abilityButtons.UpdateButtons();
        }

        DataManager.instance.gameData.highScore = Math.Max(floor, DataManager.instance.gameData.highScore);
        DataManager.instance.gameData.floor++;
        StartCoroutine(OnDie());
    }
    
    public IEnumerator OnDie()
    {
        int floor = DataManager.instance.gameData.floor;
        float duration = GetEaseDuration();

        maxHP = floor switch
        {
            >= 90 => Math.Floor(maxHP * 1.3f),
            >= 70 => Math.Floor(maxHP * 1.2f),
            >= 40 => Math.Floor(maxHP * 1.4f),
            >= 20 => Math.Floor(maxHP * 1.3f),
            _ => Math.Floor(maxHP * 1.2f)
        };

        if (DataManager.instance.gameData.abilities[7].isActivate)
        {
            gameManager.timeRemaining = Mathf.Min(60.0f, gameManager.timeRemaining + 3f);
        }

        yield return new WaitForSeconds(duration);
        yield return enemyRoot.DOScale(Vector3.zero, duration).WaitForCompletion();
        enemyAnimator.SetTrigger("_Idle");

        // HP 회복 연출
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            hpSlider.value = Mathf.Lerp(0, 1, p);
            yield return null;
        }
        easeHPSlider.value = 1f;

        if ((floor - 1) % 20 == 0)
        {
            StartCoroutine(backgroundManager.ChangeBackground(floor / 20));
        }

        Setup();
        gameManager.UpdateTexts();
        if(GameManager.currentState == GameState.GameOver)
            GameManager.SetState(GameState.Playing);
    }

    /// <summary>
    /// 골드 획득
    /// </summary>
    private void RewardGold(int floor)
    {
        double bonus = partyManager.goldBuff + 1;
        double gain = 5 * Math.Pow(1.2, floor - 1) * bonus;
        DataManager.instance.gameData.gold = Math.Floor(DataManager.instance.gameData.gold + gain);
    }

    /// <summary>
    /// 적 공격(클릭)
    /// </summary>
    public void OnMouseDown()
    {
        var player = partyManager.mainPlayer;
        if (player != null && GameManager.currentState == GameState.Playing && player.stats.passive != Passive.PartyBuff)
        {
            AudioManager.instance.PlaySfx(3);
            player.TryAttack();
        }
    }

    private float GetEaseDuration() => DataManager.instance.gameData.abilities[6].isActivate ? 0.15f : 0.3f;

    private int GetBossScale(int floor)
    {
        if (floor >= 100) return 8;
        if (floor >= 90) return 7;
        return (floor / 10 - 1) / 2 + 3;
    }
}
