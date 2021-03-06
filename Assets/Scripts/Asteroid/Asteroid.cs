﻿using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public enum AsteroidSize
{
    NONE = 0,
    SMALL,
    NORMAL,
    BIG
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Asteroid : MonoBehaviour
{
    public AsteroidSize currentSize = AsteroidSize.BIG;

    private Rigidbody mRigidbody;

    private bool ready = false;
    private Vector3 targetDir;

    public GameObject breakPartner = null;

    public int offsetDirection = 15;

    private float normalizedExtraScreen;

    private Vector3 rotateDirection;

    private void Awake()
    {
        mRigidbody = GetComponent<Rigidbody>();
        ready = false;
    }

    public void Init(AsteroidSize size, Vector3 target, bool fromBreak, float normalizedExtraScreen)
    {
        breakPartner = null;
        currentSize = size;
        mRigidbody.velocity = Vector3.zero;
        mRigidbody.rotation = Quaternion.identity;

        this.normalizedExtraScreen = normalizedExtraScreen;

        transform.rotation = Quaternion.identity;
        rotateDirection = Random.insideUnitCircle.normalized;

        ApplySize();
        if (!fromBreak)
        {
            Vector3 targetOffseted = target + new Vector3(Random.Range(-offsetDirection, offsetDirection), 0, Random.Range(-offsetDirection, offsetDirection));
            targetDir = targetOffseted - transform.position;
        }
        else
        {
            targetDir = target;
        }

        ready = true;
        gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        if (ready)
        {
            mRigidbody.AddForce(targetDir.normalized * VelocityBySize(), ForceMode.Impulse);
            ready = false;
        }

        if (CheckForDestruction())
        {
            DestroyAsteroid();
        }

        //transform.Rotate(rotateDirection * (int)currentSize * Time.deltaTime);
        mRigidbody.AddTorque(rotateDirection / ((int)currentSize * 10));
    }

    private float VelocityBySize()
    {
        switch (currentSize)
        {
            case AsteroidSize.SMALL: return 3f;
            case AsteroidSize.NORMAL: return 2f;
            case AsteroidSize.BIG: return 1f;
        }
        return 0f;
    }

    private void ApplySize()
    {
        switch (currentSize)
        {
            case AsteroidSize.SMALL:
                transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                break;
            default:
            case AsteroidSize.NORMAL:
                transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                break;
            case AsteroidSize.BIG:
                transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                break;

        }
    }

    private void Break()
    {
        DestroyAsteroid();

        AsteroidSize newSize = AsteroidSize.NONE;

        switch (currentSize)
        {
            case AsteroidSize.BIG:
                newSize = AsteroidSize.NORMAL;
                break;

            case AsteroidSize.NORMAL:
                newSize = AsteroidSize.SMALL;
                break;
        }

        if (newSize != AsteroidSize.NONE)
        {
            float alpha = 45f;
            Vector3 currentDir = mRigidbody.velocity;
            Vector3 inverseDir = -currentDir;

            Vector3 dir1 = new Vector3(Mathf.Cos(alpha) * inverseDir.x - Mathf.Sin(alpha) * inverseDir.z, 0, Mathf.Sin(alpha) * inverseDir.x + Mathf.Cos(alpha) * inverseDir.z);
            Vector3 dir2 = new Vector3(Mathf.Cos(-alpha) * inverseDir.x - Mathf.Sin(-alpha) * inverseDir.z, 0, Mathf.Sin(-alpha) * inverseDir.x + Mathf.Cos(-alpha) * inverseDir.z);

            GameObject asteroid1 = ObjectPooler.Instance.GetPooledObject("Asteroid");
            asteroid1.transform.position = transform.position;
            asteroid1.GetComponent<Asteroid>().Init(newSize, dir1, true, normalizedExtraScreen);

            GameObject asteroid2 = ObjectPooler.Instance.GetPooledObject("Asteroid");
            asteroid2.transform.position = transform.position;
            asteroid2.GetComponent<Asteroid>().Init(newSize, dir2, true, normalizedExtraScreen);

            asteroid1.GetComponent<Asteroid>().breakPartner = asteroid2;
            asteroid2.GetComponent<Asteroid>().breakPartner = asteroid1;
            EventManager.TriggerEvent(Statics.Events.asteroidBreak);
        }
    }

    private bool CheckForDestruction()
    {
        return SpawnerHelper.OffScreen(Camera.main.WorldToViewportPoint(transform.position), normalizedExtraScreen);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Asteroid"))
        {
            if (other.gameObject == breakPartner) return;
            Break();
        }
        else if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().DoDamage((int)currentSize);
            DestroyAsteroid();
        }
        else if (other.CompareTag("Base"))
        {
            DestroyAsteroid();
        }
    }

    public void DestroyAsteroid()
    {
        EffectsHelper.Particles("AsteroidCrash", transform.position);

        ready = false;
        gameObject.SetActive(false);
        EventManager.TriggerEvent(Statics.Events.asteroidDistroy);
    }
}
