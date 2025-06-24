using UnityEngine;

/// <summary>
/// 해상도 유틸리티: 최소 해상도, 주사율, 비율 검증
/// </summary>
public static class ResolutionUtility
{
    public static bool CheckMinimumResolution(int width) => width >= 1024;

    public static bool Check16To9Ratio(int width, int height)
    {
        float aspect = (float)width / height;
        return Mathf.Approximately(aspect, 16f / 9f);
    }

    public static bool CheckRefreshRateRatio(float refreshRate)
    {
        return refreshRate >= 59.5f && refreshRate <= 60.5f;
    }
}
