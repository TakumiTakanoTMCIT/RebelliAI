using UnityEngine;
using Zenject;
using KeyHandler;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;

/// <summary>
/// このクラスは、壁蹴りの受付時間を管理するクラスです。
/// 壁蹴りの受付時間中にジャンプキーが押されたら、壁蹴り状態に遷移します。
/// </summary>
public class WallKickDelayManager : MonoBehaviour
{
    [Inject]
    PlayerStats playerStats;

    private IReactiveProperty<bool> isJumpKeyReactiveProperty = new ReactiveProperty<bool>();

    private CancellationTokenSource cts;

    //Inject
    private PlayerState.EventMediator eventMediator;
    private InputHandler inputHandler;

    [Inject]
    public void Construct(PlayerState.EventMediator eventMediator, InputHandler inputHandler)
    {
        this.eventMediator = eventMediator;
        this.inputHandler = inputHandler;
    }

    private void Awake()
    {
        isJumpKeyReactiveProperty.Value = false;
    }

    /// <summary>
    /// ジャンプキーを一瞬だけ受け付けるフラグの管理をするメソッドです。
    /// </summary>
    private async UniTask JumpKey_Accepting(CancellationTokenSource cts)
    {
        //Debug.Log("<color=red>JumpKey_Accepting</color>");
        isJumpKeyReactiveProperty.Value = true;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(playerStats.delayKey_reception_time), cancellationToken: cts.Token);
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
            isJumpKeyReactiveProperty.Value = false;
        }
    }

    public void Start_JumpKey_AcceptingTime()
    {
        if (isJumpKeyReactiveProperty.Value)
        {
            Stop_JumpKey_AcceptingTime();
        }
        cts = new CancellationTokenSource();
        JumpKey_Accepting(cts).Forget();
    }

    public void Stop_JumpKey_AcceptingTime()
    {
        if (isJumpKeyReactiveProperty.Value)
        {
            cts?.Cancel();
        }
    }

    private void Update()
    {
        if (isJumpKeyReactiveProperty.Value == false) return;

        if (inputHandler.IsJumpKeyDown())
        {
            //コルーチンの受け付け時間を終了する
            Stop_JumpKey_AcceptingTime();

            //wallkickに遷移する
            eventMediator.ChangeToWallKickState.OnNext(Unit.Default);
            //Debug.Log("<color=blue>WallKickStateに遷移させます</color>");
        }
    }
}
