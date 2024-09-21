using UnityEngine;

[RequireComponent(typeof(CustomColliderImage))]
internal class CustomColliderOverrider : MonoBehaviour
{
    [SerializeField] private Collider2D targetCollider;
    public Collider2D OverrideTarget { get => targetCollider; }
}
