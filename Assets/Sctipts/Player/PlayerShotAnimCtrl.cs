using Zenject;
using HPBar;
using UnityEngine;
using UniRx;

namespace PlayerAnimCtrl
{
    public class PlayerShotAnimCtrl : MonoBehaviour
    {
        //Inject
        LifeManager lifeManager;
        HPBar.EventMediator hpbarEventMediator;

        [SerializeField] float shotAnimTime = 0.5f;

        private ITimer timer;
        private Animator animator;
        bool isShoting = false;

        [Inject]
        public void Construct(LifeManager lifeManager, HPBar.EventMediator eventMediator)
        {
            this.lifeManager = lifeManager;
            this.hpbarEventMediator = eventMediator;
        }

        private void Awake()
        {
            animator = this.gameObject.MyGetComponent_NullChker<Animator>();
            timer = new Timer(shotAnimTime);

            isShoting = false;

            lifeManager.OnPlayerDead.Subscribe(_ =>
            {
                EndShotAnim();
            })
            .AddTo(this);

            hpbarEventMediator.OnPlayerDamage.Subscribe(_ =>
            {
                EndShotAnim();
            })
            .AddTo(this);
        }

        private void OnEnable()
        {
            AllShellManager.onShotNow += OnShotNow;
        }

        private void OnDisable()
        {
            AllShellManager.onShotNow -= OnShotNow;
        }

        private void Update()
        {
            if (!isShoting) return;

            timer.Update();
            if (timer.IsTimeOver())
            {
                EndShotAnim();
            }
        }

        //イベントハンドラー
        //ショットしたら呼ばれる
        private void OnShotNow()
        {
            timer.Reset();

            //もしいまショットアニメーションなら最初から再生し直す
            if (animator.GetCurrentAnimatorStateInfo(1).IsName("ShotIdle"))
            {
                animator.Play("ShotIdle", 1, 0f);
                return;
            }

            animator.SetBool("isShoting", true);

            if (!isShoting)
                isShoting = true;
        }

        //タイマーが終了したら呼ばれる
        //ダメージか死んだときにも呼ばれる
        public void EndShotAnim()
        {
            //もしタイマーが再生中なら終了する
            if (isShoting) timer.Reset();

            animator.SetBool("isShoting", false);
            isShoting = false;
        }
    }

    public interface ITimer
    {
        void Update();
        void Reset();
        bool IsTimeOver();
    }

    public class Timer : ITimer
    {
        private float time = 0;
        private float limitTime = 0;

        public Timer(float limitTime)
        {
            this.limitTime = limitTime;
        }

        public void Update()
        {
            time += Time.deltaTime;
        }

        public void Reset()
        {
            time = 0;
        }

        public bool IsTimeOver()
        {
            return time >= limitTime;
        }
    }
}
