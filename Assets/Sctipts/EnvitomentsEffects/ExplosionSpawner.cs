using UnityEngine;

public class ExplosionSpawner : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private ExplosionPoolMgr explosionPoolMgr;

    public void MakeExplosion(Vector3 position)
    {
        var explosion = GetComponent<IPoolHandler>().GetEnemy();
        explosion.gameObject.MyGetComponent_NullChker<ExplosionBody>().MyAwake(this,position);
    }

    public void ReturnExplosion(GameObject explosion)
    {
        explosionPoolMgr.ReturnEnemy(explosion);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Explosion");
            MakeExplosion(playerTransform.position);
        }
    }
}
