using ActionStatusChk;
using UnityEngine;

namespace ComponentProvider
{
    //プレイヤーオブジェクト本体に着いているコンポーネントは各クラスで取得してください
    //プレイヤーの子オブジェクトのコンポーネント、他のオブジェクトのコンポーネントはここで取得するようにします

    public class PlayerComponentProvider : MonoBehaviour
    {
        PlayerAnimStateHandler playerAnimStateHandler;
        ActionStatusChecker actionStatusChecker;
        PlayerWeapon_KeyController playerWeapon_KeyController;
        ConflictEnemyHandler conflictEnemyHandler;
        PlayerBoundaryExitHandler playerBoundaryExitHandler;

        [SerializeField] private DeathGlitchSparkFactory deathGlitchSparkFactory;
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private GroundChk groundChk;
        [SerializeField] private SideChecker leftside, rightside, wallLeft, wallRight;
        [SerializeField] private ChargeShot_Handler chargeShotHandler;
        [SerializeField] private AllShellManager allShellManager;
        [SerializeField] private DamageTimeHandler damageTimeHandler;
        [SerializeField] private HPBar.HPBarHandler hPBarHandler;

        private void Awake()
        {
            playerWeapon_KeyController = this.gameObject.MyGetComponent_NullChker<PlayerWeapon_KeyController>();
            playerAnimStateHandler = this.gameObject.MyGetComponent_NullChker<PlayerAnimStateHandler>();
            actionStatusChecker = this.gameObject.MyGetComponent_NullChker<ActionStatusChecker>();
            conflictEnemyHandler = this.gameObject.MyGetComponent_NullChker<ConflictEnemyHandler>();
            playerBoundaryExitHandler = this.gameObject.MyGetComponent_NullChker<PlayerBoundaryExitHandler>();

            new OtherObjectComponentProvider(deathGlitchSparkFactory, playerAnimStateHandler, gameFlowManager, playerWeapon_KeyController,chargeShotHandler, allShellManager,conflictEnemyHandler, hPBarHandler, playerBoundaryExitHandler);
            new ChildComponentProvider(actionStatusChecker, groundChk, leftside, rightside, wallLeft, wallRight, conflictEnemyHandler, damageTimeHandler);
        }
    }

    class ChildComponentProvider
    {
        public ChildComponentProvider(ActionStatusChecker actionStatusChecker, GroundChk groundChk, SideChecker left, SideChecker right, SideChecker wallleft, SideChecker wallright,ConflictEnemyHandler conflictEnemyHandler, DamageTimeHandler damageTimeHandler)
        {
            actionStatusChecker.ChildComponentGetter(groundChk, left, right, wallleft, wallright);
            conflictEnemyHandler.ChildComponentGetter(damageTimeHandler);
        }
    }

    class OtherObjectComponentProvider
    {
        public OtherObjectComponentProvider(DeathGlitchSparkFactory deathGlitchSparkFactory, PlayerAnimStateHandler playerAnimStateHandler, GameFlowManager gameFlowManager, PlayerWeapon_KeyController playerWeapon_KeyController,ChargeShot_Handler chargeShot_Handler, AllShellManager allShellManager, ConflictEnemyHandler conflictEnemyHandler, HPBar.HPBarHandler hPBarHandler, PlayerBoundaryExitHandler playerBoundaryExitHandler)
        {
            playerAnimStateHandler.OtherComponentGetter(deathGlitchSparkFactory, gameFlowManager);
            playerWeapon_KeyController.OtherComponentGetter(chargeShot_Handler, allShellManager);
            playerBoundaryExitHandler.OtherComponentGetter(hPBarHandler);
        }
    }
}
