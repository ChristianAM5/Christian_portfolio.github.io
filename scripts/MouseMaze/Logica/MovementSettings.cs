using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class MovementSettings
{
    public float ForwardSpeed = 8f;
    public float BackwardSpeed = 4f;
    public float StrafeSpeed = 4f;
    public float RunMultiplier = 2f;
    public float JumpForce = 5f;
    public AnimationCurve SlopeCurveModifier = new AnimationCurve(
        new Keyframe(-90f, 1f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

    [HideInInspector] public float CurrentTargetSpeed = 8f;

    private bool m_Running;
    public bool Running => m_Running;

    public void Run(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            m_Running = !m_Running;
    }

    public void UpdateDesiredTargetSpeed(Vector2 input)
    {
        if (input == Vector2.zero) return;

        // El orden importa: forward tiene prioridad sobre strafe
        if (input.x != 0) CurrentTargetSpeed = StrafeSpeed;
        if (input.y < 0) CurrentTargetSpeed = BackwardSpeed;
        if (input.y > 0) CurrentTargetSpeed = ForwardSpeed;

        if (m_Running) CurrentTargetSpeed *= RunMultiplier;
    }

    public float SlopeMultiplier(Vector3 groundNormal)
    {
        return SlopeCurveModifier.Evaluate(Vector3.Angle(groundNormal, Vector3.up));
    }
}