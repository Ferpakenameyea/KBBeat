using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Bridger : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private GameObject brickPrefab;
    // [SerializeField] private int brickCount = 30;
    [SerializeField] private float placeTimeSeconds = 2f;
    [SerializeField] private float placeLerpSpeed = 0.01f;
    [SerializeField] private float resumeLerpSpeed = 0.5f;
    [SerializeField] private float aliveSeconds = 4f;
    [SerializeField] private Vector3 overplaceOffset;
    [SerializeField] private float resumeSeconds;
    private Vector3 BrickSize { get => brickPrefab.transform.lossyScale; }
    [SerializeField] private Vector3 startOffset;
    [SerializeField] private Vector3 gridOffset;
    private ObjectPool<GameObject> brickPool;
    private HashSet<(int, int)> brickedPositions = new();

    [SerializeField] private bool placing = true;

    private (int, int)? currentCoordinate;

    private void OnEnable() 
    {
        if (this.brickPool == null)
        {
            this.brickPool = new ObjectPool<GameObject>(
                () => Instantiate(brickPrefab),
                (o) => o.SetActive(true),
                (o) => o.SetActive(false),
                (o) => Destroy(o),
                false,
                maxSize: 100
            );
        }
    }

    private void OnDisable() 
    {
        this.brickPool?.Dispose();    
    }

    private void Update() 
    {
        if (this.target == null || this.placing == false)
        {
            return;
        }    

        var coor = 
        (
            Mathf.FloorToInt(this.target.position.x / this.BrickSize.x),
            Mathf.FloorToInt(this.target.position.z / this.BrickSize.z)
        );

        Debug.LogFormat("coor is {0}", coor);
        if ((this.currentCoordinate == null || this.currentCoordinate != coor) && 
            !this.brickedPositions.Contains(coor))
        {
            this.brickedPositions.Add(coor);
            this.currentCoordinate = coor;
            this.SummonBrick(coor);
        }

    }

    private void OnDestroy() 
    {
        this.brickPool?.Dispose();    
    }

    private void SummonBrick((int, int) gridCoordinate)
    {
        Vector3 pos = new (
            (gridCoordinate.Item1 + 0.5f) * this.BrickSize.x,
            target.position.y,
            (gridCoordinate.Item2 + 0.5f) * this.BrickSize.z
        );

        pos += gridOffset;

        GameObject brick = this.brickPool.Get();
        StartCoroutine(BridgerCoroutine(brick, pos, gridCoordinate));
    }

    private IEnumerator BridgerCoroutine(GameObject brick, Vector3 position, (int, int) coor)
    {
        Vector3 startPosition = position + this.startOffset;

        brick.transform.position = startPosition;

        Debug.LogFormat("brick started at: {0}", startPosition);

        float timer = 0f;

        Vector3 overplace = position + this.overplaceOffset;

        while (timer < this.placeTimeSeconds)
        {
            brick.transform.position = Vector3.Lerp(brick.transform.position, overplace, this.placeLerpSpeed);

            timer += Time.deltaTime;
            yield return null;
        }

        brick.transform.position = overplace;
        timer = 0f;

        while (timer < this.resumeSeconds)
        {
            brick.transform.position = Vector3.Lerp(brick.transform.position, position, this.resumeLerpSpeed);
            
            timer += Time.deltaTime;
            yield return null;
        }

        brick.transform.position = position;

        yield return new WaitForSeconds(this.aliveSeconds);

        this.brickPool.Release(brick);
        this.brickedPositions.Remove(coor);
    }
}
