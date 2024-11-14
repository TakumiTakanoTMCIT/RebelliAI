using UnityEngine;

public class ExplosionSpawner : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private ExplosionPoolMgr explosionPoolMgr;
    IPoolHandler poolHandler;

    private void Awake()
    {
        poolHandler = GetComponent<IPoolHandler>();
    }

    public void MakeExplosion(Vector3 position)
    {
        var explosion = poolHandler.GetObject();
        explosion.gameObject.MyGetComponent_NullChker<ExplosionBody>().MyAwake(this, position);
    }

    public void ReturnExplosion(GameObject explosion)
    {
        explosionPoolMgr.ReturnObjct(explosion);
    }
}
