using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SavePointDoorCtrler))]
public class SavePoint_Door_EditorCtrl : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SavePointDoorCtrler pointSaver = (SavePointDoorCtrler)target;

        if(GUILayout.Button("Reset SavePoints"))
        {
            pointSaver.pointSaver.InitSavePoint();
            EditorUtility.SetDirty(pointSaver);

            Debug.Log("セーブポイントをリセットしました。");
        }

        if(GUILayout.Button("Reset Doors"))
        {
            pointSaver.doorManager.ResetDoorStates();
            EditorUtility.SetDirty(pointSaver);

            Debug.Log("ドアの状態をリセットしました。");
        }
    }
}
