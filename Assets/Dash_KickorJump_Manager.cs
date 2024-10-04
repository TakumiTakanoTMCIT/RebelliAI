using System.Collections;
using PlayerInfo;
using UnityEngine;

/// <summary>
/// このクラスは、プレイヤーのダッシュキックとダッシュジャンプに関するクラスです。
/// コルーチンを開始して何秒以内にキーを押すと移動スピードをDashSpeedに変更します。
/// 基本的に地面についたらDashSpeedを元に戻します。
/// </summary>

//TODO:ダッシュの時間の管理をするクラスを間違えて消してしまったからそれを復活させる。そしてこのクラスでダッシュの管理を行う。←これ重要！！！！！！！

public class Dash_KickorJump_Manager : MonoBehaviour
{
    PlayerStatus playerStatus;

    [SerializeField]
    private bool is_Accepting_DashKey_Now = false;

    [SerializeField]
    private bool is_Keeping_DashSpeed_Now = false;

    /// <summary>
    /// ダッシュキーの受付を行うコルーチン
    /// </summary>
    private Coroutine accepting_DashKey_Coroutine = null;

    private void Start()
    {
        is_Accepting_DashKey_Now = false;
        playerStatus = this.GetComponent<PlayerStatus>();
    }

    /// <summary>
    /// ダッシュキーの受付を開始したい時に呼び出します。
    /// </summary>
    public void StartAcceptingDashKey()
    {
        /// <summary>
        /// 初期化したあとにコルーチンを開始します。
        /// </summary>
        if (accepting_DashKey_Coroutine != null)
        {
            StopCoroutine(accepting_DashKey_Coroutine);
            accepting_DashKey_Coroutine = null;
        }
        accepting_DashKey_Coroutine = StartCoroutine(DashTimeCounter());
    }

    /// <summary>
    /// ダッシュキーの受付を中断したいときに呼び出します。
    /// </summary>
    public void Interruput_AcceptingDashKey()
    {
        if (accepting_DashKey_Coroutine != null)
        {
            StopCoroutine(accepting_DashKey_Coroutine);
            accepting_DashKey_Coroutine = null;
        }

        /// <summary>
        /// すべての処理が終わったら、受付中のフラグをfalseにします。
        /// </summary>
        is_Accepting_DashKey_Now = false;
    }

    /// <summary>
    /// ダッシュキーの受付中かどうかを返す関数です。
    /// </summary>
    public bool IsAccepting_DashKey_Now()
    {
        return is_Accepting_DashKey_Now;
    }

    /// <summary>
    /// ダッシュスピードの保持をスタートする関数です。
    /// </summary>
    public void StartKeepDash()
    {
        is_Keeping_DashSpeed_Now = true;

        /// <summary>
        /// なぜここで中断するかというと、コルーチンが実行されている時に、また実行されるとおかしくなりそうだと思ったからです。
        /// 実際こんなパターンがあるかはわかりませんが、一応。
        /// </summary>
        Interruput_AcceptingDashKey();
    }

    /// <summary>
    /// ダッシュスピードの保持を終了する関数です。
    /// </summary>
    public void StopKeepDash()
    {
        is_Keeping_DashSpeed_Now = false;
    }

    /// <summary>
    /// 今ダッシュスピードを保持しているかどうかを返す関数です。
    /// </summary>
    public bool IsKeepingDashSpeed_Now()
    {
        return is_Keeping_DashSpeed_Now;
    }

    /// <summary>
    /// コルーチン本体です。
    /// </summary>
    private IEnumerator DashTimeCounter()
    {
        is_Accepting_DashKey_Now = true;
        yield return new WaitForSeconds(playerStatus.DashTime);
        is_Accepting_DashKey_Now = false;
    }
}
