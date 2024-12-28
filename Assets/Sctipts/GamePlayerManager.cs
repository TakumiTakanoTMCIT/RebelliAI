using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UniRx;
using Door;

public class GamePlayerManager : MonoBehaviour
{
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

    private void Awake()
    {
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
            Time.timeScale = 1;
        }).AddTo(this);

        PauseTime.Subscribe(_ =>
        {
            Time.timeScale = 0;
        }).AddTo(this);
    }

    private void OnEnable()
    {
        DeathPanelCtrl.onFinishDeathPanel += RestartStage;
        PlayerAnimStateHandler.onPlayerDeathAnimEnd += InvisiblePlayer;

        BossCutSceneHandler.onStartExplodeCutScene += StopTime;
        BossCutSceneHandler.onExplode += StartTime;

        IntroBossHPBarHandler.onDead += OnBossDead;
    }

    private void OnDisable()
    {
        DeathPanelCtrl.onFinishDeathPanel -= RestartStage;
        PlayerAnimStateHandler.onPlayerDeathAnimEnd -= InvisiblePlayer;

        BossCutSceneHandler.onStartExplodeCutScene -= StopTime;
        BossCutSceneHandler.onExplode -= StartTime;

        IntroBossHPBarHandler.onDead -= OnBossDead;
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
