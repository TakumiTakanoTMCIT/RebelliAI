using UnityEngine;
using System;

public class Example : MonoBehaviour
{
    [SerializeField] Example2 example2; // インスペクターに表示される

    void Start()
    {
        // example2 のフィールドにアクセス
        Debug.Log("Initial ExampleFloat: " + example2.ExampleFloat);

        // 値を変更する
        example2.ExampleFloat = 42.0f;

        // 変更後の値を出力
        Debug.Log("Updated ExampleFloat: " + example2.ExampleFloat);
    }
}

[Serializable]
public class Example2
{
    [SerializeField] float exampleFloat; // private フィールドでもインスペクターに表示

    // プロパティでアクセス可能にする
    public float ExampleFloat
    {
        get { return exampleFloat; }
        set { exampleFloat = value; }
    }
}
