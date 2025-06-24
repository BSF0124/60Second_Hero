using UnityEngine;

/// <summary>
/// 해상도 데이터를 저장하는 클래스
/// </summary>
public class ResolutionData
{
    public readonly int Width;
    public readonly int Height;
    public RefreshRate RefreshRateRatio;

    public ResolutionData(int width, int height, RefreshRate refreshRateRatio)
    {
        Width = width;
        Height = height;
        RefreshRateRatio = refreshRateRatio;
    }

    public override string ToString() => $"{Width}x{Height}";
}
