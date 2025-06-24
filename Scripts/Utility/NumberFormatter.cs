/// <summary>
/// 큰 숫자를 알파벳 단위로 축약하여 문자열로 변환(ex. 1A, 1.5B 등)
/// </summary>
public static class NumberFormatter
{
    public static string FormatNumber(double num)
    {
        if (num < 1000) return num.ToString("0");

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
