using UnityEditor;
using UnityEngine;
using Door;

[CustomEditor(typeof(BossDoorBody))]
public class DoorIDAssigner : Editor
{
    // 配列を定義
    string[] nouns = { "猫", "空", "夢", "時間", "世界","残響","虚無","虚空","嘘","破滅","終焉" };
    string[] verbs = { "飛ぶ", "泳ぐ", "歌う", "踊る", "考える" };
    string[] adjectives = { "青い", "静かな", "速い", "奇妙な", "優しい","黄昏の","破滅の","泡沫の","狡猾な","漆黒な" };
    string[] adverbs = { "ゆっくり", "急に", "静かに", "大胆に", "楽しく" };

    public override void OnInspectorGUI()
    {
        // 元のインスペクタを表示
        base.OnInspectorGUI();

        // 対象のBossDoorBodyスクリプトを取得
        BossDoorBody doorBody = (BossDoorBody)target;

        // ボタンを表示して押されたらIDを生成
        if (GUILayout.Button("Generate Door ID"))
        {
            if (string.IsNullOrEmpty(doorBody.doorID))
            {
                // 新しい一意のIDを生成
                string randomNoun = nouns[Random.Range(0, nouns.Length)];
                string randomVerb = verbs[Random.Range(0, verbs.Length)];
                string randomAdjective = adjectives[Random.Range(0, adjectives.Length)];
                string randomAdverb = adverbs[Random.Range(0, adverbs.Length)];

                doorBody.doorID = $"{randomAdjective}{randomNoun}が、{randomAdverb}{randomVerb}。";
                Debug.Log($"Generated Door ID: {doorBody.doorID}");

                // 変更をUnityに通知して保存可能にする
                EditorUtility.SetDirty(doorBody);
            }
        }
    }
}
