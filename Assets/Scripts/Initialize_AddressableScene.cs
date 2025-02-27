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

namespace Addressable
{
    public class Initialize_AddressableScene : MonoBehaviour
    {
        public static Initialize_AddressableScene instance;

        public GameObject loadingPanel, moduleParent, warningPopup;
        public Text loadingPercentageText;

        public string serverPath, catalogName;
        public string assetPathLocator;     //Path for assets on addressable group window

        Scene initiallyLoadedScene;     //to store scene reference for initially directly loaded scene
        internal SceneInstance initialAddressableSceneInstance; //to store scene instance for initially loaded scene using addressable
        internal bool isPreviousAddressableSceneLoaded, isDownloading, isLoading;
        internal IResourceLocator resource_Locator;

        string _JsonCatalogPath;

        AsyncOperationHandle<IResourceLocator> loadContentCatalogAsync;
        AsyncOperationHandle<SceneInstance> loadedScene;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                //Addressable.BackButton_Addressable.instance.Remove_From_DontDestroy(instance.gameObject);
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);
            //Addressable.BackButton_Addressable.instance.Add_To_DontDestroy(this.gameObject);
        }

        public void LoadModule(string moduleName)
        {
            moduleParent.SetActive(false);
            _JsonCatalogPath = serverPath + "/" + moduleName + "_" + catalogName;
            loadingPanel.SetActive(true);
            initiallyLoadedScene = SceneManager.GetActiveScene();

            //double sceneDownloadingSize = Addressables.GetDownloadSize(@_JsonCatalogPath).Result;
            //Debug.Log("content download size- " + sceneDownloadingSize);
            ////Check internet availabilty
            //if (sceneDownloadingSize > 0)
            //{
            //    CheckInternet.instance.CheckAvailabiltyInLoop();
            //}

            loadContentCatalogAsync = Addressables.LoadContentCatalogAsync(@_JsonCatalogPath);
            isDownloading = true;
            loadContentCatalogAsync.Completed += OnCompleted;
        }

        public void LoadModuleUsingPath(string jsonCatalogPath)
        {
            moduleParent.SetActive(false);
            loadingPanel.SetActive(true);

            //double sceneDownloadingSize = Addressables.GetDownloadSize(@jsonCatalogPath).Result;
            //Debug.Log("Scene download size- " + sceneDownloadingSize);
            ////Check internet availabilty
            //if (sceneDownloadingSize > 0)
            //{
            //    CheckInternet.instance.CheckAvailabiltyInLoop();
            //}

            initiallyLoadedScene = SceneManager.GetActiveScene();
            loadContentCatalogAsync = Addressables.LoadContentCatalogAsync(@jsonCatalogPath);
            isDownloading = true;
            loadContentCatalogAsync.Completed += OnCompleted;
        }

        //To load initial scene
        private void OnCompleted(AsyncOperationHandle<IResourceLocator> obj)
        {
            IResourceLocator resourceLocator = obj.Result;
            resource_Locator = resourceLocator;
            resourceLocator.Locate(assetPathLocator, typeof(SceneInstance), out IList<IResourceLocation> locations);
            IResourceLocation resourceLocation = locations[0];

            float contentCatalogSize = Addressables.GetDownloadSize(resourceLocation).Result;
            //if (contentCatalogSize != 0)
            //{
            //    CheckInternet.instance.CheckAvailabiltyInLoop();
            //}

            loadedScene = Addressables.LoadSceneAsync(resourceLocation, LoadSceneMode.Additive);
            isLoading = true;
            isDownloading = false;
            loadedScene.Completed += (sceneHandler) =>
            {
                initialAddressableSceneInstance = sceneHandler.Result;
                isPreviousAddressableSceneLoaded = true;
                loadingPanel.SetActive(false);
                SceneManager.UnloadSceneAsync(initiallyLoadedScene);
            };
        }

        private void Update()
        {
            if (isLoading)  //For download and loading scene
            {
                float progressValue = Mathf.Clamp01(loadedScene.PercentComplete);
                float progressPercentageValue = Mathf.Round(progressValue * 9000)/100;//till 2 decimal
                progressPercentageValue += 10;
                loadingPercentageText.text = progressPercentageValue + "%";
                if (loadedScene.IsDone)
                {
                    CheckInternet.instance.StopChecking();
                    //loadingPanel.SetActive(false);
                    isLoading = false;
                }
            }
            else
            if (isDownloading)  //For catalog.json downloading
            {
                if (loadContentCatalogAsync.Status == AsyncOperationStatus.Failed)
                {
                    warningPopup.SetActive(true);
                    Debug.Log("Operation Failed");
                }
                
                float progressValue = Mathf.Clamp01(loadContentCatalogAsync.PercentComplete);
                float progressPercentageValue = Mathf.Round(progressValue * 1000)/100;//till 2 decimal
                loadingPercentageText.text = progressPercentageValue + "%";
                if (loadContentCatalogAsync.IsDone)
                {
                    //loadingPanel.SetActive(false);
                    isDownloading = false;
                }
            }
        }
    }
}
