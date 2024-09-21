using System;
using System.Collections;
using System.Collections.Generic;
using KBBeat;
using KBBeat.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KBBeat.Visual
{
    internal class Explosion : MonoBehaviour
    {
        [SerializeField] private GameObject cubePrefab;
        [SerializeField] private float surviveTime;

        [SerializeField] private float minSpeed;
        [SerializeField] private float maxSpeed;

        [SerializeField] private float minAngularSpeed;
        [SerializeField] private float maxAngularSpeed;

        [SerializeField] private float minSize;
        [SerializeField] private float maxSize;

        [SerializeField] private float maxPosOffset;

        [SerializeField] private float cubeShrinkSpeed;

        private GameObject[] cubes = null;
        private Vector3[] speed = null;
        private Vector3[] angularSpeed = null;


        private void OnEnable() 
        {
            if (cubes == null) 
            {
                this.cubes = new GameObject[20];
                this.speed = new Vector3[cubes.Length];
                this.angularSpeed = new Vector3[cubes.Length];
                for (int i = 0; i < this.cubes.Length; i++)
                {
                    this.cubes[i] = Instantiate(this.cubePrefab, this.transform);
                }
            }

            foreach (var c in cubes)
            {
                var size = Random.Range(minSize, maxSize);

                c.transform.localScale = new Vector3(size, size, size);

                c.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                    );

                c.transform.localPosition = new Vector3(
                    Random.Range(-maxPosOffset, maxPosOffset),
                    Random.Range(-maxPosOffset, maxPosOffset),
                    Random.Range(-maxPosOffset, maxPosOffset)
                    );
            }
            for (int i = 0; i < cubes.Length; i++)
            {
                speed[i] = cubes[i].transform.localPosition.normalized * Random.Range(minSpeed, maxSpeed);
                angularSpeed[i] = new Vector3(
                    Random.Range(minAngularSpeed, maxAngularSpeed),
                    Random.Range(minAngularSpeed, maxAngularSpeed),
                    Random.Range(minAngularSpeed, maxAngularSpeed)
                    );
            }

            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float timer = 0f;

            while (timer < this.surviveTime)
            {
                timer += Time.deltaTime;
                for (int i = 0; i < this.cubes.Length; i++)
                {
                    this.cubes[i].transform.eulerAngles += angularSpeed[i] * Time.deltaTime;
                    var newSize = Mathf.Max(0f, cubes[i].transform.localScale.x - cubeShrinkSpeed * Time.deltaTime);
                    this.cubes[i].transform.localScale = new(
                        newSize,
                        newSize,
                        newSize
                        );

                    if (speed[i] == Vector3.zero)
                    {
                        continue;
                    }

                    this.cubes[i].transform.localPosition += speed[i];
                    var nextSpeed = speed[i] - speed[i].normalized * Time.deltaTime;
                    if (Vector3.Dot(speed[i], nextSpeed) < 0f)
                    {
                        nextSpeed = Vector3.zero;
                    }
                    speed[i] = nextSpeed;
                }

                yield return null;
            }
            LevelPlayer.Instance.HitEffectPool.Release(this.gameObject);
            yield break;
        }
    }
}