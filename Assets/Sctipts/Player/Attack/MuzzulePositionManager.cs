using UnityEngine;
using UniRx;
using Zenject;
using UnityEditor;
using Cinemachine.Utility;
using UnityEngine.UIElements;

namespace Muzzle
{
    //ステートに応じてポジションを変えるクラスはどうやって実装しよう...?
    public enum State
    {
        Idle,
        Walk,
        Dash,
        Jump,
        Fall,
        WallFall,
        WallKick
    }

    public class MuzzulePositionManager : MonoBehaviour
    {
        private MuzzlePositions.MuzzlePositionDatas currentState;

        //Inject
        private GameObject muzzleObj;
        private MuzzlePositions muzzlePositions;
        private ActionStatusChk.ActionStatusChecker actionStatusChecker;

        [Inject]
        public void Construct(ActionStatusChk.ActionStatusChecker actionStatusChecker, [Inject(Id = "Muzzle")] GameObject muzzleObj, MuzzlePositions muzzlePositions)
        {
            this.muzzlePositions = muzzlePositions;
            this.actionStatusChecker = actionStatusChecker;
            this.muzzleObj = muzzleObj;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
        }

        private void Start()
        {
            Observable.EveryUpdate()
                .Where(_ => actionStatusChecker.Direction)
                .Subscribe(_ =>
                {
                    foreach (var muzzlePosition in muzzlePositions.muzzlePositions)
                    {
                        muzzlePosition.pos.x = Mathf.Abs(muzzlePosition.pos.x);
                    }
                })
                .AddTo(this);

            Observable.EveryUpdate()
                .Where(_ => actionStatusChecker.Direction == false)
                .Subscribe(_ =>
                {
                    foreach (var muzzlePosition in muzzlePositions.muzzlePositions)
                    {
                        muzzlePosition.pos.x = -Mathf.Abs(muzzlePosition.pos.x);
                    }
                })
                .AddTo(this);

            Observable.EveryUpdate()
                .Where(_ => muzzleObj != null)
                .Where(_ => currentState != null)
                .Subscribe(_ =>
                    //muzzleObjはプレイヤーの子オブジェクトなので、ローカルの座標を変えてあげるだけdえ位置を変えれます！
                    muzzleObj.transform.localPosition = currentState.pos
                )
                .AddTo(this);
        }

        //プレイヤーのステートが変わるたびに呼び出す
        public void SetPosition(State setState)
        {
            foreach (MuzzlePositions.MuzzlePositionDatas nowMuzzle in muzzlePositions.muzzlePositions)
            {
                if (nowMuzzle.myState == setState)
                {
                    if (currentState != null)
                    {
                        currentState.isCurrentState = false;
                    }
                    currentState = nowMuzzle;
                    nowMuzzle.isCurrentState = true;
                }
            }
        }
    }
}
