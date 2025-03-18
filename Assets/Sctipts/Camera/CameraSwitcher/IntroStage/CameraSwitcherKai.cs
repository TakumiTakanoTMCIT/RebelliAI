using System;
using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

[Serializable]
class CameraRange
{
    public Vector2 startPosition, endPosition;
    public CinemachineVirtualCamera camera, outCamera;
    public bool isCheckEndZoneYPosition = false;
}

public class CameraSwitcherKai : MonoBehaviour
{
    [SerializeField] private List<CameraRange> cameraRanges;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CameraSwitcher cameraSwitcher;

    private CameraRange currentCameraRange;

    [SerializeField] bool isInRange = false;

    private void Start()
    {
        isInRange = false;
        if(cameraRanges[0] != null)
        {
            currentCameraRange = cameraRanges[0];
        }
    }

    private void Update()
    {
        ///　<summary>
        /// 切り替え範囲にいるなら切り替え範囲から出るまで待機し、出たらoutCameraに切り替える
        /// </summary>
        if (isInRange)
        {
            //切り替え範囲から出たかどうか
            if (playerTransform.position.x < currentCameraRange.startPosition.x || currentCameraRange.endPosition.x < playerTransform.position.x)
            {
                /// <summary>
                /// 切り替え範囲から出たらoutCameraに切り替え、isInRangeをfalseにして終了
                /// </summary>
                cameraSwitcher.SwitchCamera(currentCameraRange.camera, currentCameraRange.outCamera);
                isInRange = false;
                currentCameraRange = null;
                return;
            }
            //出ていないならreturnして素早く終了
            else return;
        }

        /// <summary>
        /// プレイヤーがcameraRangesのどこかの切り替え範囲内にいるかどうかをforeachで回して検索する
        /// </summary>
        foreach (var camera in cameraRanges)
        {
            /// <summary>
            /// 切り替え範囲に入っているならforeachで回して検索する必要はないので、return!!
            /// たぶんこれ下のif文でreturnしてるから意味ないかな...?
            /// </summary>
            if (isInRange) return;

            //プレイヤーが"camera"の切り替え範囲内にいるかどうか
            //cameraの範囲内なので、すべての範囲内を見ているわけではない。
            //cameraRangesのうちの、一つの範囲内にいるかどうかを見ています！！
            if (camera.startPosition.x < playerTransform.position.x && playerTransform.position.x < camera.endPosition.x)
            {
                /// <summary>
                /// EndPosのY座標をチェックするならば、プレイヤーのY座標がEndPosのY座標よりも大きいかどうかもチェックする
                /// </summary>
                if (camera.isCheckEndZoneYPosition)
                    if (playerTransform.position.y < camera.endPosition.y)
                        continue;
                /// <summary>
                /// もしプレイヤーがcameraRangesのどこかの範囲にいるならカメラを設定して、return!!
                /// </summary>
                isInRange = true;
                currentCameraRange = camera;
                Debug.Log($"このカメラに切り替えます！: {currentCameraRange.camera}");
                cameraSwitcher.SwitchCamera(currentCameraRange.outCamera, currentCameraRange.camera);
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var cameraCount in cameraRanges)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(cameraCount.startPosition, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(cameraCount.endPosition, 0.5f);
        }
    }
}
