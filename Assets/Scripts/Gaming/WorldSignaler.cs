using KBbeat;
using UnityEngine;

public class WorldSignaler : MonoBehaviour
{
    public void Signal()
    {
        LevelPlayer.Instance.Signal();
    }
}
