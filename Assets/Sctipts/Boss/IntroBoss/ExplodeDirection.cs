using UnityEngine;
using UnityEditor;
using System;
using Cysharp.Threading.Tasks;
using UniRx;
public class ExplodeDirection : MonoBehaviour
{
    [SerializeField] ExplosionSpawner spawner;
    [SerializeField] Transform centerOfExplosionTransform;
    [SerializeField] float ExplosionPosY = 2f, spawnRange = 5f;
    [SerializeField] internal float explosionDirectionTime = 10f;
    [SerializeField] float minInterval = 0.1f, maxInterval = 1f;

    bool isExplodable = false;

    public static event Action onFinishExplodeDirection;

    private void OnEnable()
    {
        BossCutSceneHandler.onExplode += ExplodeCutScene;
    }

    private void OnDisable()
    {
        BossCutSceneHandler.onExplode -= ExplodeCutScene;
    }

    //ランダムな位置に爆発を生成
    void MakeExplosion()
    {
        //Debug.Log("MakeExplosion");
        Vector3 randPos = new Vector3(
            centerOfExplosionTransform.position.x + (Mathf.Cos(UnityEngine.Random.Range(-100, 100)) * spawnRange),
            ExplosionPosY + centerOfExplosionTransform.position.y + (Mathf.Sin(UnityEngine.Random.Range(-100, 100)) * spawnRange),
            0);
        spawner.MakeExplosion(randPos);
    }

    async void ExplodeCutScene()
    {
        Debug.Log("ExplodeCutScene");
        CountExplodionDirectionTime().Forget();

        while (isExplodable)
        {
            MakeExplosion();
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(minInterval, maxInterval)), ignoreTimeScale: true);
            }
            catch (Exception e)
            {
                Debug.Log($"awaitでエラーが起きました。{e}");
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }
        }

        onFinishExplodeDirection?.Invoke();
    }

    async UniTask CountExplodionDirectionTime()
    {
        isExplodable = true;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(explosionDirectionTime), ignoreTimeScale: true);
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //EditorApplication.isPaused = true;
            return;
        }
        isExplodable = false;
    }
}
