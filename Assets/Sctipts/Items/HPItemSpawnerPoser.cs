using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class HPItemSpawnerPoser : MonoBehaviour
{
    //Inject
    private HPItemSpawner.Factory spawnerFactory;

    [SerializeField]
    float radius = 1f;

    [SerializeField]
    List<Vector2> positionsOfHPItems = new List<Vector2>();

    [Inject]
    public void Construct(HPItemSpawner.Factory hpItemSpawnerFactory)
    {
        this.spawnerFactory = hpItemSpawnerFactory;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var position in positionsOfHPItems)
        {
            Gizmos.DrawSphere(position, radius);
        }
    }

    private void Awake()
    {
        foreach (var position in positionsOfHPItems)
        {
            var hpItemSpawner = spawnerFactory.Create();
            hpItemSpawner.transform.SetParent(transform);
            hpItemSpawner.transform.position = position;
        }
    }
}
