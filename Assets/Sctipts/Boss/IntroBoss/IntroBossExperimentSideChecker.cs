using UnityEngine;
using System;
public class IntroBossExperimentSideChecker : MonoBehaviour
{
    [SerializeField] private bool direction;

    bool isRecentlyEnteredWall = false;
    public static event Action<bool> onEnterWall;

    private void OnEnable()
    {
        IntroBossExperimenter.WalkState.onWalkReverseDirection += OnWalkReverseDirection;
    }

    private void OnDisable()
    {
        IntroBossExperimenter.WalkState.onWalkReverseDirection -= OnWalkReverseDirection;
    }

    //イベントハンドラ
    void OnWalkReverseDirection()
    {
        isRecentlyEnteredWall = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            if (isRecentlyEnteredWall) return;

            Debug.LogWarning($"Enterd {direction}Wall");
            onEnterWall?.Invoke(direction);
            isRecentlyEnteredWall = true;
        }
    }
}
