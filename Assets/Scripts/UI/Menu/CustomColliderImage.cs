using UnityEngine;
using UnityEngine.UI;

public class CustomColliderImage : Image
{
    private Collider2D targetCollider;
    protected override void Start()
    {
        base.Start();
        if (this.TryGetComponent<CustomColliderOverrider>(out var overrider))
        {
            this.targetCollider = overrider.OverrideTarget;
        }
        else
        {
            targetCollider = GetComponent<Collider2D>();
        }
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)

    {
        bool isRay = base.IsRaycastLocationValid(screenPoint, eventCamera);
        if (isRay && targetCollider != null)
        {
            bool isTrig = targetCollider.OverlapPoint(screenPoint);
            return isTrig;
        }
        return isRay;
    }
}