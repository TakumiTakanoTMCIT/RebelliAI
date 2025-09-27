using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float parallaxFactor = 0.5f; // 奥行き感 (0〜1)
    private Transform cam;
    private Vector3 lastCamPos;

    bool isInit = false;

    private void Awake()
    {
        isInit = false;
    }

    public void Init()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);

        isInit = true;

        Debug.Log("呼ばれた！");
    }

    private void LateUpdate()
    {
        if (!isInit) return;

        Vector3 delta = cam.position - lastCamPos;
        transform.position += delta * parallaxFactor;
        lastCamPos = cam.position;
    }
}
