using UnityEngine;
using PlayerState;
using KeyHandler;
using ActionStatusChk;
using PlayerAction;
using Zenject;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStateMgr), typeof(ActionStatusChecker))]
[RequireComponent(typeof(WallKickDelayManager), typeof(PlayerDashTimeCtrl))]
[RequireComponent(typeof(PlayerDashKeepManager), typeof(InputHandler), typeof(PlayerWeapon_KeyController))]
[RequireComponent(typeof(PlayerAnimStateHandler))]
public class PlayerInitializer : MonoBehaviour
{
    /// <summary>
    /// 追加したいコンポーネントを格納する変数
    /// </summary>
    PlayerStateMgr playerStateMgr;

    ActionStatusChecker actionStatusChecker;
    WallKickDelayManager wallKickDelayManager;
    PlayerDashKeepManager dashKeepManager;
    PlayerAnimStateHandler animHandler;

    InputHandler inputHandler;
    PlayerWeapon_KeyController playerWeapon_KeyController;

    //他のオブジェクトにアタッチされているコンポーネントを取得するための変数
    [SerializeField] GameObject groundCheckerObj;
    [SerializeField] GameObject leftsideObj, rightsideObj, wallLeftObj, wallRightObj;
    [SerializeField] GameObject shellManagerObj;
    [SerializeField] GameObject chargeShotHandlerObj;

    [Inject]
    ActionHandler actionHandler;

    GroundChk groundChk;
    SideChecker leftside, rightside, wallleftside, wallrightside;
    AllShellManager shellManager;
    ChargeShot_Handler chargeShotHandler;

    //Unity側のコンポーネント-------------------------------------
    Rigidbody2D rb;

    private void Awake()
    {
        this.gameObject.MyGetComponent_NullChker<BoxCollider2D>();
        rb = this.gameObject.MyGetComponent_NullChker<Rigidbody2D>();

        playerStateMgr = this.gameObject.MyGetComponent_NullChker<PlayerStateMgr>();
        actionStatusChecker = this.gameObject.MyGetComponent_NullChker<ActionStatusChecker>();
        wallKickDelayManager = this.gameObject.MyGetComponent_NullChker<WallKickDelayManager>();
        dashKeepManager = this.gameObject.MyGetComponent_NullChker<PlayerDashKeepManager>();
        inputHandler = this.gameObject.MyGetComponent_NullChker<InputHandler>();
        playerWeapon_KeyController = this.gameObject.MyGetComponent_NullChker<PlayerWeapon_KeyController>();
        animHandler = this.gameObject.MyGetComponent_NullChker<PlayerAnimStateHandler>();

        groundChk = this.gameObject.GetOtherObjComponent_NullCheck<GroundChk>(groundCheckerObj);
        leftside = this.gameObject.GetOtherObjComponent_NullCheck<SideChecker>(leftsideObj);
        rightside = this.gameObject.GetOtherObjComponent_NullCheck<SideChecker>(rightsideObj);
        wallleftside = this.gameObject.GetOtherObjComponent_NullCheck<SideChecker>(wallLeftObj);
        wallrightside = this.gameObject.GetOtherObjComponent_NullCheck<SideChecker>(wallRightObj);
        shellManager = this.gameObject.GetOtherObjComponent_NullCheck<AllShellManager>(shellManagerObj);
        chargeShotHandler = this.gameObject.GetOtherObjComponent_NullCheck<ChargeShot_Handler>(chargeShotHandlerObj);

        playerStateMgr.Init(rb, animHandler, actionStatusChecker, inputHandler, actionHandler, wallKickDelayManager);
        actionStatusChecker.Init(groundChk, leftside, rightside, wallleftside, wallrightside, rb, playerStateMgr, inputHandler, animHandler);
        dashKeepManager.Init(actionStatusChecker, inputHandler);
        playerWeapon_KeyController.Init(inputHandler, chargeShotHandler, shellManager);
        wallKickDelayManager.Init(inputHandler);
    }
}
