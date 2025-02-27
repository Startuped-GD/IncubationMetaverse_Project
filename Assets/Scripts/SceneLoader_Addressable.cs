using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Addressable
{
    public class SceneLoader_Addressable : MonoBehaviour
    {
        public static SceneLoader_Addressable instance;

        internal static int currSceneIndex;

        private void Awake()
        {
            instance = this;
        }

        //Load scene using index
        public void LoadScene(int index)
        {
            SceneData[] sd = SceneManager_Addressable.instance.sceneData;

            for (int i = 0; i < sd.Length; i++)
            {
                if (sd[i].sceneIndex == index)
                {
                    SceneManager_Addressable.instance.LoadSelectedScene(sd[i]);
                    currSceneIndex = sd[i].sceneIndex;
                    break;
                }
            }
        }

        //Load scene using name
        public void LoadScene(string sceneName)
        {
            SceneData[] sd = SceneManager_Addressable.instance.sceneData;

            for (int i = 0; i < sd.Length; i++)
            {
                if (sd[i].sceneName == sceneName)
                {
                    SceneManager_Addressable.instance.LoadSelectedScene(sd[i]);
                    currSceneIndex = sd[i].sceneIndex;
                    break;
                }
            }
        }

        public void LoadCurrentScene()
        {
            LoadScene(currSceneIndex);
        }

    }
}
