using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HPItemInfo", menuName = "Game/HPItemInfo")]
public class HPItemInfo : ScriptableObject {

    public int healAmount = 1;
    public float initialDisplayTime = 2f;
    public float blinkingTime = 2f;
}
