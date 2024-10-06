using KeyHandler;
using UnityEngine;

public class PlayerWeapon_KeyController : MonoBehaviour
{
    ChargeShot_Handler chargeShotHandler;
    [SerializeField] private ShellManager mameManager;
    InputHandler inputHandler;

    [SerializeField] private GameObject levelLower_EnergyBall, fullLevel_EnergyBall;

    private void Start()
    {
        inputHandler = this.GetComponent<InputHandler>();
        chargeShotHandler = GameObject.Find("ShellHandler").GetComponent<ChargeShot_Handler>();
    }

    private void Update()
    {
        if (inputHandler.IsShootKeyDown())
        {
            if(!chargeShotHandler.IsCharging)
            {
                chargeShotHandler.StartCharge();
            }
            mameManager.ShootMame();
            return;
        }

        if (inputHandler.IsShootKeyUp())
        {
            if (!chargeShotHandler.IsLowCharged)
            {
                if(chargeShotHandler.IsMinimumChargeTime)
                {
                    mameManager.ShootMame();
                }

                chargeShotHandler.InterruputChaging();
                return;
            }
            else if(chargeShotHandler.IsLowCharged && !chargeShotHandler.IsFullCharged)
            {
                chargeShotHandler.Shoot_Charged_Shell(levelLower_EnergyBall);
            }
            else if(chargeShotHandler.IsFullCharged)
            {
                chargeShotHandler.Shoot_Charged_Shell(fullLevel_EnergyBall);
            }
        }
    }
}
