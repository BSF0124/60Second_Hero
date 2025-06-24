using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메뉴 버튼 이벤트 처리 및 패널 토글 관리
/// </summary>
public class MenuButtons : MonoBehaviour
{
    public GameObject[] panels;

    public Button howtoPlayBtn;
    public Button soundBtn;
    public Button displayBtn;
    public Button saveBtn;
    public Button pauseBtn;

    public TextMeshProUGUI timerTMP;
    public Sprite pauseSprite;
    public Sprite resumeSprite;

    private enum PanelType { HowToPlay = 0, Sound, Display, Save }

    private void Awake()
    {
        howtoPlayBtn.onClick.AddListener(() => TogglePanel(PanelType.HowToPlay));
        soundBtn.onClick.AddListener(() => TogglePanel(PanelType.Sound));
        displayBtn.onClick.AddListener(() => TogglePanel(PanelType.Display));
        saveBtn.onClick.AddListener(() => TogglePanel(PanelType.Save));
        pauseBtn.onClick.AddListener(TogglePause);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideAllPanels();
        }
    }

    /// <summary>
    /// 해당 패널만 토글로 켜고 나머지는 모두 끔
    /// </summary>
    private void TogglePanel(PanelType panelType)
    {
        int index = (int)panelType;
        if (panels[index].activeSelf)
        {
            HideAllPanels();
        }
        else
        {
            if (panelType == PanelType.HowToPlay) GameManager.SetState(GameState.GameOver);
            HideAllPanels();
            panels[index].SetActive(true);
            AudioManager.instance.PlaySfx(0);
        }
    }

     /// <summary>
    /// 일시정지 토글
    /// </summary>
    public void TogglePause()
    {
        if (GameManager.currentState == GameState.Paused)
        {
            GameManager.SetState(GameState.Playing);
            AudioManager.instance.PlaySfx(2);
            pauseBtn.GetComponent<Image>().sprite = pauseSprite;
        }
        else
        {
            GameManager.SetState(GameState.Paused);
            AudioManager.instance.PlaySfx(1);
            timerTMP.text = "<color=#FFFFFF>일시 정지</color>";
            pauseBtn.GetComponent<Image>().sprite = resumeSprite;
        }
    }

    /// <summary>
    /// 모든 패널 숨김 처리
    /// </summary>
    public void HideAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }
}
