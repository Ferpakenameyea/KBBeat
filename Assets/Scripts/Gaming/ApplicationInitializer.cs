using UnityEngine;

public class ApplicationInitializer : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 144;

    private void Awake()
    {
        Application.targetFrameRate = this.targetFrameRate;
    }
}