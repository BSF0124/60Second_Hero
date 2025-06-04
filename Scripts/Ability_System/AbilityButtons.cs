using TMPro;
using UnityEngine;

public class AbilityButtons : MonoBehaviour
{
    public TextMeshProUGUI[] buttonTexts;

    private void OnEnable()
    {
        for (int i = 0; i < buttonTexts.Length; i++)
        {
            buttonTexts[i].text = DataManager.instance.gameData.abilities[i].isActivate
            ? "<color=#00FF00>해금됨</color>"
            : DataManager.instance.gameData.abilities[i].description;
        }
    }

    public void Unlock(int index)
    {
        if (DataManager.instance.gameData.abilityPoint <= 0) return;
        if (DataManager.instance.gameData.abilities[index].isActivate) return;

        DataManager.instance.gameData.abilities[index].isActivate = true;
        DataManager.instance.gameData.abilityPoint--;
        DataManager.instance.SaveGameData();
        OnEnable();
    }
}
