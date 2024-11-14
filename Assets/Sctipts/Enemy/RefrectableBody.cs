using IntroBossExperimenter;
using UnityEngine;

public class RefrectableBody : MonoBehaviour
{
    float saveOffsetX;
    private void Awake()
    {
        saveOffsetX = transform.localPosition.x;
    }

    private void OnEnable()
    {
        IntroBossExperimentAnimCtrl.onChangeDirection += ChangeDirection;
    }

    private void OnDisable()
    {
        IntroBossExperimentAnimCtrl.onChangeDirection -= ChangeDirection;
    }

    void ChangeDirection(bool direction)
    {
        if (direction)
            transform.localPosition = new Vector2(-saveOffsetX, transform.localPosition.y);
        else
            transform.localPosition = new Vector2(saveOffsetX, transform.localPosition.y);
    }
}
