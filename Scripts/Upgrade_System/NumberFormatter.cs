/// <summary>
/// 숫자 단위를 보기 쉬운 형태로 포맷합니다. (예: 1.2A, 3.4C)
/// </summary>
public static class NumberFormatter
{
    public static string FormatNumber(double num)
    {
        if (num < 1000)
            return num.ToString("0");

        int index = 0;
        while (num >= 1000)
        {
            num /= 1000;
            index++;
        }

        return num.ToString("0.##") + GetLetterSuffix(index);
    }

    private static string GetLetterSuffix(int index)
    {
        string suffix = "";
        while (index > 0)
        {
            index--;
            suffix = (char)('A' + (index % 26)) + suffix;
            index /= 26;
        }

        return suffix;
    }
}
