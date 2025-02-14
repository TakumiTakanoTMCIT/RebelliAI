using System.Collections.Generic;
using UnityEngine;
using HPBar;
using UniRx;
using Zenject;

public class CamCtrl : MonoBehaviour
{
    //Inject
    LifeManager lifeManager;

    [SerializeField] List<CamFollowZone> camFollowZones;

    Vector3 pos, targetPos;

    [SerializeField] private Transform playerTransform;
    bool isFreezeCam = false;
    [SerializeField] bool isDebugMode = false;

    [Inject]
    public void Construct(LifeManager lifeManager)
    {
        this.lifeManager = lifeManager;
    }

    private void Awake()
    {
        isFreezeCam = false;

        lifeManager.OnPlayerDead.Subscribe(_ =>
        {
            OnPlayerDeath();
        }).AddTo(this);
    }

    private void Start()
    {
        transform.position = new Vector3(4, playerTransform.position.y, -10);
    }

    void OnPlayerDeath()
    {
        isFreezeCam = true;
    }

    private void Update()
    {
        if (isFreezeCam) return;

        if (isDebugMode)
        {
            transform.position = playerTransform.position;
            return;
        }

        int count = 0;
        foreach (var zone in camFollowZones)
        {
            //ゾーンに対応したX座標にプレイヤーがいた場合にカメラを追従させる
            if (playerTransform.position.x > zone.minX && playerTransform.position.x < zone.maxX)
            {
                Debug.Log("In Zone " + count);
                pos = transform.position;
                pos.x = playerTransform.position.x;
                pos.z = -10;

                /*if (count == 3)
                {
                    pos.x = 57.5f;
                }*/

                if (transform.position.y > zone.camYMin && transform.position.y < zone.camYMax)
                {
                    pos.y = playerTransform.position.y;
                    transform.position = pos;
                    return;
                }
                transform.position = pos;
            }
            count++;
        }

        //上の範囲にいなかったら制限すべき範囲にいないから普通に追順させる！
        pos = transform.position;
        pos.x = playerTransform.position.x;
        pos.y = playerTransform.position.y;
        transform.position = pos;
    }
}

[System.Serializable]
public class CamFollowZone
{
    public float minX, maxX, camYMin, camYMax;
}
