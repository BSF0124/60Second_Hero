using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 해상도 및 전체화면 설정 UI 관리
/// </summary>
public class ResolutionSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public Button fullScreenBtn;
    public Button nextResolutionBtn;
    public Button previousResolutionBtn;

    public TextMeshProUGUI resolutionTMP;
    public Sprite[] fullScreenSprite; // [0]: ON, [1]: OFF

    private List<ResolutionData> options = new();
    private int resolutionIndex;
    private bool isFullScreen = false;

    private void Awake()
    {
        isFullScreen = Screen.fullScreen;
        UpdateFullScreenIcon();

        foreach (Resolution res in Screen.resolutions)
        {
            if (ResolutionUtility.CheckMinimumResolution(res.width) &&
                ResolutionUtility.CheckRefreshRateRatio((float)res.refreshRateRatio.value) &&
                ResolutionUtility.Check16To9Ratio(res.width, res.height))
            {
                options.Add(new ResolutionData(res.width, res.height, res.refreshRateRatio));
            }
        }

        fullScreenBtn.onClick.AddListener(SetFullScreen);
        previousResolutionBtn.onClick.AddListener(PreviousResolution);
        nextResolutionBtn.onClick.AddListener(NextResolution);
    }

    private void OnEnable()
    {
        resolutionIndex = options.FindIndex(opt => opt.Width == Screen.width && opt.Height == Screen.height);
        if (resolutionIndex != -1)
            resolutionTMP.text = options[resolutionIndex].ToString();
    }

    /// <summary>
    /// 전체화면 전환
    /// </summary>
    void SetFullScreen()
    {
        AudioManager.instance.PlaySfx(0);
        isFullScreen = !isFullScreen;
        Screen.fullScreen = isFullScreen;
        UpdateFullScreenIcon();
    }

    /// <summary>
    /// 이전 해상도로 변경
    /// </summary>
    void PreviousResolution()
    {
        if (resolutionIndex > 0)
        {
            AudioManager.instance.PlaySfx(0);
            resolutionIndex--;
            ApplyResolution();
        }
    }

    /// <summary>
    /// 다음 해상도로 변경
    /// </summary>
    void NextResolution()
    {
        if (resolutionIndex < options.Count - 1)
        {
            AudioManager.instance.PlaySfx(0);
            resolutionIndex++;
            ApplyResolution();
        }
    }

    /// <summary>
    /// 해상도 적용
    /// </summary>
    void ApplyResolution()
    {
        var res = options[resolutionIndex];
        Screen.SetResolution(res.Width, res.Height, isFullScreen);
        resolutionTMP.text = res.ToString();
    }

    /// <summary>
    /// 전체화면 아이콘 업데이트
    /// </summary>
    void UpdateFullScreenIcon()
    {
        fullScreenBtn.GetComponent<Image>().sprite = isFullScreen ? fullScreenSprite[0] : fullScreenSprite[1];
    }
}
