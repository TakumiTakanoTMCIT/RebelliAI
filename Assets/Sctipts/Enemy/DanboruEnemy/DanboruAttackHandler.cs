using UnityEngine;

public class DanboruAttackHandler : MonoBehaviour
{
    [SerializeField] private GameObject prefabMissile;

    DanboruGroundChk groundChk;
    DanboruAnimStateMgr animStateMgr;
    DanboruActMgr actMgr;
    bool isShotedMissile;

    private void Awake()
    {
        isShotedMissile = false;
        animStateMgr = gameObject.MyGetComponent_NullChker<DanboruAnimStateMgr>();
        actMgr = gameObject.MyGetComponent_NullChker<DanboruActMgr>();
        groundChk = transform.GetChild(0).gameObject.MyGetComponent_NullChker<DanboruGroundChk>();
    }

    private void Update()
    {
        if (animStateMgr.currentState == animStateMgr.ReleaseState)
        {
            if (groundChk.IsGround()) return;
            if (!actMgr.IsFalling() && !actMgr.IsJumping())
            {
                if (isShotedMissile) return;
                ShotMissile();
            }
        }
        else
        {
            if (isShotedMissile) isShotedMissile = false;
        }
    }

    private void ShotMissile()
    {
        var missile = Instantiate(prefabMissile, transform.position, Quaternion.identity);
        Debug.Log("ミサイル発射");
        isShotedMissile = true;
    }
}
