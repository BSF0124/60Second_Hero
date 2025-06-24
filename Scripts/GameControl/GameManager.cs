using System.Collections;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;

public enum GameState
{
    Paused,
    Playing,
    GameOver,
    Ended
}

/// <summary>
/// 게임 흐름과 상태를 전반적으로 관리하는 클래스
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameState currentState { get; private set; } = GameState.Paused;

    public static void SetState(GameState state)
    {
        currentState = state;
    }

    public PartyManager partyManager;
    public BackgroundManager backgroundManager;
    public CameraMove cameraMove;
    public Enemy enemy;
    public UpgradeUIManager upgradeUIManager;

    public RectTransform resetButton;

    [Header("Texts")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI floorText;

    public float timeRemaining = GameConstants.TimeLimit;
    public bool timerCulc = false;
    
    public GameObject[] hideObjectLists;

    private void Awake()
    {
        SetState(GameState.Paused);
    }

    /// <summary>
    /// 게임 시작 시 호출: 저장된 유닛 로드 + 초기화
    /// </summary>
    public void StartGame()
    {
        LoadParty();
        ResetGame();
        UpdateTexts();
        upgradeUIManager.UpdateGold();

        // 어빌리티 6번: 배속모드 BGM
        AudioManager.instance.PlayBgm(DataManager.instance.gameData.abilities[6].isActivate ? 1 : 0);
        AudioManager.instance.bgmPlayer.loop = true;
        timerCulc = true;

        if (!DataManager.instance.gameData.abilities[2].isActivate)
            SetState(GameState.Playing);
    }

    private void LoadParty()
    {
        for(int i = 0; i < DataManager.instance.gameData.players.Count; i++)
        {
            Vector3 targetPos = new Vector3(-10.5f + (i / 8 * -18) + (i % 8 * -2f), -1.5f, 0);
            partyManager.LoadPlayer(DataManager.instance.gameData.players[i], DataManager.instance.gameData.playeritems[i], targetPos);
        }
    }

    private void Update()
    {
        if(timerCulc)
        {
            DataManager.instance.gameData.timer += Time.deltaTime;
        }

        if(currentState != GameState.Playing) return;

        float delta = DataManager.instance.gameData.abilities[6].isActivate ? Time.deltaTime * 2 : Time.deltaTime;
        timeRemaining -= delta;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            timerText.text = timeRemaining.ToString("F2");
            timerText.color = Color.red;
            SetState(GameState.GameOver);
            StartCoroutine(GameOver(false));
        }
        else
        {
            timerText.text = timeRemaining.ToString("F2");
            timerText.color = Color.Lerp(Color.red, Color.white, timeRemaining / GameConstants.TimeLimit);
        }
    }

    /// <summary>
    /// 게임 오버 처리(승리 or 시간초과)
    /// </summary>
    public IEnumerator GameOver(bool isWin)
    {
        SetState(GameState.GameOver);

        if(isWin)
        {
            foreach(GameObject obj in hideObjectLists)
                obj.SetActive(false);

            SetState(GameState.Ended);
            timerCulc = false;
            timerText.text = "";
            floorText.text = "";
            yield return new WaitForSeconds(2.5f);

            foreach(Transform unit in partyManager.transform)
            {
                unit.GetChild(0).GetComponent<SetRandomItem>().SetAlpha(true);
                unit.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("Move");
            }
            partyManager.isPartyMove = true;

            yield return floorText.DOText($"총 {partyManager.transform.childCount}명의 용사와 함께 탑을 정복했습니다.", 1f).WaitForCompletion();
            yield return new WaitForSeconds(2.5f);

            timerText.text = $"<color=#FFFFFF>플레이 타임: {(int)DataManager.instance.gameData.timer / 60}분 {(int)DataManager.instance.gameData.timer % 60}초 {(int)(DataManager.instance.gameData.timer * 100) % 100}</color>";
            AudioManager.instance.PlaySfx(9);
            yield return timerText.GetComponent<RectTransform>().DOShakeAnchorPos(0.5f, 30, 30).WaitForCompletion();
            yield return new WaitForSeconds(2.5f);

            resetButton.localScale = Vector2.zero;
            resetButton.gameObject.SetActive(true);
            yield return resetButton.DOScale(1f, 0.5f).SetEase(Ease.OutBack).WaitForCompletion();
        }
        else
        {
            AudioManager.instance.PlaySfx(5);
            float duration = DataManager.instance.gameData.abilities[6].isActivate ? 0.25f : 0.5f;
            upgradeUIManager.abilityButton.onClick.RemoveListener(upgradeUIManager.ShowAbilityPanel);

            timerText.text = "GAME OVER";
            AudioManager.instance.PlaySfx(9);
            yield return timerText.GetComponent<RectTransform>().DOShakeAnchorPos(duration, 30, 30).WaitForCompletion();

            yield return new WaitForSeconds(duration);

            upgradeUIManager.abilityButton.onClick.AddListener(upgradeUIManager.AddParty);
            upgradeUIManager.abilityPointText.text = "<color=#00B705>파티 추가</color>";
            AudioManager.instance.PlaySfx(9);
            yield return upgradeUIManager.abilityPointText.GetComponent<RectTransform>().DOShakeAnchorPos(duration, 30, 30).WaitForCompletion();

        }
    }

    /// <summary>
    /// 게임 초기화(파티 생성, 상태 리셋)
    /// </summary>
    public void ResetGame()
    {
        partyManager.CreateNewPlayer();
        cameraMove.SetMaxPosition();

        DataManager.instance.SaveGameData();
        DataManager.instance.gameData.floor = 1;
        floorText.text = $"{DataManager.instance.gameData.floor}층";

        if (!DataManager.instance.gameData.abilities[4].isActivate)
            DataManager.instance.gameData.gold = 0;

        enemy.maxHP = 10;
        enemy.Setup();

        timeRemaining = GameConstants.TimeLimit;

        upgradeUIManager.abilityButton.onClick.RemoveListener(upgradeUIManager.AddParty);
        upgradeUIManager.abilityButton.onClick.AddListener(upgradeUIManager.ShowAbilityPanel);
        upgradeUIManager.abilityPointText.text = DataManager.instance.gameData.abilityPoint > 0 ?
            $"어빌리티 <color=#00FF00>({DataManager.instance.gameData.abilityPoint})</color>" : "어빌리티";
        upgradeUIManager.UpdateGold();

        StartCoroutine(backgroundManager.ResetBackground());
        if(!DataManager.instance.gameData.abilities[2].isActivate)
            SetState(GameState.Playing);
    }

    /// <summary>
    /// 골드, 층수 텍스트 UI 업데이트
    /// </summary>
    public void UpdateTexts()
    {
        StringBuilder goldStr = new();
        goldStr.Append("<color=#50C878>$" + NumberFormatter.FormatNumber(DataManager.instance.gameData.gold) + "</color>");

        if (partyManager.goldBuff > 0)
                goldStr.Append("<color=#00B705>(" + partyManager.goldBuff * 100 + "%)</color>");

        goldText.text = goldStr.ToString();
        floorText.text = $"{DataManager.instance.gameData.floor}층";
    }
    
    public void ResetGameButton()
    {
        StartCoroutine(SceneLoader.instance.ReloadScene());
    }

    private void OnApplicationQuit()
    {
        // Process.GetCurrentProcess().Kill();
    }
}
