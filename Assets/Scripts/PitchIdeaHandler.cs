using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PitchIdeaHandler : MonoBehaviour
{

    public static PitchIdeaHandler instance;
    int  Ipt;
    public Text iptCoinText;
    public GameObject pitchIdeaBtn, pitchResponsePanel, emptyWarning, pitchIdeaCamera;

    InputField pitchResponseInputField;
    int collectedIpt;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        pitchResponseInputField = pitchResponsePanel.GetComponentInChildren<InputField>();
    }

    public void ClickPitchIdea()
    {
        pitchIdeaBtn.SetActive(false);
        pitchResponsePanel.SetActive(true);
        pitchIdeaCamera.SetActive(true);
        PlayerSelectionManager.instance.currPlayerData.player.SetActive(false);
        ThirdPersonController.instance.isControllingEnabled = false;
    }

    public void ClickSubmit()
    {
        if (pitchResponseInputField.text != "")
        {
            pitchIdeaBtn.SetActive(true);
            pitchResponsePanel.SetActive(false);
            AddIPTCoin(100000);
            pitchResponseInputField.text = "";
            pitchIdeaCamera.SetActive(false);
            PlayerSelectionManager.instance.currPlayerData.player.SetActive(true);
            ThirdPersonController.instance.isControllingEnabled = true;
        }
        else
        {
            StartCoroutine(EmptyWarningRoutine());
        }
    }

    IEnumerator EmptyWarningRoutine()
    {
        emptyWarning.SetActive(true);
        yield return new WaitForSeconds(2);
        emptyWarning.SetActive(false);
    }

    public void AddIPTCoin(int amount)
    {
        collectedIpt += amount;
       
       int iptAmount = collectedIpt;
      //  iptCoinText.text = "$" + collectedIpt;
    }
}
