using UnityEngine;
using IntroBossExperimenter;
using System;

public class IntroBossHealthCtrl : MonoBehaviour, IDamageableFromShot
{
    public static event Action<int> onDamage;

    [SerializeField] BoxCollider2D boxCollider2D;

    float saveOffsetX;

    private void Awake()
    {
        IsAlivingNow = true;
        saveOffsetX = -boxCollider2D.offset.x;
    }

    private void OnEnable()
    {
        IntroBossExperimentAnimCtrl.onChangeDirection += ChangeDirection;
        IntroBossHPBarHandler.onDead += OnDead;
    }

    private void OnDisable()
    {
        IntroBossExperimentAnimCtrl.onChangeDirection -= ChangeDirection;
        IntroBossHPBarHandler.onDead -= OnDead;
    }

    void OnDead()
    {
        IsAlivingNow = false;
    }

    void ChangeDirection(bool direction)
    {
        if (direction)
            boxCollider2D.offset = new Vector2(saveOffsetX, boxCollider2D.offset.y);
        else
            boxCollider2D.offset = new Vector2(-saveOffsetX, boxCollider2D.offset.y);
    }

    public bool IsAlivingNow { get; set; }
    public void TakeDamage(int damage)
    {
        if(!IsAlivingNow) return;

        onDamage?.Invoke(damage);
        Debug.Log("TakeDamage");
    }
}
