using UnityEngine;

public class PlayerShotAnimCtrl : MonoBehaviour
{
    private Animator animator;

    [SerializeField] float shotAnimTime = 2f;

    float countTime = 0f;
    bool isShoting = false;

    private void Awake()
    {
        animator = this.gameObject.MyGetComponent_NullChker<Animator>();
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

        if (countTime >= shotAnimTime)
        {
            animator.SetBool("isShoting", false);
            isShoting = false;
            return;
        }

        countTime += Time.deltaTime;
    }

    //イベントハンドラー
    private void OnShotNow()
    {
        animator.SetBool("isShoting", true);
        countTime = 0f;
        isShoting = true;
    }
}
