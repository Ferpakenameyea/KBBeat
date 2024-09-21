using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
internal class QuitButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(Application.Quit);
    }
}
