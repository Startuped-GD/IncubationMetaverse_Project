using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Addressable
{
    public class SceneManager_Addressable : MonoBehaviour
    {
        public static SceneManager_Addressable instance;

        public bool isAddressable;
        public int iniSceneIndex;
        public GameObject loadingPanel;
        public Image loadingFillBar;

        [Header("Use if want to show progress as text")]
        public string loadingMsg;
        public TextMeshProUGUI loadingPercentageText;

        [Header("Scene Data")]
        public SceneData[] sceneData;

        [Header("Prefab Data")]
        public PrefabData[] prefabData;

        AsyncOperation loadingOpreation;        //For lacal scene loading
        AsyncOperationHandle<SceneInstance> loadedAddressableScene;     //For addressable scene loading
        bool isPreviousSceneLoaded, isLoading, isSceneDownloadingCompleted, isPrefabDownloadingCompleted, isSceneInitiated;
        SceneInstance previousScene;

        AsyncOperationHandle downloadedPrefab, downloadedScene;

        internal GameObject instantiatedPrefab;
        Coroutine afterDownloadingRoutine;

        public object Initialize_AddressableScene { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            if (isAddressable)
            {
                StartCoroutine(DownloadAllScene());
            }
            else
            {
                isSceneInitiated = true;
                LoadSelectedScene(sceneData[iniSceneIndex]);
            }
        }

        #region Scene Downloader
        IEnumerator DownloadAllScene()
        {
            //Scenes
            List<string> sceneKeyList = new List<string>();
            for (int i = 0; i < sceneData.Length; i++)
            {
                sceneKeyList.Add(sceneData[i].sceneAddressableKey);
            }
           

            downloadedScene = Addressables.DownloadDependenciesAsync(sceneKeyList, Addressables.MergeMode.Union);
            downloadedScene.Completed += (DS) =>
            {
                isSceneDownloadingCompleted = true;
                if (afterDownloadingRoutine != null)
                {
                    StopCoroutine(afterDownloadingRoutine);
                }
                afterDownloadingRoutine = StartCoroutine(AfterDownloading());
            };

            //Prefabs

            if (prefabData.Length > 0)
            {
                List<string> prefabKeyList = new List<string>();
                for (int i = 0; i < prefabData.Length; i++)
                {
                    prefabKeyList.Add(prefabData[i].prefabAddressableKey);
                }

                //float prefabDownloadingSize = Addressables.GetDownloadSize(prefabKeyList).Result;

                downloadedPrefab = Addressables.DownloadDependenciesAsync(prefabKeyList, Addressables.MergeMode.Union);
                downloadedPrefab.Completed += (DP) =>
                {
                    isPrefabDownloadingCompleted = true;
                    if (afterDownloadingRoutine != null)
                    {
                        StopCoroutine(afterDownloadingRoutine);
                    }
                    afterDownloadingRoutine = StartCoroutine(AfterDownloading());
                };
            }
            else
            {
                isPrefabDownloadingCompleted = true;
                if (afterDownloadingRoutine != null)
                {
                    StopCoroutine(afterDownloadingRoutine);
                }
                afterDownloadingRoutine = StartCoroutine(AfterDownloading());
            }

            yield return null;
        }
        #endregion

        IEnumerator AfterDownloading()
        {
            yield return new WaitForEndOfFrame();

            if (isSceneDownloadingCompleted && isPrefabDownloadingCompleted && !isSceneInitiated)
            {
                isSceneInitiated = true;
                CheckInternet.instance.StopChecking();
                LoadSelectedScene(sceneData[iniSceneIndex]);
            }
        }

        #region Scene Loader
        //To control loading of selected scene using Addressable/Local
        internal void LoadSelectedScene(SceneData currSceneData)
        {
            if (isAddressable)
            {
                LoadAddressableScene(currSceneData.sceneAddressableKey);
            }
            else
            {
                LoadLocalScene(currSceneData.sceneName);
            }
        }

        //To load addressable scene
        void LoadAddressableScene(string addressableKey)
        {
            loadingPanel.SetActive(true);

            //if (isPreviousSceneLoaded)
            //{
            //    AsyncOperationHandle<SceneInstance> unloadedScene = Addressables.UnloadSceneAsync(previousScene);
            //    Addressables.Release<SceneInstance>(unloadedScene);
            //}

            isPreviousSceneLoaded = false;
            previousScene = new SceneInstance();
            loadedAddressableScene = Addressables.LoadSceneAsync(addressableKey, LoadSceneMode.Single);
            isLoading = true;
            loadedAddressableScene.Completed += (LS) =>
            {
                previousScene = LS.Result;
                isPreviousSceneLoaded = true;
            };
        }

        //To load local scene
        void LoadLocalScene(string sceneName)
        {
            //Debug.Log("Local scene");
            loadingOpreation = SceneManager.LoadSceneAsync(sceneName);
            loadingPanel.SetActive(true);
            isLoading = true;
        }
        #endregion

        #region Prefab Loader
        public GameObject LoadSelectedPrefab(PrefabData currPrefabData, Transform prefabParent)
        {
            return LoadLocalPrefab(currPrefabData.prefab, prefabParent);
            //if (isAddressable)
            //{
            //    return LoadAddressablePrefab(currPrefabData.prefabAddressableKey, prefabParent);
            //}
            //else
            //{
            //    return LoadLocalPrefab(currPrefabData.prefab, prefabParent);
            //}
        }

        //GameObject LoadAddressablePrefab(string prefabAddressableKey, Transform prefabParent)
        //{
        //    instantiatedPrefab = null;
        //    AsyncOperationHandle<GameObject> prefabToLoad = Addressables.LoadAssetAsync<GameObject>(prefabAddressableKey);
            
        //    prefabToLoad.Completed += (pl) =>
        //    {
        //        GameObject obj = prefabToLoad.Result;

        //        if (prefabParent == null)
        //        {
        //            instantiatedPrefab = Instantiate(obj);
        //        }
        //        else
        //        {
        //            instantiatedPrefab = Instantiate(obj, prefabParent);
        //        }

        //        Debug.Log("prefab instantiated- "+instantiatedPrefab.name);
        //    };

        //    return instantiatedPrefab;
        //}

        GameObject LoadLocalPrefab(GameObject prefab, Transform prefabParent)
        {
            if (prefabParent == null)
            {
                GameObject obj = Instantiate(prefab);
                return obj;
            }
            else
            {
                GameObject obj = Instantiate(prefab, prefabParent);
                return obj;
            }
        }
        #endregion

        private void Update()
        {
            if (isLoading)
            {
                float progressValue = 0;
                float progressPercentageValue = 0;
                if (isAddressable)
                {
                    progressValue = Mathf.Clamp01(loadedAddressableScene.PercentComplete);

                    //Show progress as text (Old method will remove in new version)
                    if (loadingPercentageText != null)
                    {
                        progressPercentageValue = Mathf.Round(progressValue * 10000) / 100;//till 2 decimal
                        loadingPercentageText.SetText(loadingMsg + progressPercentageValue + "%");
                    }
                    else //Show progress as fill bar (New Method)
                    {
                        loadingFillBar.fillAmount = progressValue;
                    }

                    if (loadedAddressableScene.IsDone)
                    {
                        loadingPanel.SetActive(false);
                        isLoading = false;
                    }
                }
                else if(loadingOpreation != null)
                {
                    progressValue = Mathf.Clamp01(loadingOpreation.progress);

                    //Show progress as text (Old method will remove in new version)
                    if (loadingPercentageText != null)
                    {
                        progressPercentageValue = Mathf.Round(progressValue * 100);
                        loadingPercentageText.SetText(loadingMsg + progressPercentageValue + "%");
                    }
                    else //Show progress as fill bar (New Method)
                    {
                        loadingFillBar.fillAmount = progressValue;
                    }

                    if (loadingOpreation.isDone)
                    {
                        loadingPanel.SetActive(false);
                        isLoading = false;
                    }
                }
            }
            else
            if (!isSceneInitiated)  //while downloading scenes
            {
                loadingPanel.SetActive(true);
                if (downloadedScene.Status == AsyncOperationStatus.Failed || downloadedPrefab.Status == AsyncOperationStatus.Failed)
                {
                    //Initialize_AddressableScene.instance.warningPopup.SetActive(true);
                    Debug.Log("Operation Failed");
                }

                float progressValue = Mathf.Clamp01((downloadedScene.PercentComplete + downloadedPrefab.PercentComplete)/2);

                //Show progress as text (Old method will remove in new version)
                if (loadingPercentageText != null)
                {
                    float progressPercentageValue = Mathf.Round(progressValue * 100); //till 2 decimal
                    loadingPercentageText.SetText(loadingMsg + progressPercentageValue + "%");
                }
                else //Show progress as fill bar (New Method)
                {
                    loadingFillBar.fillAmount = progressValue;
                }
                
                if (downloadedScene.IsDone && downloadedPrefab.IsDone)
                {
                    loadingPanel.SetActive(false);
                    //isDownloading = false;
                }
            }
        }
    }

    [System.Serializable]
    public class SceneData
    {
        public int sceneIndex;
        public string sceneName, sceneAddressableKey;
    }

    [System.Serializable]
    public class PrefabData 
    {
        public string prefabName, prefabAddressableKey;
        public GameObject prefab;
    }

}
