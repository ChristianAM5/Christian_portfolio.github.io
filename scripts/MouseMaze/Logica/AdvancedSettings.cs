using System;
using UnityEngine;

[Serializable]
public class AdvancedSettings
{
    public float groundCheckDistance = 0.01f;
    public float stickToGroundHelperDistance = 0.5f;
    public float slowDownRate = 20f;
    public bool airControl;
    [Tooltip("Set to 0.1 or more if you get stuck in walls")]
    public float shellOffset;

    public void StickToGroundHelper(Transform t, CapsuleCollider capsule, Rigidbody rb)
    {
        float castRadius = capsule.radius * (1f - shellOffset);
        float castDistance = (capsule.height / 2f - capsule.radius) + stickToGroundHelperDistance;

        if (Physics.SphereCast(t.position, castRadius, Vector3.down, out RaycastHit hit,
            castDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < 85f)
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal);
        }
    }
}