using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerHandler : MonoBehaviour
{
    public void OnExperiment(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("Experiment");
        }
    }
}
