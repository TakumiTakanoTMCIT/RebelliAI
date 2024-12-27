using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursolCtrl : MonoBehaviour
{
    [SerializeField] private Selectable initialSelectable;
    [SerializeField] private bool isCursolControllable = true;

    private void Start()
    {
        if (initialSelectable != null)
        {
            SetSeletedObject(initialSelectable);
        }
        else
        {
            Debug.LogError("Initial selectable is not assigned!");
        }

        isCursolControllable = true;
    }

    void Update()
    {
        if (!isCursolControllable) return;

        // 現在選択されているオブジェクトを取得
        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current == null)
        {
            Debug.Log("Current is not found.");
            return; // 選択がない場合はスキップ
        }

        // `Selectable`を取得
        Selectable selectable = current.GetComponent<Selectable>();
        if (selectable == null)
        {
            Debug.Log("Selectable is not found.");
            return; // `Selectable`がない場合はスキップ
        }

        Debug.Log($"Current selectable: {current.name}");
        Debug.Log($"Up: {selectable.FindSelectableOnUp()?.name ?? "None"}");
        Debug.Log($"Down: {selectable.FindSelectableOnDown()?.name ?? "None"}");
        Debug.Log($"Left: {selectable.FindSelectableOnLeft()?.name ?? "None"}");
        Debug.Log($"Right: {selectable.FindSelectableOnRight()?.name ?? "None"}");

        // 入力に応じて移動先を決定
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            NavigateTo(selectable.FindSelectableOnUp());
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            NavigateTo(selectable.FindSelectableOnDown());
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NavigateTo(selectable.FindSelectableOnLeft());
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NavigateTo(selectable.FindSelectableOnRight());
        }

        //決定ボタン
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Confilm();
        }
    }

    void NavigateTo(Selectable next)
    {
        if (next != null)
        {
            // 次のSelectableに移動
            EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color = Color.white;
            SetSeletedObject(next);
        }
    }

    private void SetSeletedObject(Selectable selectable)
    {
        EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        selectable.gameObject.GetComponent<Image>().color = Color.red;
        Debug.Log("Selected: " + selectable.gameObject.name);
    }

    //決定のメソッドです
    private void Confilm()
    {
        EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color = Color.yellow;
        Debug.LogWarning("Confilm: " + EventSystem.current.currentSelectedGameObject.name);
        isCursolControllable = false;
    }
}
