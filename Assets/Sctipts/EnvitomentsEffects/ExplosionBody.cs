using UnityEngine;

public class ExplosionBody : MonoBehaviour
{
    ExplosionSpawner spawner;
    public void MyAwake(ExplosionSpawner spwaner,Vector3 pos)
    {
        transform.position = pos;
        this.spawner = spwaner;
    }
    /// <summary>
    /// アニメーションイベントです。Unity側からAnimationで登録していて呼び出されます。
    /// </summary>
    public void EndAnim()
    {
        spawner.ReturnExplosion(gameObject);
    }
}
