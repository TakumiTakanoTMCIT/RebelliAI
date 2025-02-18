using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;

public class GamePlayerManager : MonoBehaviour
{
    //Inject
    LifeManager lifeManager;

    [SerializeField] PlayerAnimStateHandler playerAnimStateHandler;
    [SerializeField] GameObject playerObj, playerPanelObj;

    [SerializeField] internal bool isInGameArea = false, isDebugMode = false;

    public static event Action onPlayerOffStage;

    [SerializeField] float settingSceneMovingSpeed = 4.0f;
    private static float cameraChangingSceneSpeed;
    public static float CameraChangingSceneSpeed { get { return cameraChangingSceneSpeed; } }

    public Subject<Unit> DeactivePlayer = new Subject<Unit>();
    public Subject<Unit> ActivePlayer = new Subject<Unit>();

    public Subject<Unit> EnableTime = new Subject<Unit>();
    public Subject<Unit> PauseTime = new Subject<Unit>();

    EventStreamer eventStreamer;

    [Inject]
    public void Construct(LifeManager lifeManager, EventStreamer eventStreamer)
    {
        this.lifeManager = lifeManager;
        this.eventStreamer = eventStreamer;
    }

    private void Awake()
    {
        /// <summary>
        /// 重要！フレームレートの設定
        /// </summary>
        Application.targetFrameRate = 60;

        //static変数はインスペクタで表示できないのでゴリ押しで代入！
        cameraChangingSceneSpeed = settingSceneMovingSpeed;

        playerObj.SetActive(true);

        DeactivePlayer.Subscribe(_ =>
        {
            //プレイヤーを非表示
            playerObj.SetActive(false);
        }).AddTo(this);

        ActivePlayer.Subscribe(_ =>
        {
            //プレイヤーを表示
            playerObj.SetActive(true);
        }).AddTo(this);

        EnableTime.Subscribe(_ =>
        {
            Debug.LogWarning("時間を再開");
            Time.timeScale = 1;
        }).AddTo(this);

        PauseTime.Subscribe(_ =>
        {
            Debug.LogWarning("時間を止める");
            Time.timeScale = 0;
        }).AddTo(this);

        //プレイヤーが死んだ時の処理
        playerAnimStateHandler.OnPlayerDeathAnimEnd.Subscribe(_ =>
        {
            InvisiblePlayer();
        })
        .AddTo(this);

        lifeManager.OnPlayerDead.Subscribe(_ =>
        {
            PlayerDeath();
        })
        .AddTo(this);

        eventStreamer.startBossDoorCutScene.Subscribe(_ =>
        {
            StopTime();
        })
        .AddTo(this);

        eventStreamer.finishBossDoorCutScene.Subscribe(_ =>
        {
            StartTime();
        })
        .AddTo(this);
    }

    private void OnEnable()
    {
        DeathPanelCtrl.onFinishDeathPanel += RestartStage;

        BossCutSceneHandler.onStartExplodeCutScene += StopTime;
        BossCutSceneHandler.onExplode += StartTime;

        IntroBossHPBarHandler.onDead += OnBossDead;
    }

    private void OnDisable()
    {
        DeathPanelCtrl.onFinishDeathPanel -= RestartStage;

        BossCutSceneHandler.onStartExplodeCutScene -= StopTime;
        BossCutSceneHandler.onExplode -= StartTime;

        IntroBossHPBarHandler.onDead -= OnBossDead;
    }

    private void PlayerDeath()
    {
        isInGameArea = false;
    }

    void OnBossDead()
    {
        playerPanelObj.SetActive(false);
    }

    //イベントハンドラー
    void InvisiblePlayer()
    {
        onPlayerOffStage?.Invoke();
        playerObj.SetActive(false);
    }

    //イベントハンドラー
    void RestartStage()
    {
        if (isDebugMode) Debug.Log("RestartGame");
        Destroy(playerObj);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void StopTime()
    {
        if (isDebugMode) Debug.Log("時間を止める");
        Time.timeScale = 0;
    }

    void StartTime()
    {
        if (isDebugMode) Debug.Log("TimeResume");
        Time.timeScale = 1;
    }
}
