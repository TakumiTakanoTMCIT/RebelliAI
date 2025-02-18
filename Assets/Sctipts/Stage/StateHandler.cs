using UnityEngine;
using Door;
using Zenject;
using UniRx;

public class StateHandler : MonoBehaviour
{
    private DoorManager doorManager;
    DoorIDAssignerHandler idLogic;

    public bool IsEntered { get; private set; } = false;

    [Inject]
    public void Construct(DoorManager doorManager)
    {
        this.doorManager = doorManager;
    }

    private void Awake()
    {
        idLogic = gameObject.MyGetComponent_NullChker<DoorIDAssignerHandler>();

        //ドアがすでに登録されているかを確認
        if (doorManager.IsResisteredDoor(idLogic.DoorID))
        {
            IsEntered = true;
        }
        else
        {
            IsEntered = false;
        }
    }
}
