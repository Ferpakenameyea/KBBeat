using KBBeat;
using UnityEngine;
using KBBeat.Core;

internal class WorldSignaler : MonoBehaviour
{
    public void SignalFadeInAnimationComplete()
    {
        LevelPlayer.Instance.Signal();
    }
}
