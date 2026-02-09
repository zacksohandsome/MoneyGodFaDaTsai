using System.Collections;
using UnityEngine;

public class BonusSpawner : MonoBehaviour
{
    public BonusObject bonusPrefab;

    [Header("時間設定")]
    [SerializeField] private float minTime = 100f;
    [SerializeField] private float maxTime = 200f;

    [Header("出現位置")]
    [SerializeField] private Vector2 spawnPositionXRange;
    [SerializeField] private Vector2 spawnPositionYRange;

    private float nextSpawnTime;

    private void Start()
    {
        SetNextSpawnTime();
        StartCoroutine(SpawnRoutine());
    }

    private void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minTime, maxTime);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(nextSpawnTime);

            // 開啟 BonusObject
            float spawnX = Random.Range(spawnPositionXRange.x, spawnPositionXRange.y);
            float spawnY = Random.Range(spawnPositionYRange.x, spawnPositionYRange.y);
            bonusPrefab.SetPosition(new Vector2(spawnX, spawnY));
            bonusPrefab.Show();

            SetNextSpawnTime();
        }
    }
}
