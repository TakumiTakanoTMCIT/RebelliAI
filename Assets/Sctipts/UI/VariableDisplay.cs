using UnityEngine;
using TMPro;

public class VariableDisplay : MonoBehaviour
{
    public TextMeshProUGUI variableText;

    [SerializeField]
    private Rigidbody2D playerRigidbody;

    void LateUpdate()
    {
        variableText.text = "playerRigidbody.velocity.x: " + playerRigidbody.velocity.x.ToString();
    }
}
