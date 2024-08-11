using KBBeat;
using UnityEngine;

namespace KBBeat.Core
{
    [RequireComponent(typeof(Animator))]
    public class DefaultTrack : Track
    {
        private Animator animator;
        protected void Start()
        {
            animator = GetComponent<Animator>();
        }

        public override void Spark()
        {
            this.animator.SetTrigger("HIT");

            var ex = LevelPlayer.Instance.HitEffectPool.Get();
            ex.transform.position = this.transform.position;
            var scmin = Mathf.Min(
                this.transform.lossyScale.x,
                this.transform.lossyScale.y,
                this.transform.lossyScale.z);
            ex.transform.localScale = new(scmin, scmin, scmin);
        }
    }

    public class Track : MonoBehaviour
    {
        public TrackGroup parent { get; internal set; }
        public virtual void Spark()
        {
            Debug.Log("SPARK");
        }
    }
}
