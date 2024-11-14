using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public static class SavePoint
{
    [SerializeField] public static int currentSavePoint = 0, nextSavePoint = 1;
}

[Serializable]
public class SavePointBase
{
    public Vector2 savePosition;
    public bool isBossSaveRoom = false;
}

public class SavePointHandler : MonoBehaviour
{
    [SerializeField] List<SavePointBase> savePointBase;
    [SerializeField] Transform playerTransform;

    private void OnDrawGizmos()
    {
        foreach (var point in savePointBase)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(point.savePosition, 0.5f);
        }
    }

    private void Awake()
    {
        //InitSavePoint();
        if (playerTransform == null)
        {
            Debug.Log($"プレイヤーのTransformが設定されていません。");
        }

        if(MyGameManager.resetSaveData)
        {
            InitSavePoint();
            MyGameManager.resetSaveData = false;
        }
    }

    //新しくステージに入った時に呼び出す
    public void InitSavePoint()
    {
        SavePoint.currentSavePoint = 0;
        SavePoint.nextSavePoint = 1;
    }

    private void Update()
    {
        //Debug.Log($"savePointBase[SavePoint.{SavePoint.nextSavePoint}].savePosition.x : {savePointBase[SavePoint.nextSavePoint].savePosition.x}");
        //プレイヤーのポジションが次のセーブポイントを超えたらセーブ
        if (SavePoint.nextSavePoint > savePointBase.Count - 1) return;
        //if (savePointBase[SavePoint.nextSavePoint].isBossSaveRoom) return;

        if (playerTransform.position.x > savePointBase[SavePoint.nextSavePoint].savePosition.x)
        {
            /*if (savePointBase[SavePoint.nextSavePoint].isBossSaveRoom)
            {
                Debug.Log("ボス部屋に到達しました。");
                return;
            }*/
            Debug.Log($"セーブポイント{SavePoint.nextSavePoint}に到達しました。次のステートは{SavePoint.nextSavePoint + 1}です。");
            SavePointUpdate();
        }
    }

    void SavePointUpdate()
    {
        SavePoint.currentSavePoint++;
        SavePoint.nextSavePoint++;
    }

    //現在のセーブポイントがボス部屋ならtrueを返す
    public bool SetPlayerOnStagePositionSavePoint()
    {
        playerTransform.position = savePointBase[SavePoint.currentSavePoint].savePosition;
        return savePointBase[SavePoint.currentSavePoint].isBossSaveRoom;
    }
}
