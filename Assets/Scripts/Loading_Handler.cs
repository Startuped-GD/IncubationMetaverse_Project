using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;

public class Loading_Handler : MonoBehaviour
{
    public static Loading_Handler instance;

    [SerializeField] GameObject loadingPanel;
    [SerializeField] Image loadingFillBar;
    public TextMeshProUGUI LoadingText;

    Coroutine updateLoadingBarRoutine;

    private void Awake()
    {
      
            instance = this;
           // DontDestroyOnLoad(this.gameObject);
        
      
    }

    private void OnEnable()
    {
        SetLoadingPanel(true);
    }

    // Remove OnDisable() since it prematurely sets loadingPanel to inactive

    internal void SetLoadingPanel(bool status)
    {
        if (loadingPanel != null) loadingPanel.SetActive(status);
    }

    internal void UpdateLoadingBar(float loadingBarValue)
    {
        if (loadingFillBar != null)
        {
            // Update the fill amount of the loading bar
            loadingFillBar.fillAmount = Mathf.Clamp01(loadingBarValue);

            // Update the loading text
            UpdateLoadingText(loadingBarValue);
        }
    }

    internal void UpdateLoadingBar(AsyncOperation asyncOperation)
    {
        if (updateLoadingBarRoutine != null)
        {
            StopCoroutine(updateLoadingBarRoutine);
        }

        updateLoadingBarRoutine = StartCoroutine(UpdateLoading_Enumrator(asyncOperation));
    }

    internal void UpdateLoadingBar<T>(AsyncOperationHandle<T> asyncOperation)
    {
        if (updateLoadingBarRoutine != null)
        {
            StopCoroutine(updateLoadingBarRoutine);
        }

        updateLoadingBarRoutine = StartCoroutine(UpdateLoading_Enumrator(asyncOperation));
    }

    IEnumerator UpdateLoading_Enumrator(AsyncOperation asyncOperation)
    {
        SetLoadingPanel(true);

        while (!asyncOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(asyncOperation.progress);

            // Update both the loading bar and the loading text
            UpdateLoadingBar(progressValue);

            yield return null; // Wait for the next frame
        }

        // Ensure the loading bar and text show 100% when done
        UpdateLoadingBar(1f);
        SetLoadingPanel(false);
    }

    IEnumerator UpdateLoading_Enumrator<T>(AsyncOperationHandle<T> asyncOperation)
    {
        SetLoadingPanel(true);

        while (!asyncOperation.IsDone)
        {
            if (!asyncOperation.IsValid())
            {
                yield break;
            }

            if (asyncOperation.Status == AsyncOperationStatus.Failed)
            {
                Debug.Log("Operation Failed");
                yield break;
            }

            float progressValue = Mathf.Clamp01(asyncOperation.PercentComplete);

            // Update both the loading bar and the loading text
            UpdateLoadingBar(progressValue);

            yield return null; // Wait for the next frame
        }

        // Ensure the loading bar and text show 100% when done
        UpdateLoadingBar(1f);
        SetLoadingPanel(false);
    }

    private void UpdateLoadingText(float progress)
    {
        float percentage = progress * 100f;
        LoadingText.text = $"{percentage:0}%"; // Update the loading text
    }
}
