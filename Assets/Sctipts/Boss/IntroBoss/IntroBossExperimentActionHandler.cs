using System;
using UnityEditor;
using UnityEngine;

namespace IntroBossExperimenter
{
    public class IntroBossExperimentActionHandler : MonoBehaviour
    {
        IntroBossExperiment introBoss;
        Rigidbody2D rb;
        [SerializeField] IntroBossExprtimentGroundChekcer groundChk;

        [SerializeField] bool isFalling = false;
        public bool IsFalling { get => isFalling; }

        public static event Action onFall;

        private void Awake()
        {
            rb = gameObject.MyGetComponent_NullChker<Rigidbody2D>();
            introBoss = gameObject.MyGetComponent_NullChker<IntroBossExperiment>();
        }

        private void Start()
        {
            isFalling = false;
        }

        void GetRigidBody()
        {
            //Activeselfがfalseの時にコンポーネントの取得ができるか実験してみる
            if (!gameObject.activeSelf)
            {
                Debug.LogWarning("gameObjectが非アクティブです。");
                //EditorApplication.isPaused = true;
                return;
            }

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody2Dの取得に失敗しました。");
                //EditorApplication.isPaused = true;
                return;
            }
        }

        public void Walk(float speed, bool direction)
        {
            if (introBoss.isDebugMode) Debug.LogAssertion($"Walk speed: {speed}, direction: {direction}");
            if (direction)
                rb.velocity = new Vector2(speed, rb.velocity.y);
            else
                rb.velocity = new Vector2(-speed, rb.velocity.y);
        }

        public void StopWalk()
        {
            if (introBoss.isDebugMode) Debug.LogAssertion("StopWalk");

            if (rb == null)
            {
                GetRigidBody();
            }
            if (rb == null)
            {
                Debug.LogError("Rigidbody2Dがnullです。");
                //EditorApplication.isPaused = true;
                return;
            }

            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        private void Update()
        {
            //地面ついていながら落下しているならそれは落下していないとみなします。落ちる床とかに対応するため
            if (groundChk.IsGround)
            {
                isFalling = false;
                return;
            }

            if (rb.velocity.y < 0f)
            {
                if (isFalling) return;
                isFalling = true;
                onFall?.Invoke();
            }
            else
            {
                isFalling = false;
            }
        }

        private void OnDestroy()
        {
            onFall = null;
            rb = null;
            introBoss = null;
            groundChk = null;
        }
    }
}
