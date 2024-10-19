using PlayerAction;
using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    bool isFreezeCam = false;

    private void Awake()
    {
        isFreezeCam = false;
        ActionHandler.onPlayerDeath += OnPlayerDeath;
    }

    private void Start()
    {
        transform.position = new Vector3(4, playerTransform.position.y, -10);
    }

    void OnPlayerDeath()
    {
        Debug.Log("どうもカメラです！プレイヤーが死んだらしいぜ！");
        isFreezeCam = true;
    }

    private void Update()
    {
        if(isFreezeCam) return;

        if (playerTransform.position.x > 4)
        {
            Vector3 pos = transform.position;
            pos.x = playerTransform.position.x;
            transform.position = pos;
        }

        if (playerTransform.position.x > 27f)
        {
            if (playerTransform.position.y > -2.5f)
            {
                SetYPos();
            }
        }
        else if (playerTransform.position.x > 10)
        {
            if (playerTransform.position.y > 0f)
            {
                SetYPos();
            }
        }
        else
        {
            if (playerTransform.position.y > 1f)
            {
                SetYPos();
            }
        }
    }

    void SetYPos()
    {
        Vector3 pos = transform.position;
        pos.y = playerTransform.position.y;
        transform.position = pos;
    }
}
