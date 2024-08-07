using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static readonly Dictionary<string, ObjectPool> NameToPoolInstance = new();

    [SerializeField] protected GameObject prefab = null;
    [SerializeField] protected int defaultCount = 10;
    [SerializeField] protected int count = 0;
    [SerializeField] protected int countIncreaseStep = 10;
    [SerializeField] protected string poolName = null;
    [SerializeField] protected bool needSceneAcross = false;
    [SerializeField] protected float decreaseAwaitTime_seconds = 1f;
    public bool Using { get => this.count == this.transform.childCount; }
    public bool Initialized { get; private set; } = false;
    private bool registered = false;

    private Coroutine decreaseCoroutine;
    public GameObject TargetPrefab { get => prefab; }

    private void InitializeThisPool()
    {
        for (int i = 0; i < this.defaultCount; i++)
        {
            Instantiate(this.prefab, this.transform).SetActive(false);
        }
        this.count = defaultCount;
        this.Initialized = true;
    }

    protected virtual void Awake()
    {
        if (this.prefab == null)
        {
            Debug.LogWarning($"Invalid object pool with an unknown target prefab!");
            Destroy(gameObject);
            return;
        }

        this.poolName ??= this.gameObject.name;
        ObjectPool.RegisterPool(this);
        if (this.needSceneAcross) DontDestroyOnLoad(this.gameObject);
        InitializeThisPool();
        this.decreaseCoroutine = StartCoroutine(DecreaseCoroutine());
    }

    protected virtual void OnDestroy()
    {
        if (registered)
        {
            ObjectPool.UnregisterPool(this);
            if (Initialized)
            {
                StopCoroutine(this.decreaseCoroutine);
            }
        }
    }

    private void InstantiateMore()
    {
        for (int i = 0; i < this.countIncreaseStep; i++)
        {
            Instantiate(this.prefab, this.transform).SetActive(false);
        }
        this.count += countIncreaseStep;
    }

    public GameObject Get(bool setActive)
    {
        if (this.transform.childCount == 0)
        {
            this.InstantiateMore();
        }
        var g = this.transform.GetChild(0).gameObject;
        if (setActive) g.SetActive(true);
        g.transform.parent = null;
        return g;
    }

    public void Release(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.parent = this.transform;
        return;
    }

    private IEnumerator DecreaseCoroutine()
    {
        while (this.enabled == true)
        {
            yield return new WaitForSeconds(this.decreaseAwaitTime_seconds);
            if (this.count > this.defaultCount && this.transform.childCount >= this.countIncreaseStep)
            {
                for (int i = 0; i < this.countIncreaseStep; i++)
                {
                    Destroy(this.transform.GetChild(0).gameObject);
                }
            }
        }
        yield break;
    }

    public static bool RegisterPool(ObjectPool pool)
    {
        if (NameToPoolInstance.ContainsKey(pool.poolName))
        {
            NameToPoolInstance.TryGetValue(pool.poolName, out ObjectPool existing);
            Debug.LogWarning($"Registering pool {pool.poolName} failed, because it already exists! ({existing.gameObject})");
            return false;
        }

        pool.registered = true;
        NameToPoolInstance.Add(pool.poolName, pool);
        Debug.Log($"Successfully registered pool {pool.poolName}. ({pool.gameObject})");
        return true;
    }

    public static bool TryGetPool(string name, out ObjectPool pool)
    {
        return NameToPoolInstance.TryGetValue(name, out pool);
    }

    public static bool ContainsPool(string name)
    {
        return NameToPoolInstance.ContainsKey(name);
    }

    public static bool UnregisterPool(ObjectPool pool)
    {
        var result = NameToPoolInstance.Remove(pool.poolName);
        if (result)
        {
            Debug.Log($"Successfully unregistered pool {pool.poolName}.");
        }
        else
        {
            Debug.LogError($"Unregistering pool {pool.poolName} failed.");
        }
        pool.registered = false;
        return result;
    }
}

public class DynamicObjectPool : ObjectPool
{
    internal static Queue<DynamicObjectPool> yieldedPools = new();
    internal static int DynamicPoolCount { get; private set; }
    public int DefaultCount
    {
        get
        {
            return defaultCount;
        }
        set
        {
            defaultCount = value;
        }
    }
    public int CountIncreaseStep
    {
        get
        {
            return countIncreaseStep;
        }
        set
        {
            this.countIncreaseStep = value;
        }
    }
    public new GameObject TargetPrefab
    {
        get
        {
            return this.prefab;
        }
        set
        {
            if (this.Initialized)
            {
                Debug.LogWarning($"You can't set the prefab of an already initialized object pool!");
                return;
            }
            this.prefab = value;
        }
    }

    protected override void Awake()
    {
        return;
    }

    internal void Initialize()
    {
        DynamicPoolCount++;
        if (DynamicPoolReleaser.Instance == null)
        {
            DynamicPoolReleaser.Create();
        }
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DynamicPoolCount--;
        Debug.Log($"Dynamic object pool {this.gameObject.name} has been destoryed");
    }

    public void Kill()
    {
        yieldedPools.Enqueue(this);
    }
}

public static class DynamicObjectPoolFactory
{
    public static DynamicObjectPool Get(string name, GameObject prefab, int defaultCount, int step)
    {
        var obj = new GameObject(name);
        var component = obj.AddComponent<DynamicObjectPool>();
        component.DefaultCount = defaultCount;
        component.CountIncreaseStep = step;
        component.TargetPrefab = prefab;

        component.Initialize();
        return component;
    }
}

public class DynamicPoolReleaser : MonoBehaviour
{
    public static void Create()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = new GameObject().AddComponent<DynamicPoolReleaser>();
    }

    internal static DynamicPoolReleaser Instance { get; private set; } = null;
    private Coroutine releaseCoroutine = null;

    private void Awake()
    {
        this.releaseCoroutine = StartCoroutine(this.ReleaseCoroutine());
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        if (this.releaseCoroutine != null)
        {
            StopCoroutine(this.releaseCoroutine);
        }
        Instance = null;
    }

    private IEnumerator ReleaseCoroutine()
    {
        while (DynamicObjectPool.DynamicPoolCount != 0)
        {
            yield return new WaitForSeconds(60);
            if (!DynamicObjectPool.yieldedPools.TryDequeue(out var pool))
            {
                continue;
            }
            if (pool == null)
            {
                continue;
            }

            if (!pool.Using)
            {
                Destroy(pool.gameObject);
            }
            else
            {
                DynamicObjectPool.yieldedPools.Enqueue(pool);
            }
            yield return null;
        }
        Instance = null;
        DestroyImmediate(this.gameObject);
        yield break;
    }
}