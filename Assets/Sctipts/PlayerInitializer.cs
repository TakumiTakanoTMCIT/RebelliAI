using UnityEngine;
using PlayerInfo;
using PlayerState;
using KeyHandler;
using Unity.VisualScripting;
using ActionStatusChk;
using PlayerAction;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStatus), typeof(PlayerStateMgr), typeof(ActionStatusChecker))]
[RequireComponent(typeof(ActionHandler), typeof(WallKickDelayManager), typeof(PlayerDashTimeCtrl))]
[RequireComponent(typeof(PlayerDashKeepManager), typeof(InputHandler), typeof(PlayerWeapon_KeyController))]
[RequireComponent(typeof(PlayerAnimStateHandler))]
public class PlayerInitializer : MonoBehaviour
{
    /// <summary>
    /// 追加したいコンポーネントを格納する変数
    /// </summary>
    PlayerStatus playerStatus;
    PlayerStateMgr playerStateMgr;

    ActionStatusChecker actionStatusChecker;
    ActionHandler actionHandler;
    WallKickDelayManager wallKickDelayManager;
    PlayerDashTimeCtrl playerDashTimeCtrl;
    PlayerDashKeepManager dashKeepManager;
    PlayerAnimStateHandler animHandler;

    InputHandler inputHandler;
    PlayerWeapon_KeyController playerWeapon_KeyController;

    //他のオブジェクトにアタッチされているコンポーネントを取得するための変数
    [SerializeField] GameObject groundCheckerObj;
    [SerializeField] GameObject leftsideObj, rightsideObj, wallLeftObj, wallRightObj;
    [SerializeField] GameObject shellManagerObj;
    [SerializeField] GameObject chargeShotHandlerObj;

    GroundChk groundChk;
    SideChecker leftside, rightside, wallleftside, wallrightside;
    AllShellManager shellManager;
    ChargeShot_Handler chargeShotHandler;

    //Unity側のコンポーネント-------------------------------------
    Rigidbody2D rb;

    //アタッチされてほしいコンポーネント-------------------------------
    BoxCollider2D boxCollider;

    private void Awake()
    {
        this.gameObject.MyGetComponent_NullChker<BoxCollider2D>();
        rb = this.gameObject.MyGetComponent_NullChker<Rigidbody2D>();

        playerStatus = this.gameObject.MyGetComponent_NullChker<PlayerStatus>();
        playerStateMgr = this.gameObject.MyGetComponent_NullChker<PlayerStateMgr>();
        actionStatusChecker = this.gameObject.MyGetComponent_NullChker<ActionStatusChecker>();
        actionHandler = this.gameObject.MyGetComponent_NullChker<ActionHandler>();
        wallKickDelayManager = this.gameObject.MyGetComponent_NullChker<WallKickDelayManager>();
        playerDashTimeCtrl = this.gameObject.MyGetComponent_NullChker<PlayerDashTimeCtrl>();
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
        chargeShotHandler =  this.gameObject.GetOtherObjComponent_NullCheck<ChargeShot_Handler>(chargeShotHandlerObj);

        playerStateMgr.Init(rb, playerStatus, actionHandler, actionStatusChecker, inputHandler, dashKeepManager, wallKickDelayManager,animHandler);
        actionStatusChecker.Init(groundChk, leftside, rightside, wallleftside, wallrightside, playerStatus, rb);
        actionHandler.Init(rb, actionStatusChecker, playerStatus, dashKeepManager);
        playerStatus.Init(inputHandler, actionStatusChecker, playerStateMgr, animHandler);
        wallKickDelayManager.Init(playerStatus, playerStateMgr);
        playerDashTimeCtrl.Init(playerStatus);
        dashKeepManager.Init(actionStatusChecker, inputHandler);
        playerWeapon_KeyController.Init(inputHandler, chargeShotHandler, shellManager);
        chargeShotHandler.Init(playerStatus, shellManager);
    }
}
