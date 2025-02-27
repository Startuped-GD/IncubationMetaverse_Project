using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSelectionManager : MonoBehaviour

{
   
    public static PlayerSelectionManager instance;

    public PlayerDataCollector femalePlayerData, malePlayerData;

    internal PlayerDataCollector currPlayerData;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void SetPlayer(bool isFemale)
    {
        if (isFemale)
        {
            currPlayerData = femalePlayerData;
            malePlayerData.player.SetActive(false);
        }
        else
        {
            currPlayerData = malePlayerData;
            femalePlayerData.player.SetActive(false);
        }

        currPlayerData.player.SetActive(true);
    }
}

[System.Serializable]
public class PlayerDataCollector
{
    public GameObject player;
    public TextMeshPro nameText;
}
