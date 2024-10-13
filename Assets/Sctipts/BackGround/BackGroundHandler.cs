using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using PlayerInfo;
using UnityEngine;

public class BackGroundHandler : MonoBehaviour
{
    [SerializeField] float speed = 0.01f;
    [SerializeField] private GameObject player;
    PlayerStatus playerStatus;
    ActionStatusChecker actionStatusChecker;
    Vector3 pos;
    private float previousPlayerPosX;  // プレイヤーの前フレームのX位置

    void Start()
    {
        player.gameObject.MyGetComponent_NullChker<Transform>();
        playerStatus = player.gameObject.MyGetComponent_NullChker<PlayerStatus>();
        actionStatusChecker = player.gameObject.MyGetComponent_NullChker<ActionStatusChecker>();

        previousPlayerPosX = 4;

        //現在地を4にする
        Vector3 pos = transform.position;
        pos.x = 4;
        transform.position = pos;
    }

    void Update()
    {
        if (player.transform.position.x > 4)
        {
            if (!actionStatusChecker.IsMovingNow()) return;

            float deltaX = player.transform.position.x - previousPlayerPosX;

            //現在地を取得
            pos = transform.position;

            //X座標にプレイヤーの座標の差分をspeed倍して加算
            pos.x += deltaX * speed;
            transform.position = pos;

            previousPlayerPosX = player.transform.position.x;

            /*if (playerStatus.playerdirection)
                pos.x -= speed;
            else
                pos.x += speed;
            transform.position = pos;*/
        }
    }
}
