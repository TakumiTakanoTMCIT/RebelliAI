using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public static class GameObject_MyExtension
{
    public static void PauseGame()
    {
        EditorApplication.isPaused = true;
    }

    public static T MyGetComponent_NullChker<T>(this GameObject requester) where T : Component
    {
        var component = requester.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"{requester}'s {component.GetType()} が見つかりませんでした。追加してください。 呼び出し元は{requester}です");

            PauseGame();
        }
        //Debug.Log($"{requester}'s {component.GetType()} が見つかりました。");
        return component;
    }

    public static T GetOtherObjComponent_NullCheck<T>(this GameObject requester,  GameObject targetObj) where T : Component
    {
        if (targetObj == null)
        {
            Debug.LogError($"{typeof(T).Name} が見つかりませんでした。追加して作成してください。 呼び出し元は{requester}です");
            PauseGame();
            return null;
        }

        var getComponent = targetObj.GetComponent<T>();
        if (getComponent == null)
        {
            Debug.LogWarning($"{targetObj}'s {typeof(T).Name} が見つかりませんでした。追加してください 呼び出し元は{requester}です");
            PauseGame();
            return null;
        }

        return getComponent;
    }
}
