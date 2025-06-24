public static class GameConstants
{
    // Player stats
    public const float DefaultAttackSpeedMultiplier = 0.5f;

    // GameManager.cs(Timer)
    public const float TimeLimit = 60f;

    // UpgradeUIManager.cs
    public const double GoldBaseUpgradeCost = 10;
    public const int MaxCriticalChanceLevel = 17;

    // AbilityButtons.cs
    public const int INDEX_PASSIVE = 2;
    public const int INDEX_REROLL = 5;
    public const int INDEX_SPEED = 6;
    public const int INDEX_LUCK = 8;

    // DamageText.cs 
    public const float DestroyThresholdY = -600f;

    // AudioSetting.cs
    public const int min_Volume = 0;
    public const int max_Volume = 10;
    public const string BGM_KEY = "BGMVolume";
    public const string SFX_KEY = "SFXVolume";

    // CameraMove.cs
    public const float moveUnit = 17.77f;

    // DataManager.cs
    public const string SaveFileName = "Data.json";
}
