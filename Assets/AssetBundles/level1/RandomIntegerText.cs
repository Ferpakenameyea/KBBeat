using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
[ExecuteInEditMode]
public class RandomIntegerText : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    [SerializeField] private float updateInterval;
    [SerializeField][Min(0)] private int min;
    [SerializeField][Min(0)] private int max;

    private float timer = 0f;

    private void Start()
    {
        this.textMeshPro = GetComponent<TextMeshPro>();
    }

    private void OnEnable()
    {
        this.timer = 0f;
    }

    private void Update()
    {
        this.timer += Time.deltaTime;
        if (this.timer > this.updateInterval)
        {
            this.timer = 0f;
            this.textMeshPro.text = ((int)Random.Range(min, max)).ToString();
        }
    }
}
