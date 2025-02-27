using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActivityHandler : MonoBehaviour
{
    public static ActivityHandler instance;

    [Header("Office Triggers")]
    public OfficeTriggers[] officeTriggers;

    [Header("UI Panels")]
    public GameObject enterInOfficeBtn;
    public GameObject exitFromOfficeBtn, focusTvBtn, exitTvFocusBtn;

    [Header("TV focus camera")]
    public GameObject tvFocusCamera;

    int currTriggerIndex;

    private void Awake()
    {
        instance = this;
    }

    internal void OnTrigger_EnterInOffice(bool status, int index)
    {
        currTriggerIndex = index;
        enterInOfficeBtn.SetActive(status);
        exitFromOfficeBtn.SetActive(false);
    }

    internal void OnTrigger_ExitFromOffice(bool status, int index)
    {
        currTriggerIndex = index;
        enterInOfficeBtn.SetActive(false);
        exitFromOfficeBtn.SetActive(status);
    }

    public void ClickOn_EnterInOffice()
    {
        OnTrigger_ExitFromOffice(true, currTriggerIndex);
        ThirdPersonController.instance.isControllingEnabled = false;
        PlayerSelectionManager.instance.currPlayerData.player.transform.position = officeTriggers[currTriggerIndex].exitFromOffice.transform.position;
        StartCoroutine(EnablePlayerMovement_Routine());
    }

    public void ClickOn_ExitFromOffice()
    {
        OnTrigger_EnterInOffice(true, currTriggerIndex);
        ThirdPersonController.instance.isControllingEnabled = false;
        PlayerSelectionManager.instance.currPlayerData.player.transform.position = officeTriggers[currTriggerIndex].enterInOffice.transform.position;
        StartCoroutine(EnablePlayerMovement_Routine());
    }

    IEnumerator EnablePlayerMovement_Routine()
    {
        yield return new WaitForSeconds(1);
        ThirdPersonController.instance.isControllingEnabled = true;
    }

    #region Focus TV mode

    public void Set_FocusOnTV(bool status)
    {
        focusTvBtn.SetActive(!status);
        exitTvFocusBtn.SetActive(status);
        tvFocusCamera.SetActive(status);
        ThirdPersonController.instance.isControllingEnabled = !status;
        PlayerSelectionManager.instance.currPlayerData.player.SetActive(!status);
    }

    #endregion
}

[System.Serializable]
public class OfficeTriggers
{
    public GameObject enterInOffice, exitFromOffice;
}
