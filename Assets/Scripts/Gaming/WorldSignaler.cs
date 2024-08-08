using KBbeat;
using UnityEngine;

public class WorldSignaler : MonoBehaviour
{
    public void SignalFadeInAnimationComplete()
    {
        LevelPlayer.Instance.Signal();
    }
}
