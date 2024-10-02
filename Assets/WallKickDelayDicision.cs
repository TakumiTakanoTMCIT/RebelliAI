using System.Collections;
using PlayerState;
using UnityEngine;

public class WallKickDelayDicision : MonoBehaviour
{
    [SerializeField] float wallKickDelayTime = 0.1f;

    PlayerStateMgr stateMgr;

    [SerializeField] private bool isWallKickDelayDicision = false;
    public bool IsWallKickDelayDicision
    {
        get { return isWallKickDelayDicision; }
        private set { isWallKickDelayDicision = value; }
    }

    private void Start()
    {
        stateMgr = this.GetComponent<PlayerStateMgr>();
    }

    public IEnumerator wallKickDelayDicision()
    {
        isWallKickDelayDicision = true;
        yield return new WaitForSeconds(wallKickDelayTime);
        isWallKickDelayDicision = false;
    }

    public void ResetWallKickDelay()
    {
        isWallKickDelayDicision = false;
    }
}
