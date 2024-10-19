using System.Collections;
using PlayerInfo;
using UnityEngine;

public class DamageTimeHandler : MonoBehaviour
{
    [SerializeField] PlayerStatus playerStatus;

    private bool isDamaging;
    public bool IsDamaging { get { return isDamaging; } }

    Coroutine damageTimeCoroutine;
    IEnumerator DamageTime()
    {
        isDamaging = true;
        yield return new WaitForSeconds(playerStatus.damagingTime);
        isDamaging = false;
    }


    //外部から呼び出す関数です
    public void StartDamageTime()
    {
        if (damageTimeCoroutine != null)
            StopCoroutine(damageTimeCoroutine);

        damageTimeCoroutine = StartCoroutine(DamageTime());
    }

    public void StopDamageTime()
    {
        if (damageTimeCoroutine != null)
            StopCoroutine(damageTimeCoroutine);

        isDamaging = false;
        damageTimeCoroutine = null;
    }
}
