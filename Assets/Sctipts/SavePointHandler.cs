using System;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] SavePointSaver pointSaver;

    //セーブポイントの位置をGizmosで表示
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
        if (playerTransform == null)
        {
            Debug.Log($"プレイヤーのTransformが設定されていません。");
        }

        //セーブポイントをリセットすべきならリセットします
        if (MyGameManager.resetSaveData)
        {
            pointSaver.InitSavePoint();
            MyGameManager.resetSaveData = false;
        }
    }

    private void Update()
    {
        //今のセーブが最後のセーブポイントなら処理をしない
        if (pointSaver.NextSavePoint > savePointBase.Count - 1) return;

        //プレイヤーのポジションが次のセーブポイントを超えたらセーブ
        if (playerTransform.position.x > savePointBase[pointSaver.NextSavePoint].savePosition.x)
        {
            Debug.Log($"セーブポイント{pointSaver.NextSavePoint}に到達しました。次のステートは{pointSaver.NextSavePoint + 1}です。");

            //セーブポイントを更新
            pointSaver.SaveNextPoint();
        }
    }

    //現在のセーブポイントがボス部屋ならtrueを返す
    public bool SetPlayerOnStagePositionSavePoint()
    {
        playerTransform.position = savePointBase[pointSaver.CurrentSavePoint].savePosition;
        return savePointBase[pointSaver.CurrentSavePoint].isBossSaveRoom;
    }
}
