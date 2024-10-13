using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void Start()
    {
        transform.position = new Vector3(4, player.transform.position.y, -10);
        /*Vector3 pos = transform.position;
        pos.x = player.transform.position.x;
        transform.position = pos;*/
    }

    private void Update()
    {
        if (player.transform.position.x > 4)
        {
            Vector3 pos = transform.position;
            pos.x = player.transform.position.x;
            transform.position = pos;
        }

        if (player.transform.position.x > 27f)
        {
            if (player.transform.position.y > -2.5f)
            {
                SetYPos();
            }
        }
        else if (player.transform.position.x > 10)
        {
            if (player.transform.position.y > 0f)
            {
                SetYPos();
            }
        }
        else
        {
            if (player.transform.position.y > 1f)
            {
                SetYPos();
            }
        }
    }

    void SetYPos()
    {
        Vector3 pos = transform.position;
        pos.y = player.transform.position.y;
        transform.position = pos;
    }
}
