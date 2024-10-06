using UnityEngine;

public class DamageAbleFinder : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageableFromShot damageable = other.GetComponent<IDamageableFromShot>();
        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount);
            this.gameObject.GetComponent<IDestroyable>().DestroyShell();
        }
    }
}
