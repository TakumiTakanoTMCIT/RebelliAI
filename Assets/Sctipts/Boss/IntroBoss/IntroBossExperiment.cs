using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;
using System.Threading;
using Unity.VisualScripting;

interface IBoss
{
    void WakeUp();
    void Sleep();
}

namespace IntroBossExperimenter
{
    public class IntroBossExperiment : MonoBehaviour, IBoss
    {
        [SerializeField] internal float minspeed = 1.0f, maxSpeed = 8.0f, minIdleTime = 0.2f, maxIdleTime = 2.0f, minWalkTime = 0.5f, maxWalkTime = 1.3f;

        [SerializeField] private int damage = 3;

        [SerializeField] internal bool isDebugMode = false;

        internal IState currentState, idleState, walkState, attackState, fallState,deathState;
        internal bool isStartCutScene = false;
        IntroBossExperimentAnimCtrl animStateCtrl;
        [SerializeField] IntroBossExprtimentGroundChekcer groundChk;

        public static event Action onWakeUp;

        private void Awake()
        {
            isStartCutScene = false;
            animStateCtrl = gameObject.MyGetComponent_NullChker<IntroBossExperimentAnimCtrl>();

            if (isDebugMode) Debug.Log("IntroBossがAwakeされました");

            idleState = new IdleState(this, animStateCtrl, groundChk);
            walkState = new WalkState(this, animStateCtrl);
            attackState = new AttackState(this, animStateCtrl);
            fallState = new FallState(this, groundChk);
            deathState = new DeathState();
        }

        private void OnEnable()
        {
            BossCutSceneHandler.onApperanceCutSceneStart += OnStartCutScene;
            BossCutSceneHandler.onStartBattle += BattleStart;
            IntroBossHPBarHandler.onDead += Sleep;
        }

        private void OnDisable()
        {
            BossCutSceneHandler.onApperanceCutSceneStart -= OnStartCutScene;
            BossCutSceneHandler.onStartBattle -= BattleStart;
            IntroBossHPBarHandler.onDead -= Sleep;
        }

        //イベントハンドラ
        void OnStartCutScene()
        {
            isStartCutScene = true;
            currentState.OnStartCutScene();
        }

        void BattleStart()
        {
            isStartCutScene = false;
            currentState.OnBattleStart();
        }

        public void GetComponents(IntroBossExperimentAnimCtrl animCtrl, IntroBossExprtimentGroundChekcer groundChekcer)
        {
            groundChk = groundChekcer;
            animStateCtrl = animCtrl;
        }

        //インターフェース実装
        public void WakeUp()
        {
            Debug.Log("IntroBossがWakeUp");
            onWakeUp?.Invoke();

            currentState = idleState;
            Debug.Log("IdleStateにいくよ");
            currentState.OnEnter();
        }

        //インターフェース実装
        public void Sleep()
        {
            ChangeState(deathState);
            animStateCtrl.ChangeAnimState(animStateCtrl.deathState);
        }

        public void ChangeState(IState nextState)
        {
            if (currentState == nextState)
            {
                Debug.LogError("ChangeStateにて同じステートが指定された。");
                //EditorApplication.isPaused = true;
                return;
            }

            if (isDebugMode) Debug.Log($"{currentState.GetType().Name}から{nextState.GetType().Name}に遷移します");

            currentState.OnExit();
            currentState = nextState;
            currentState.OnEnter();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            var conflictEnemy = other.gameObject.GetComponent<IConflictEnemy>();
            if (conflictEnemy == null) return;
            conflictEnemy.OnConflictEnemy(damage);
        }

        private void OnDestroy()
        {
            if (currentState != null)
            {
                currentState.OnExit();
                currentState = null;
            }

            idleState = null;
            walkState = null;
            attackState = null;
            fallState = null;

            animStateCtrl = null;
            groundChk = null;
        }
    }

    public interface IState
    {
        void OnStartCutScene();
        void OnBattleStart();

        void OnEnter();
        void OnExit();
    }

    public class IdleState : IState
    {
        IntroBossExperiment stateMgr;
        IntroBossExperimentAnimCtrl animStateCtrl;
        IntroBossExprtimentGroundChekcer groundChk;

        CancellationTokenSource cts;

        public IdleState(IntroBossExperiment stateMgr, IntroBossExperimentAnimCtrl animCtrl, IntroBossExprtimentGroundChekcer groundChk)
        {
            this.groundChk = groundChk;
            this.animStateCtrl = animCtrl;
            this.stateMgr = stateMgr;
        }

        public void OnEnter()
        {
            //イベント登録
            IntroBossExperimentActionHandler.onFall += GoToFallState;

            animStateCtrl.ChangeAnimState(animStateCtrl.idleState);

            if (stateMgr.isStartCutScene) return;

            CountGoToWalk().Forget();
        }

        //イベントハンドラー
        public void OnStartCutScene()
        {
            //カットシーン中は何もしない。戦闘開始されるまでずっと待機
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        public void OnBattleStart()
        {
            CountGoToWalk().Forget();
        }

        private async UniTask CountGoToWalk()
        {
            try
            {
                cts = new CancellationTokenSource();
                await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(stateMgr.minIdleTime, stateMgr.maxIdleTime)), cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
                if (stateMgr.isDebugMode) Debug.Log("歩きがキャンセルされました");
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました、{e.Message}");
                //EditorApplication.isPaused = true;
                return;
            }

            stateMgr.ChangeState(stateMgr.walkState);
        }

