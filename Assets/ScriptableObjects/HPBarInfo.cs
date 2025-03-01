using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "HPBarInfo", menuName = "Game/HPBarInfo")]
public class HPBarInfo : ScriptableObject
{
    [SerializeField]
    public List<Sprite> sprites;

    [SerializeField]
    public Sprite defaultSprite;

    [SerializeField]
    private Vector2 randomWaitTime = new Vector2(0.055f, 0.2f);

    public float RandomWaitTime
    {
        get
        {
            return UnityEngine.Random.Range(randomWaitTime.x, randomWaitTime.y);
        }

        private set { }
    }

    [SerializeField]
    public List<Sprite> topSprites;

    [SerializeField]
    public Sprite topDefaultSprite;

    [SerializeField]
    public List<Sprite> midSprites;

    [SerializeField]
    public Sprite midDefaultSprite;

    [Serializable]
    public class DevidePoint
    {
        [SerializeField]
        public List<int> devidePoint;
    }

    [SerializeField]
    public List<DevidePoint> devidePoints;

    [SerializeField]
    private Vector2 unitRandomWaitTime = new Vector2(0.5f, 1.1f);

    public float UnitRandomWaitTime
    {
        get
        {
            return UnityEngine.Random.Range(unitRandomWaitTime.x, unitRandomWaitTime.y);
        }
    }

    [SerializeField]
    public float unitMoveThereshold = 0.1f;
}
