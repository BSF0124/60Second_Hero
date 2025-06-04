/// <summary>
/// 해상도 조건을 검증하는 유틸리티 클래스
/// </summary>
using UnityEngine;

public static class ResolutionUtility
{
    public static bool CheckMinimumResolution(int width) => width >= 1024;

    public static bool Check16To9Ratio(int width, int height)
    {
        float aspectRatio = (float)width / height;
        return Mathf.Approximately(aspectRatio, 16.0f / 9.0f);
    }

    public static bool CheckRefreshRateRatio(float refreshRate)
    {
        return Mathf.Approximately(refreshRate, 60f);
    }
}
