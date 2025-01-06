using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ObjectPoolFactory;

/// <summary>
/// このクラスの責務は、ワープエフェクトのアニメーションを制御することです。
/// </summary>
namespace Warp
{
    public class WarpEffectBody : MonoBehaviour
    {
        //animatorのインスタンスを取得
        Animator animator;
        //WarpPoolのインスタンスを取得
        WarpPool factory;
        //エフェクトが表示される時間
        private float showingTime;

        //各クラスのインスタンスを生成
        private AnimatorCtrl animatorCtrl;
        private Timer timer;
        private Warp.PoolHandler poolHandler;
        private PositionSetter positionSetter;

        //オブジェクトプールで最初に生成されるので、このメソッドで初期化する
        private void Awake()
        {
            //コンポーネントを取得
            animator = gameObject.MyGetComponent_NullChker<Animator>();

            //各クラスのインスタンスを生成
            animatorCtrl = new AnimatorCtrl(animator);
            poolHandler = new PoolHandler(factory, gameObject);
            timer = new Timer(poolHandler);
            positionSetter = new PositionSetter(gameObject);
        }

        //ActiveSelfがfalseになったら、アニメーションをリセットする
        private void OnDisable()
        {
            animatorCtrl.ResetAnim();
        }

        //生成されたら、ポジションをXをプレイヤーの位置に、Yを指定の位置にする（YはFactoryで上から下になるように設定されます）
        //どのアニメーションにするのかここでランダムに指定する
        public void Init(WarpPool factory, float showingTime, float playerPosX, float PosY)
        {
            //TODO:ここの数値をfactoryiinfoから取得するようにする(Inject)
            this.factory = factory;
            this.showingTime = showingTime;
            positionSetter.SetPosition(playerPosX, PosY);
            animatorCtrl.StartAnim();
        }

        //アニメーションイベント
        void OnFinishStart()
        {
            animatorCtrl.FinishStart();
            timer.CountReleaseTime(showingTime).Forget();
        }

        /*//ある程度アニメーションしたら、エフェクトを消す
        async UniTask CountReleaseTime()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(showingTime));
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }

            factory.ReturnObject(gameObject);
        }*/
    }

    //アニメーションの制御をするクラス
    public class AnimatorCtrl
    {
        Animator animator;
        public AnimatorCtrl(Animator animator)
        {
            this.animator = animator;
        }

        //生成時
        public void StartAnim()
        {
            animator.SetInteger("RandColor", UnityEngine.Random.Range(1, 5));
        }

        //スタートアニメーション終了時
        public void FinishStart()
        {
            animator.SetBool("isStart", true);
        }

        //消滅時
        public void ResetAnim()
        {
            animator.SetBool("isStart", false);
            animator.SetInteger("RandColor", 0);
        }
    }

    //表示する時間を制御するクラス
    public class Timer
    {
        private Warp.PoolHandler poolHadnler;

        public Timer(Warp.PoolHandler poolHadnler)
        {
            this.poolHadnler = poolHadnler;
        }

        public async UniTask CountReleaseTime(float showingTime)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(showingTime));
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }
            finally
            {
                //エフェクトを消す
                poolHadnler.ReturnMeToPool();
            }
        }
    }

    //プールを制御するクラス
    //自分自身をいつプールに返すかを制御するクラスです！
    public class PoolHandler
    {
        private WarpPool _factory;
        private GameObject _mySelftObj;

        public PoolHandler(WarpPool factory, GameObject mySelftObj)
        {
            _factory = factory;
            this._mySelftObj = mySelftObj;
        }

        public void ReturnMeToPool()
        {
            _factory.ReturnObject(_mySelftObj);
        }
    }

    //自分の座標を制御するクラス
    public class PositionSetter
    {
        private GameObject mySelftObj;

        public PositionSetter(GameObject mySelftObj)
        {
            this.mySelftObj = mySelftObj;
        }

        public void SetPosition(float playerPosX, float PosY)
        {
            mySelftObj.transform.position = new Vector3(playerPosX, PosY, 0);
        }
    }
}
