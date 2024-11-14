using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UniRx;
using Blaster.State;
using System;

namespace Blaster
{
    public class BlasterManager : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject blaster;
        [SerializeField] private float moveYRange, moveXRange, moveYTime, moveXTime;

        DisplayCtrl displayCtrl;
        StateMachine stateMachine;
        IState idleState, moveState, shotState;

        private void Awake()
        {
            stateMachine = new StateMachine();
            displayCtrl = new DisplayCtrl(blaster);
            idleState = new State.Idle(playerTransform, blaster, moveYRange, moveXRange, moveYTime, moveXTime);
            moveState = new State.Move();
            shotState = new State.Shot();

            displayCtrl.HideBlaster();

            //stateMachine.ChangeState(idleState);
        }
    }

    public class DisplayCtrl
    {
        private static Subject<Unit> ondisplayBlaster = new Subject<Unit>();
        public static IObserver<Unit> OnDisplayBlaster => ondisplayBlaster;

        GameObject blaster;
        public DisplayCtrl(GameObject blaster)
        {
            this.blaster = blaster;

            ondisplayBlaster.Subscribe(_ => DisplayBlaster())
                .AddTo(blaster);
        }

        public void DisplayBlaster()
        {
            blaster.SetActive(true);
        }

        public void HideBlaster()
        {
            blaster.SetActive(false);
        }
    }

    namespace State
    {
        public class StateMachine
        {
            private IState currentState;

            public void ChangeState(IState nextState)
            {
                currentState?.Exit();
                currentState = nextState;
                currentState.Enter();
            }
        }

        public interface IState
        {
            void Enter();
            void Exit();
        }

        public class Idle : IState
        {
            Sequence sequence;

            private float moveYRange, moveXRange, moveYTime, moveXTime;
            private GameObject blaster;
            private Transform playerTransform;
            public Idle(Transform playerTransform, GameObject blaster, float moveYRange, float moveXRange, float moveYTime, float moveXTime)
            {
                this.moveYTime = moveYTime;
                this.moveXTime = moveXTime;
                this.moveXRange = moveXRange;
                this.moveYRange = moveYRange;
                this.blaster = blaster;
                this.playerTransform = playerTransform;
            }

            public void Enter()
            {
                blaster.transform.position = playerTransform.position;

                Debug.Log("Enter Idle");

                /*sequence = DOTween.Sequence().Join
                    (blaster.transform.DOLocalMoveX(moveXRange, moveXTime)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo));*/

                /*sequence.Join(blaster.transform.DOLocalMoveY(moveYRange, moveYTime)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo));*/
                Debug.Log($"moveYRange: {moveYRange}");
                blaster.transform.DOLocalMoveY(blaster.transform.localPosition.y + moveYRange, moveYTime)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo);
                //blaster.transform.DOMoveY(moveYRange, moveYTime);
                /*blaster.transform.DOLocalMoveY(moveYRange, moveYTime)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo);*/
            }

            public void Exit()
            {
                Debug.Log("Exit Idle");

                sequence.Kill();
            }
        }

        public class Move : IState
        {
            public void Enter()
            {
                Debug.Log("Enter Move");
            }

            public void Exit()
            {
                Debug.Log("Exit Move");
            }
        }

        public class Shot : IState
        {
            public void Enter()
            {
                Debug.Log("Enter Shot");
            }

            public void Exit()
            {
                Debug.Log("Exit Shot");
            }
        }
    }
}
