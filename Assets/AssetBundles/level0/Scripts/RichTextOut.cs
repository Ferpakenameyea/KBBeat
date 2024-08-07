using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class RichTextOut : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    
    [SerializeField] 
    [TextArea] 
    private string text;

    [SerializeField] private float delaySeconds;
    [SerializeField] private bool runOnAwake = true;

    private Coroutine coroutine = null;

    private void OnEnable()
    {
        textMeshPro = GetComponent<TextMeshPro>();
        if (string.IsNullOrEmpty(text))
        {
            text = textMeshPro.text;
        }

        if (runOnAwake)
        {
            Run();
        }
    }

    public void Run()
    {
        if (this.coroutine != null)
        {
            StopCoroutine(this.coroutine);
        }

        this.textMeshPro.text = string.Empty;
        this.coroutine = StartCoroutine(RunCoroutine());
    }

    private IEnumerator RunCoroutine()
    {
        StringBuilder sb = new();
        bool inLabel = false;
        foreach (char c in text)
        {
            sb.Append(c);
            if (c == '<')
            {
                inLabel = true;
            }
            else if (c == '>')
            {
                inLabel = false;
            }

            if (!inLabel)
            {
                this.textMeshPro.text = sb.ToString();
                yield return new WaitForSeconds(delaySeconds);
            }
        }
        this.coroutine = null;
        yield break;
    }
}
