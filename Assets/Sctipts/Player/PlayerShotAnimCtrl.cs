using System;
using HPBar;
using UnityEngine;

namespace PlayerAnimCtrl
{
    public class PlayerShotAnimCtrl : MonoBehaviour
    {
        [SerializeField] float shotAnimTime = 0.5f;

        private ITimer timer;
        private Animator animator;
        bool isShoting = false;

        private void Awake()
        {
            animator = this.gameObject.MyGetComponent_NullChker<Animator>();
            timer = new Timer(shotAnimTime);

            isShoting = false;
        }

        private void OnEnable()
        {
            HPBarHandler.onPlayerDamage += EndShotAnim;
            HPBarHandler.onPlayerDeath += EndShotAnim;

            AllShellManager.onShotNow += OnShotNow;
        }

        private void OnDisable()
        {
            HPBarHandler.onPlayerDamage -= EndShotAnim;
            HPBarHandler.onPlayerDeath -= EndShotAnim;
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
