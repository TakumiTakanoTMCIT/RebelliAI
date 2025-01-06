using UnityEngine;
using Zenject;
using KeyHandler;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// このクラスは、壁蹴りの受付時間を管理するクラスです。
/// 壁蹴りの受付時間中にジャンプキーが押されたら、壁蹴り状態に遷移します。
/// </summary>
public class WallKickDelayManager : MonoBehaviour
{
    [Inject]
    PlayerStats playerStatus;

    InputHandler inputHandler;

    public Action OnWallKickRequest;

    private bool isJumpKey_Accepting = false;

    private CancellationTokenSource cts;

    private void Awake()
    {
        inputHandler = this.gameObject.MyGetComponent_NullChker<InputHandler>();
    }

    /// <summary>
    /// ジャンプキーを一瞬だけ受け付けるフラグの管理をするメソッドです。
    /// </summary>
    private async UniTask JumpKey_Accepting(CancellationTokenSource cts)
    {
        isJumpKey_Accepting = true;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(playerStatus.delayKey_reception_time), cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            //キャンセルされたら何もしない
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            isJumpKey_Accepting = false;
        }
    }

    public void Start_JumpKey_AcceptingTime()
    {
        if (isJumpKey_Accepting)
        {
            Stop_JumpKey_AcceptingTime();
        }
        cts = new CancellationTokenSource();
        JumpKey_Accepting(cts).Forget();
    }

    public void Stop_JumpKey_AcceptingTime()
    {
        if (isJumpKey_Accepting)
        {
            cts.Cancel();
        }
    }

    private void Update()
    {
        //受付時間以外なら受け付けない
        if (!isJumpKey_Accepting) return;

        //ジャンプキーが押されたら
        if (inputHandler.IsJumpKeyDown())
        {
            //コルーチンの受け付け時間を終了する
            Stop_JumpKey_AcceptingTime();

            //wallkickに遷移する
            OnWallKickRequest?.Invoke();
        }
    }
}
