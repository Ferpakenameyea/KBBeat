using UnityEngine;

namespace KBBeat.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        private AudioSource audioSource;
        
        private bool paused = false;
        public bool Stopped { get => !audioSource.isPlaying && !this.paused; }
        public bool Paused { get => this.paused; }
        public AudioClip Clip { get => audioSource.clip; set => audioSource.clip = value; }
        public float Volume { get => audioSource.volume; set => audioSource.volume = value; }
        public float Pitch { get => audioSource.pitch; set => audioSource.pitch = value; }
        public float Time { get => audioSource.time; set => audioSource.time = value; }
        public bool Repeat { get => audioSource.loop; set => audioSource.loop = value; }
        private void Awake()
        {
            this.audioSource = GetComponent<AudioSource>();
        }

        public void Play() => audioSource.Play();
        public void Stop() => audioSource.Stop();
        public void Pause()
        {
            if (this.Stopped)
            {
                return;
            }
            this.paused = true;
            audioSource.Pause();
        }
        public void UnPause()
        {
            if (this.Stopped)
            {
                return;
            }
            this.paused = false;
            audioSource.UnPause();
        }
    }

}