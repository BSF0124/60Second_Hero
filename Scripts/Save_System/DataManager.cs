/// <summary>
/// 게임 데이터를 저장/불러오기 위한 매니저 클래스
/// </summary>
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    public GameData gameData = new();
    private string filePath;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.persistentDataPath, "save.dat");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGameData()
    {
        string json = JsonUtility.ToJson(gameData);
        string encrypted = AESHelper.Encrypt(json);
        File.WriteAllText(filePath, encrypted);
    }

    public void LoadGameData()
    {
        if (!File.Exists(filePath)) return;
        
        string encrypted = File.ReadAllText(filePath);
        string json = AESHelper.Decrypt(encrypted);
        gameData = JsonUtility.FromJson<GameData>(json);
    }

    public void ResetGameData()
    {
        gameData = new GameData();
        SaveGameData();
    }
}

[System.Serializable]
public class GameData
{
    public double gold = 0;
    public int abilityPoint = 0;
    public Ability[] abilities = new Ability[10];
    public Stats[] players = new Stats[1];
}

[System.Serializable]
public class Ability
{
    public bool isActivate;
    public string description;
}