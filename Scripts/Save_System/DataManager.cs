using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 어빌리티 상태
/// </summary>
[System.Serializable]
public class Ability
{
    public bool unlock = false;
    public bool isActivate = false;
}

/// <summary>
/// 게임 데이터 정보
/// </summary>
[System.Serializable]
public class GameData
{
    public double gold = 0;         // 골드
    public int abilityPoint = 0;    // 어빌리티 포인트
    public int highScore = 0;
    public int floor = 1;           // 층수
    public float timer = 0f;
    public List<Ability> abilities = new List<Ability>();
    public List<Stats> players = new List<Stats>();
    public List<EquippedItems> playeritems = new List<EquippedItems>();

    public GameData()
    {
        for (int i = 0; i < 9; i++) abilities.Add(new Ability());
    }
}

/// <summary>
/// 게임 저장 및 로드, 데이터 파일 경로 및 암호화 처리
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    public GameData gameData;

    private string filePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        filePath = Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);
        LoadGameData();
    }

    /// <summary>
    /// 게임 데이터 저장 (암호화)
    /// </summary>
    public void SaveGameData()
    {
        try
        {
            string json = JsonUtility.ToJson(gameData);
            string encrypted = AESHelper.Encrypt(json);
            File.WriteAllText(filePath, encrypted);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] Save failed: {e.Message}");
        }
    }

    /// <summary>
    /// 게임 데이터 불러오기 (복호화)
    /// </summary>
    public void LoadGameData()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                gameData = new GameData();
                SaveGameData();
                return;
            }

            string encrypted = File.ReadAllText(filePath);
            string json = AESHelper.Decrypt(encrypted);
            gameData = JsonUtility.FromJson<GameData>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataManager] Load failed: {e.Message}");
            ResetGameData();
        }
    }

    /// <summary>
    /// 게임 데이터 초기화
    /// </summary>
    public void ResetGameData()
    {
        gameData = new GameData();
        SaveGameData();
        Debug.Log("[DataManager] Game data reset.");
    }
}
