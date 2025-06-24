using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 저장 메뉴: 저장, 데이터 초기화, 종료 기능 처리
/// </summary>
public class SaveMenu : MonoBehaviour
{
    public Button saveButton;
    public Button dataResetButton;
    public Button exitButton;

    private bool isDataReset = false;
    private TextMeshProUGUI dataResetTMP => dataResetButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

    private void Awake()
    {
        saveButton.onClick.AddListener(Save);
        dataResetButton.onClick.AddListener(DataReset);
        exitButton.onClick.AddListener(Exit);
    }

    private void OnDisable()
    {
        isDataReset = false;
        dataResetButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "데이터 초기화";
    }

    /// <summary>
    /// 데이터 저장
    /// </summary>
    void Save()
    {
        AudioManager.instance.PlaySfx(0);
        DataManager.instance.SaveGameData();
    }

    /// <summary>
    /// 데이터 리셋
    /// </summary>
    void DataReset()
    {
        AudioManager.instance.PlaySfx(0);
        if (isDataReset)
        {
            StartCoroutine(SceneLoader.instance.ReloadScene());
        }

        else
        {
            isDataReset = true;
            dataResetTMP.text = "초기화 하시겠습니까?";
        }
    }
    
    /// <summary>
    /// 게임 종료
    /// </summary>
    void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else
        Application.Quit();
        
#endif
    }
}
