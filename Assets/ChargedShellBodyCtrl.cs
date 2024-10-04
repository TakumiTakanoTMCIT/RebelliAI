using ActionStatusChk;
using PlayerInfo;
using UnityEngine;

public class ChargedShellBodyCtrl : MonoBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [SerializeField] PlayerStatus playerStatus;
    [SerializeField] GameObject player;

    bool direction;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    public void Init(bool direction)
    {
        GameObject player = GameObject.Find("Player");

        playerStatus = player.GetComponent<PlayerStatus>();

        MoveShell();
    }

    private void MoveShell()
    {
        rb = this.GetComponent<Rigidbody2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();

        if (playerStatus.playerDirection) rb.velocity = new Vector2(speed, 0);
        else rb.velocity = new Vector2(-speed, 0);
    }

    private void Update()
    {
        if(!spriteRenderer.isVisible) Destroy(this.gameObject);
    }
}
