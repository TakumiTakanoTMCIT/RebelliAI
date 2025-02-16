using UnityEngine;
using UniRx;
using Zenject;

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
        private IPlayerDirection playerDirection;

        [Inject]
        public void Construct(IPlayerDirection playerDirection, [Inject(Id = "Muzzle")] GameObject muzzleObj, MuzzlePositions muzzlePositions)
        {
            this.playerDirection = playerDirection;
            this.muzzlePositions = muzzlePositions;
            this.muzzleObj = muzzleObj;
        }

        private void Start()
        {
            // TODO : このEveryUpdateの書き方、UniRxの本領を発揮できてない。もっといい書き方があるはず。
            Observable.EveryUpdate()
                .Where(_ => playerDirection.Direction.Value == true)
                .Subscribe(_ =>
                {
                    foreach (var muzzlePosition in muzzlePositions.muzzlePositions)
                    {
                        muzzlePosition.pos.x = Mathf.Abs(muzzlePosition.pos.x);
                    }
                })
                .AddTo(this);

            Observable.EveryUpdate()
                .Where(_ => playerDirection.Direction.Value == false)
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
