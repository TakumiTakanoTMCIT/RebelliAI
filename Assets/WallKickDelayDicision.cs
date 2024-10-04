using System.Collections;
using PlayerState;
using UnityEngine;

/// <summary>
/// 壁キックの遅延判定を行うクラスです。
/// 壁から少し離れたとしても壁キックをできるようにするためのクラスです。
/// コルーチンで遅延判定を行い、フラグで判定を返します。
/// </summary>
public class WallKickDelayDicision : MonoBehaviour
{
    private Coroutine wallKickDelayCoroutine = null;

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

    /// <summary>
    /// コルーチンのオンオフを切り替える関数です。
    /// </summary>
    public void StartWallKickDelayDicision()
    {
        if (wallKickDelayCoroutine != null)
        {
            StopCoroutine(wallKickDelayCoroutine);
            wallKickDelayCoroutine = null;
        }
        wallKickDelayCoroutine = StartCoroutine(wallKickDelayDicision());
    }

    public void StopWallKickDelayDicision()
    {
        if (wallKickDelayCoroutine != null)
        {
            StopCoroutine(wallKickDelayCoroutine);
            wallKickDelayCoroutine = null;
        }
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

    /// <summary>
    /// 壁から離れているときにでも壁キックを行えるような実装を書いています
    /// 落ちたとしても時間以内だったら壁キックを行う処理を書く
    /// wallfallからfallに遷移する時に時間以内にジャンプしたら多少キーの押し方に問題があっても壁キックを行えるようにするための処理です
    /// </summary>
    private void Update()
    {
        if (isWallKickDelayDicision)
        {
            if (stateMgr.inputHandler.IsJumpKeyDown())
            {
                ResetWallKickDelay();

                stateMgr.ChangeState(stateMgr.wallKick);
                return;
            }
        }
    }
}