        //イベントハンドラー
        private void GoToFallState()
        {
            if (stateMgr.isDebugMode) Debug.Log("落下ステートに遷移します");
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            stateMgr.ChangeState(stateMgr.fallState);
        }

        public void OnExit()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            //イベント解除
            IntroBossExperimentActionHandler.onFall -= GoToFallState;
        }
    }

    public class WalkState : IState
    {
        bool isRecentlyEnteredWall;
        bool saveWallDirection;
        internal bool direction;
        CancellationTokenSource cts;

        IntroBossExperimentActionHandler actionHandler;
        IntroBossExperiment stateMgr;
        IntroBossExperimentAnimCtrl animStateCtrl;

        public static event Action onWalkReverseDirection;

        public WalkState(IntroBossExperiment stateMgr, IntroBossExperimentAnimCtrl animCtrl)
        {
            try
            {
                this.animStateCtrl = animCtrl;
                this.stateMgr = stateMgr;
                actionHandler = stateMgr.gameObject.MyGetComponent_NullChker<IntroBossExperimentActionHandler>();
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました、{e.Message}");
                Debug.Log("IntroBossExperimentのインスペクタを確認してください");
                //EditorApplication.isPaused = true;
                return;
            }
        }

        public void OnStartCutScene()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            stateMgr.ChangeState(stateMgr.idleState);
        }

        public void OnEnter()
        {
            //イベント登録
            IntroBossExperimentSideChecker.onEnterWall += OnEnterWall;

            animStateCtrl.ChangeAnimState(animStateCtrl.walkState);

            //方向をランダムに決定
            //さっき壁に触れたなら触れた方向と逆に進む
            if (isRecentlyEnteredWall)
            {
                onWalkReverseDirection?.Invoke();
                direction = !saveWallDirection;
                isRecentlyEnteredWall = false;
            }
            else
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    direction = true;
                }
                else
                {
                    direction = false;
                }
            }

            animStateCtrl.SetWalkDirection(direction);

            actionHandler.Walk(UnityEngine.Random.Range(stateMgr.minspeed, stateMgr.maxSpeed), direction);

            cts = new CancellationTokenSource();
            CountTimeToAttackState().Forget();
        }

        public void OnBattleStart() { }

        private async UniTask CountTimeToAttackState()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(stateMgr.minWalkTime, stateMgr.maxWalkTime)), cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
                if (stateMgr.isDebugMode) Debug.Log("攻撃がキャンセルされました");
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました、{e.Message}");
                //EditorApplication.isPaused = true;
                return;
            }

            stateMgr.ChangeState(stateMgr.attackState);
        }

        private void OnEnterWall(bool wallDirectioin)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            isRecentlyEnteredWall = true;
            saveWallDirection = wallDirectioin;
            stateMgr.ChangeState(stateMgr.idleState);
        }

        public void OnExit()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            //イベント解除
            IntroBossExperimentSideChecker.onEnterWall -= OnEnterWall;

            actionHandler.StopWalk();
        }
    }

    public class AttackState : IState
    {
        IntroBossExperiment stateMgr;
        IntroBossExperimentAnimCtrl animStateCtrl;
        IntroBossAttackPoolCtrl poolCtrl;
        public AttackState(IntroBossExperiment stateMgr, IntroBossExperimentAnimCtrl animCtrl)
        {
            this.animStateCtrl = animCtrl;
            this.stateMgr = stateMgr;
            poolCtrl = stateMgr.gameObject.MyGetComponent_NullChker<IntroBossAttackPoolCtrl>();
        }

        public void OnStartCutScene() { }
        public void OnBattleStart() { }

        public void OnEnter()
        {
            //イベント登録
            IntroBossExperimentAnimCtrl.onEndAttackAnim += OnEndAttackAnim;

            animStateCtrl.ChangeAnimState(animStateCtrl.attackState);

            Attack();
        }

        void Attack()
        {
            WalkState walkState = stateMgr.walkState as WalkState;
            if (walkState == null)
            {
                Debug.LogError("WalkStateが取得できませんでした");
                //EditorApplication.isPaused = true;
                return;
            }

            poolCtrl.GetAttack().gameObject.MyGetComponent_NullChker<IntroBossAttackBody>().StartMove(walkState.direction, stateMgr.transform);
        }

        //イベントハンドラー
        public void OnEndAttackAnim()
        {
            if (stateMgr.isDebugMode) Debug.LogWarning("攻撃アニメーション終了");
            stateMgr.ChangeState(stateMgr.idleState);
        }

        public void OnExit()
        {
            //イベント解除
            IntroBossExperimentAnimCtrl.onEndAttackAnim -= OnEndAttackAnim;
        }
    }

    public class FallState : IState
    {
        private IntroBossExperiment stateMgr;

        public void OnStartCutScene() { }
        public void OnBattleStart() { }

        public FallState(IntroBossExperiment stateMgr, IntroBossExprtimentGroundChekcer groundChk)
        {
            this.stateMgr = stateMgr;
        }

        public void OnEnter()
        {
            if (stateMgr.isDebugMode) Debug.Log("FallStateに遷移しました");
            IntroBossExprtimentGroundChekcer.onGround += OnGround;
        }

        //イベントハンドラー
        private void OnGround()
        {
            stateMgr.ChangeState(stateMgr.idleState);
        }

        public void OnExit()
        {
            IntroBossExprtimentGroundChekcer.onGround -= OnGround;
        }
    }

    public class DeathState : IState
    {
        public void OnStartCutScene() { }
        public void OnBattleStart() { }

        public void OnEnter() { }

        public void OnExit() { }
    }
}
