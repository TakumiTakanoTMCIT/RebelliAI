using UnityEngine;

public class DanboruGroundChk : MonoBehaviour
{
    [SerializeField] private bool isGround;

    DanboruAnimStateMgr danboruAnimStateMgr;
    private void Awake()
    {
        danboruAnimStateMgr = gameObject.transform.parent.gameObject.MyGetComponent_NullChker<DanboruAnimStateMgr>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGround = false;
        }
    }

    public bool IsGround()
    {
        return isGround;
    }
}
