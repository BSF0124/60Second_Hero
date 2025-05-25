/// <summary>
/// 화면 해상도 및 전체화면 설정을 제어하는 UI 컴포넌트
/// </summary>
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSetting : MonoBehaviour
{
    public Button fullScreenBtn;
    public Button nextResolutionBtn;
    public Button previousResolutionBtn;
    public TextMeshProUGUI resolutionTMP;
    public Sprite[] fullScreenSprite;

    private List<Resolution> availableResolutions = new();
    private int resolutionIndex;
    private bool isFullScreen;

    private void Awake()
    {
        isFullScreen = Screen.fullScreen;
        UpdateFullScreenIcon();

        foreach (var res in Screen.resolutions)
        {
            if (ResolutionUtility.CheckMinimumResolution(res.width) &&
            ResolutionUtility.CheckRefreshRateRatio((float)res.refreshRateRatio.value) &&
            ResolutionUtility.Check16To9Ratio(res.width, res.height))
            {
                availableResolutions.Add(res);
            }
        }

        fullScreenBtn.onClick.AddListener(ToggleFullScreen);
        nextResolutionBtn.onClick.AddListener(NextResolution);
        previousResolutionBtn.onClick.AddListener(PreviousResolution);
    }

    private void OnEnable()
    {
        resolutionIndex = availableResolutions.FindIndex(r => r.width == Screen.width && r.height == Screen.height);
        if (resolutionIndex != -1)
        {
            resolutionTMP.text = GetResolutionText(availableResolutions[resolutionIndex]);
        }
    }

    private void ToggleFullScreen()
    {
        isFullScreen = !isFullScreen;
        Screen.fullScreen = isFullScreen;
        UpdateFullScreenIcon();
    }

    private void NextResolution()
    {
        if (resolutionIndex < availableResolutions.Count - 1)
        {
            resolutionIndex++;
            ApplyResolution();
        }
    }

    private void PreviousResolution()
    {
        if (resolutionIndex > 0)
        {
            resolutionIndex--;
            ApplyResolution();
        }
    }

    private void ApplyResolution()
    {
        var res = availableResolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        resolutionTMP.text = GetResolutionText(res);
    }

    private string GetResolutionText(Resolution res) => $"{res.width}x{res.height}";

    private void UpdateFullScreenIcon()
    {
        fullScreenBtn.GetComponent<Image>().sprite = isFullScreen ? fullScreenSprite[0] : fullScreenSprite[1];
    }
}