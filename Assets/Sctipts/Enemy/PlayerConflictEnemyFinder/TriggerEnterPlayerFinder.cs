using UnityEngine;

public class TriggerEnterPlayerFinder : MonoBehaviour
{
    MissileBody missileBody;
    int damage;
    public void Init(int damage, MissileBody missileBody)
    {
        Debug.LogAssertion("Missile INIT");
        this.damage = damage;
        this.missileBody = missileBody;
    }

    private void OnTriggerEnter2D(Collider2D other, MissileBody missileBody)
    {
        if (!missileBody.IsAlivingNow) return;

        var conflictEnemy = other.gameObject.GetComponent<IConflictEnemy>();
        if (conflictEnemy == null) return;
        Destroy(gameObject);
        Debug.Log("Missile Hit");
        conflictEnemy.OnConflictEnemy(damage);
    }
}
