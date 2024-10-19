using UnityEngine;

public class ConflictPlayerFinder : MonoBehaviour
{
    int damage;
    public void Init(int damage)
    {
        this.damage = damage;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var conflictEnemy = other.gameObject.GetComponent<IConflictEnemy>();
        if (conflictEnemy == null) return;
        conflictEnemy.OnConflictEnemy(damage);
    }
}
