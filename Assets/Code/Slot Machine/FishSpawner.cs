using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Prefabs - match order to your FishData array")]
    public GameObject[] fishPrefabs;

    [Header("Spawn")]
    public Transform spawnPoint;

    private GameObject _currentFish;

    public static FishSpawner Instance { get; private set; }

    void Awake() => Instance = this;

    public void SpawnFish(int fishIndex)
    {
        Debug.Log($"SpawnFish called — spawnPoint: {spawnPoint}, _currentFish: {_currentFish}");
        if (spawnPoint == null)
        {
            Debug.LogError("spawnPoint is null!");
            return;
        }
        //debugs

        if (_currentFish != null)
            Destroy(_currentFish);

        if (fishIndex < 0 || fishIndex >= fishPrefabs.Length) return;

        _currentFish = Instantiate(fishPrefabs[fishIndex], spawnPoint.position, spawnPoint.rotation);
    }

    public void ClearFish()
    {
        if (_currentFish != null)
            Destroy(_currentFish);
    }
}
