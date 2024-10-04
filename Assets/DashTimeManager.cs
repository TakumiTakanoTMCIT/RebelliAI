using UnityEngine;
using System.Collections;
using System;
using PlayerInfo;

/// <summary>
/// プレイヤーのダッシュの時間を管理するクラスです。
/// ダッシュの時間制限はこのクラスで管理します
/// </summary>
public class DashTimeManager : MonoBehaviour
{
    PlayerStatus status;

    private bool isDashing = false;
    public bool IsDashing
    {
        get { return isDashing; }
        private set { isDashing = value; }
    }

    Coroutine dashTimeCoroutine = null;

    private void Awake()
    {
        /// <summary>
        /// 初期化
        /// </summary>
        IsDashing = false;
        status = this.GetComponent<PlayerStatus>();
    }

    public void StartDashTimeCounter()
    {
        if (dashTimeCoroutine != null)
        {
            StopCoroutine(dashTimeCoroutine);
            dashTimeCoroutine = null;
        }
        dashTimeCoroutine = StartCoroutine(DashTimeCounter());
    }

    public void StopDashTimeCounter()
    {
        if (dashTimeCoroutine != null)
        {
            StopCoroutine(dashTimeCoroutine);
            dashTimeCoroutine = null;
        }
    }

    private IEnumerator DashTimeCounter()
    {
        IsDashing = true;
        yield return new WaitForSeconds(status.DashTime);
        IsDashing = false;
    }
}
