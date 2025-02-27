using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullMapHandler : MonoBehaviour
{
    public GameObject fullMap, fullMapCamera;

    [Header("Locations")]
    public MapLocations[] mapLocations;

    // Start is called before the first frame update
    void Start()
    {

    }


    public void Click_ViewFullMap(bool status)
    {
        fullMap.SetActive(status);
        fullMapCamera.SetActive(status);
        Zoom_Map();
        ThirdPersonController.instance.isControllingEnabled = !status;
    }

    public void SelectLocation(int index)
    {
        PlayerSelectionManager.instance.currPlayerData.player.transform.position = mapLocations[index].locationTransform.position;
    }

    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    void Zoom_Map()
    {
        // Zoom In
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && Camera.main.orthographicSize > minZoom)
        {
            Camera.main.orthographicSize -= zoomSpeed * Time.deltaTime;
        }

        // Zoom Out
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && Camera.main.orthographicSize < maxZoom)
        {
            Camera.main.orthographicSize += zoomSpeed * Time.deltaTime;
        }
    }
}

[System.Serializable]
public class MapLocations
{
    public Transform locationTransform;
}
