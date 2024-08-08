using KBBeat;
using UnityEngine;


namespace AssetBundles.levels.level1
{
    [RequireComponent(typeof(Animator))]
    public class CustomTrack : Track
    {
        private Animator animator;
        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        public override void Spark()
        {
            this.animator.SetTrigger("HIT");
            var ex = LevelPlayer.Instance.HitEffectPool.Get();
            ex.transform.position = this.transform.position;
            ex.transform.localScale = this.transform.lossyScale;
        }
    }
}