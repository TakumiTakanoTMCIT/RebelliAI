using ActionStatusChk;
using PlayerInfo;
using UnityEngine;

public interface IDestroyable
{
    void DestroyShell();
}

public class ChargedShellBodyCtrl : MonoBehaviour, IDestroyable
{
    [SerializeField] private float speed = 5.0f;

    PlayerStatus playerStatus;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        GameObject player = GameObject.Find("Player");
        playerStatus = player.GetComponent<PlayerStatus>();

        MoveShell();
    }

    private void MoveShell()
    {
        rb = this.GetComponent<Rigidbody2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();

        if (playerStatus.playerdirection) rb.velocity = new Vector2(speed, 0);
        else rb.velocity = new Vector2(-speed, 0);
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            MoveShell();
        }

        if (!spriteRenderer.isVisible) DestroyShell();
    }

    public void DestroyShell()
    {
        Destroy(this.gameObject);
    }
}
