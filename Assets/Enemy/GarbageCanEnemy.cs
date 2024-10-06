using UnityEngine;

public interface IDamageableFromShot
{
    void TakeDamage(int damage);
}

public interface IWalkable
{
    void Walk();
}

public class GarbageCanEnemy : MonoBehaviour, IDamageableFromShot
{
    [SerializeField] private int hp = 3;

    public void TakeDamage(int damage)
    {
        Debug.Log("Enemy took " + damage + " damage");
        hp -= damage;

        if (hp <= 0)
        {
            Destroy(gameObject);
            Debug.Log("Enemy destroyed");
        }
    }
}
