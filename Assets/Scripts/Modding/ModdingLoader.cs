using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KBBeat.Modding
{
    public class ModdingLoader : MonoBehaviour
    {
        public static ModdingLoader Instance { get; private set; }

        private void Awake() 
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadModInstances()
        {
            
        }

        private void Update()
        {
            
        }
    }

    public interface IKBBeatMod
    {
        public void Initialize();
        public void Update();
    }
}

