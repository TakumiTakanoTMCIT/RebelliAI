using UnityEngine;
using System.Collections;
using Zenject;

/// <summary>
/// プレイヤーがダッシュを始めたら何秒間ダッシュを続けるかを制御するクラスです
/// </summary>
public class PlayerDashTimeCtrl : MonoBehaviour
{
    [Inject]
    PlayerStats playerStatus;

    [SerializeField]
    private bool isDashNow = false;
    public bool IsDashNow => isDashNow;

    /// <summary>
    /// プレイヤーがダッシュを始めたら何秒間ダッシュを続けるかを制御するコルーチンです
    /// ダッシュを続ける時間はすべてplayerStatus.DashTimeで管理します
    /// </summary>
    Coroutine dashTimeCtrlCoroutine;
    IEnumerator dashTimeCtrler()
    {
        isDashNow = true;
        yield return new WaitForSeconds(playerStatus.DashTime);
        isDashNow = false;
    }

    /// <summary>
    /// ダッシュ時間制御コルーチンを開始する関数です
    /// </summary>
    public void StartDashTimeCtrl()
    {
        if(dashTimeCtrlCoroutine != null)
        {
            StopCoroutine(dashTimeCtrlCoroutine);
            dashTimeCtrlCoroutine = null;
        }
        dashTimeCtrlCoroutine = StartCoroutine(dashTimeCtrler());
    }

    /// <summary>
    /// ダッシュ時間制御コルーチンを停止する関数です
    /// </summary>
    public void StopDashTimeCtrl()
    {
        if(dashTimeCtrlCoroutine != null)
        {
            StopCoroutine(dashTimeCtrlCoroutine);
            dashTimeCtrlCoroutine = null;
        }
        isDashNow = false;
    }
}
