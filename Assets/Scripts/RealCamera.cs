using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealCamera : MonoBehaviour
{
    public static RealCamera instance;

    public GameObject targetPlayer;
    public Transform cameraTransform;

    [Header("Initial vectors")]
    public Vector3 iniPos;
    public Vector3 iniAngle;

    [Header("nearest to player")]
    public Vector3 nearestPos;
    public Vector3 nearestAngle;
    public float smoothSpeed;

    internal bool isPlayerMoved;

    bool isTriggered;
    Coroutine moveRoutine;
    string hitName="";

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        transform.localPosition = iniPos;
        transform.localEulerAngles = iniAngle;
    }

    private void OnDisable()
    {
        isTriggered = false;
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }
        cameraTransform.localPosition = iniPos;
        cameraTransform.localEulerAngles = iniAngle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.name.Equals(targetPlayer.name) && (other.gameObject.layer != 4 && other.gameObject.layer != 2))
        {
            if (!isTriggered)
            {
                Debug.Log("triggered in- " + other.gameObject.name, other.gameObject);
                isTriggered = true;
                hitName = other.gameObject.name;
                if (moveRoutine != null)
                {
                    StopCoroutine(moveRoutine);
                }
                moveRoutine = StartCoroutine(MoveCameraTo(nearestPos, nearestAngle));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.name.Equals(targetPlayer.name) && (other.gameObject.layer != 4 && other.gameObject.layer != 2))
        {
            if (isTriggered && other.gameObject.name.Equals(hitName))
            {
                Debug.Log("triggered exit- " + other.gameObject.name, other.gameObject);
                isTriggered = false;
                if (moveRoutine != null)
                {
                    StopCoroutine(moveRoutine);
                }
                moveRoutine = StartCoroutine(MoveCameraTo(iniPos, iniAngle));
            }
        }
    }

    IEnumerator MoveCameraTo(Vector3 targetpPos, Vector3 targetAngle)
    {
        float timeCount = 0;

        Vector3 currPos = cameraTransform.localPosition;
        Vector3 currAngle = cameraTransform.localEulerAngles;

        while (timeCount < 1)
        {
            timeCount += smoothSpeed * Time.deltaTime;

            cameraTransform.localPosition = Vector3.Lerp(currPos, targetpPos, timeCount);
            cameraTransform.localEulerAngles = Vector3.Lerp(currAngle, targetAngle, timeCount);

            yield return new WaitForSeconds(0.01f);
        }
    }
}
