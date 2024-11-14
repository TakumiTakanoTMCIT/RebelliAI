using UnityEngine;
using System;

namespace IntroBossExperimenter
{
    public class IntroBossExprtimentGroundChekcer : MonoBehaviour
    {
        public static event Action onGround;

        //正直isDebugModeを取得するだけのためにインスタンスを取得しているのでメンバ変数に新しくisDebugModeを作成してそちらを参照するように変更するほうがいいかもしれない
        [SerializeField] IntroBossExperiment introBoss;

        [SerializeField] private bool isGround = false;
        public bool IsGround { get => isGround; }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Ground")
            {
                if (introBoss.isDebugMode) Debug.Log("地面についた");
                isGround = true;
                onGround?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Ground")
            {
                if (introBoss.isDebugMode) Debug.Log("地面から離れた");
                isGround = false;
            }
        }
    }
}
