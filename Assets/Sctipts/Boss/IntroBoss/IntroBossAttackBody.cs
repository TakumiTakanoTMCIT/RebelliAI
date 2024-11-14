using IntroBossExperimenter;
using UnityEngine;

namespace IntroBossExperimenter
{
    public interface IDealDamage
    {
        void DealDamage(int damage, IConflictEnemy conflictEnemy);
    }

    public class IntroBossAttackBody : MonoBehaviour, IDealDamage
    {
        [SerializeField] float minSpeed = 5.0f, maxSpeed = 10.0f;
        [SerializeField] int damage = 2;

        float saveOffset;

        AttackObjectPoolHandler PoolHandler;

        BoxCollider2D boxCol2D;
        Rigidbody2D rb;
        SpriteRenderer sr;
        private void Awake()
        {
            rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
            sr = gameObject.MyGetComponent_NullChker<SpriteRenderer>();
            boxCol2D = gameObject.MyGetComponent_NullChker<BoxCollider2D>();

            saveOffset = boxCol2D.offset.x;
        }

        public void Init(AttackObjectPoolHandler poolHandler, Transform parentTransform)
        {
            PoolHandler = poolHandler;
            transform.SetParent(parentTransform);
        }

        public void StartMove(bool direction, Transform introBossTransform)
        {
            transform.position = introBossTransform.position;

            sr.flipX = direction;

            float randSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);

            if (direction)
            {
                rb.velocity = new Vector2(randSpeed, rb.velocity.y);

                boxCol2D.offset = new Vector2(-saveOffset, boxCol2D.offset.y);
            }
            else
            {
                rb.velocity = new Vector2(-randSpeed, rb.velocity.y);
                boxCol2D.offset = new Vector2(saveOffset, boxCol2D.offset.y);
            }
        }

        private void OnBecameInvisible()
        {
            PoolHandler.pool.Release(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IConflictEnemy dealDamage = other.gameObject.GetComponent<IConflictEnemy>();
            if (dealDamage == null) return;
            DealDamage(damage, dealDamage);
        }

        //インターフェース実装
        public void DealDamage(int damage, IConflictEnemy conflictEnemy)
        {
            conflictEnemy.OnConflictEnemy(damage);
        }
    }
}
