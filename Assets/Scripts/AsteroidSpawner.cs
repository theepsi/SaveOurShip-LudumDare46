﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public float spawnRate = 2f;
    public int spawnAmount = 2;
    public float normalizedExtraScreen = 0.5f;

    private Coroutine spawner;
    private Camera mainCam;

    public void Init(Camera mainCam)
    {
        this.mainCam = mainCam;
    }

    public void StartSpawner()
    {
        spawner = StartCoroutine(Spawner());
    }

    public void StopSpawner()
    {
        if (spawner != null)
        {
            StopCoroutine(spawner);
            spawner = null;
        }
    }

    private IEnumerator Spawner()
    {
        for (;;)
        {
            yield return new WaitForSeconds(spawnRate);

            Spawn();
        }
    }

    private void Spawn()
    {
        for (int i = 0; i < spawnAmount; ++i)
        {
            GameObject asteroid = ObjectPooler.Instance.GetPooledObject("Asteroid");

            Vector2 position = SpawnerHelper.SpawnPosition(normalizedExtraScreen);

            asteroid.transform.position = mainCam.ViewportToWorldPoint(new Vector3(position.x, position.y, mainCam.transform.position.y));
            AsteroidSize randomSize = (AsteroidSize)Random.Range(1, System.Enum.GetNames(typeof(AsteroidSize)).Length);
            asteroid.GetComponent<Asteroid>().Init(randomSize, GameManager.Instance.player.transform.position, false, normalizedExtraScreen);
        }
    }
}
