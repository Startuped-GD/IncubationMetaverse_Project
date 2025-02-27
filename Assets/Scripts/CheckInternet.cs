using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CheckInternet : MonoBehaviour
{
    public static CheckInternet instance;
    public GameObject errorPopPanel;

    Coroutine checkAvailabiltyCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    //To check internet availabilty at once
    public void CheckAvailabilty()
    {
        StopChecking();
        checkAvailabiltyCoroutine = StartCoroutine(SendWebRequest());
    }
    IEnumerator SendWebRequest()
    {
        UnityWebRequest request = new UnityWebRequest("http://google.com");
        yield return request.SendWebRequest();
        errorPopPanel.SetActive(request.error != null);
    }

    //To stop checking
    public void StopChecking()
    {
        if (checkAvailabiltyCoroutine != null)
        {
            StopCoroutine(checkAvailabiltyCoroutine);
        }
    }

    //To check internet availabilty during downloading
    public void CheckAvailabiltyInLoop() 
    {
        StopChecking();
        checkAvailabiltyCoroutine = StartCoroutine(SendWebRequesInLoop());
    }
    IEnumerator SendWebRequesInLoop()
    {
        while (true)
        {
            UnityWebRequest request = new UnityWebRequest("http://google.com");
            yield return request.SendWebRequest();
            errorPopPanel.SetActive(request.error != null);

            if (request.error != null)
            {
                yield break;
            }
            yield return new WaitForSeconds(1);
        }
    }
}
