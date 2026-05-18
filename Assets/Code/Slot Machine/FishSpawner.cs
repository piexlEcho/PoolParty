using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Prefabs - match order to your FishData array")]
    public GameObject[] fishPrefabs; // Shrimp, Hokkagi, Tuna, Squid, Salmon, Avocado

    [Header("Spawn")]
    public Transform spawnPoint; // Place this on top of your rice pile

    private GameObject _currentFish;

    public static FishSpawner Instance { get; private set; }

    void Awake() => Instance = this;

    public void SpawnFish(int fishIndex)
    {
        // Clear any previous fish
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
