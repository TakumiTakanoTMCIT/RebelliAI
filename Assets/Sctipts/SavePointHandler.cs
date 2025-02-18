using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

[Serializable]
public class SavePointBase
{
    public Vector2 respawnPosition;
    public bool isBossSaveRoom = false;
}

public class SavePointHandler : MonoBehaviour
{
    [SerializeField] List<SavePointBase> savePointBase;
    [SerializeField] Transform playerTransform;
    [SerializeField] SavePointSaver pointSaver;

    //Inject
    private EventStreamer eventStreamer;

    [Inject]
    public void Construct(EventStreamer eventStreamer)
    {
        this.eventStreamer = eventStreamer;
    }

    //セーブポイントの位置をGizmosで表示
    private void OnDrawGizmos()
    {
        foreach (var point in savePointBase)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(point.respawnPosition, 0.5f);
        }
    }

    private void Awake()
    {
        if (playerTransform == null)
        {
            Debug.Log($"プレイヤーのTransformが設定されていません。");
        }

        eventStreamer.startBossDoorCutScene
            .Subscribe(_ =>
            {
                pointSaver.SaveNextPoint();
            })
            .AddTo(this);

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
        if (playerTransform.position.x > savePointBase[pointSaver.NextSavePoint].respawnPosition.x)
        {
            Debug.Log($"セーブポイント{pointSaver.NextSavePoint}に到達しました。次のステートは{pointSaver.NextSavePoint + 1}です。");

            //セーブポイントを更新
            pointSaver.SaveNextPoint();
        }
    }

    //現在のセーブポイントがボス部屋ならtrueを返す
    public bool SetPlayerOnStagePositionSavePoint()
    {
        playerTransform.position = savePointBase[pointSaver.CurrentSavePoint].respawnPosition;
        return savePointBase[pointSaver.CurrentSavePoint].isBossSaveRoom;
    }
}
