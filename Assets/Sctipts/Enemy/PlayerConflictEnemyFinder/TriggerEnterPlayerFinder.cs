using UnityEngine;

public class TriggerEnterPlayerFinder : MonoBehaviour
{
    MissileBody missileBody;
    int damage;
    public void Init(int damage, MissileBody missileBody)
    {
        this.damage = damage;
        this.missileBody = missileBody;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!missileBody.IsAlivingNow) return;
        var conflictEnemy = other.gameObject.GetComponent<IConflictEnemy>();
        if (conflictEnemy == null) return;
        Destroy(gameObject);
        conflictEnemy.OnConflictEnemy(damage);
    }
}
