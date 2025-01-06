using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Game/PlayerStats", order = 1)]
public class PlayerStats : ScriptableObject
{
    [SerializeField]
    private float moveSpeed = 5f;
    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    [SerializeField]
    private float jumpForce = 15f;
    public float JumpForce
    {
        get { return jumpForce; }
    }

    public float FallSpeedLevel = -0.5f;

    public float JumpForceLevel = 0.5f;

    public float WallFallSpeed = 2.8f;

    public float DashSpeed = 11f;
    public float DashTime = 0.365f;

    public float delayKey_reception_time = 0.15f;

    private bool PlayerDirection = true;
    public bool playerdirection
    {
        get { return PlayerDirection; }
    }

    public float damagingTime = 3f;

    public Vector2 damageForce = new Vector2(5, 5);

    public float blikingInterval = 0.03f, invincibleTime = 1.8f;

    public float DefaultGravity => defaultGravity;
    private float defaultGravity = 3.2f;
}
