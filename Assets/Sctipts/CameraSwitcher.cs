using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] internal CinemachineVirtualCamera[] virtualCameras;

    int currentCameraIndex = 0;

    CinemachineVirtualCamera currentCamera;

    public static readonly int INITIALCAMERA = 0, WALLFALLINGCAMERA = 1, BOSSSAVEROOM = 2, BOSSROOM = 3, WALLKICKCAMERA = 4;

    private const int ACTIVE_CAMERA_PRIORITY = 10, INACTIVE_CAMERA_PRIORITY = 0;

    private void Start()
    {
        foreach (var camera in virtualCameras)
        {
            if (camera == virtualCameras[INITIALCAMERA])
            {
                camera.Priority = ACTIVE_CAMERA_PRIORITY;
                continue;
            }
            camera.Priority = INACTIVE_CAMERA_PRIORITY;
        }
    }

    public void SwitchCamera(int nextCameraIndex)
    {
        virtualCameras[currentCameraIndex].Priority = INACTIVE_CAMERA_PRIORITY;
        virtualCameras[nextCameraIndex].Priority = ACTIVE_CAMERA_PRIORITY;
        currentCameraIndex = nextCameraIndex;
    }

    public void SwitchCamera(CinemachineVirtualCamera nextCamera)
    {
        currentCamera.Priority = INACTIVE_CAMERA_PRIORITY;
        currentCamera = nextCamera;
        nextCamera.Priority = ACTIVE_CAMERA_PRIORITY;
    }

    public void SwitchCamera(CinemachineVirtualCamera previousCamera, CinemachineVirtualCamera nextCamera)
    {
        previousCamera.Priority = INACTIVE_CAMERA_PRIORITY;
        nextCamera.Priority = ACTIVE_CAMERA_PRIORITY;
    }
}
