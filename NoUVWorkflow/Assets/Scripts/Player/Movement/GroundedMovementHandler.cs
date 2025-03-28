using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GroundedMovementHandler : MovementHandler
{
    public static float runSpeed = 168f;
    public static float jumpStrength = 14f;
    public static float horizontalDrag = 15f;
    public static float bobbingStrength = 0.3f;

    // Timers are public in case another state wants to set them before switching
    public float jumpTimer = .3f;
    public float coyoteTimer = .65f;

    private float bobbingCycle = 0f;

    public void Move(PlayerCharacterController player)
    {
        // Running around
        Vector3 direction = (player.orientation.right * player.xInput + player.orientation.forward * player.yInput).normalized;
        direction = Quaternion.FromToRotation(Vector3.up, player.groundHit.normal) * direction;
        player.rb.AddForce(direction * runSpeed, ForceMode.Acceleration);

        // Add camera bobbing torque
        bobbingCycle += Time.fixedDeltaTime * player.rb.velocity.magnitude / 2f;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * player.rb.velocity * bobbingStrength, ForceMode.Acceleration);
        float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * player.rb.velocity.magnitude * bobbingStrength / 8f;
        player.rb.AddTorque(player.orientation.forward * sidewaysBobbingMagnitude, ForceMode.Acceleration);

        // Jump has a slight cooldown after landing
        if (jumpTimer > 0.0f) jumpTimer -= 0.1f;

        // Jump and switch state to airborne
        if (player.jumpInput && jumpTimer <= 0f)
        {
            if(player.onGround)
            {
                Vector3 pos = player.transform.position;
                pos.y += player.hoverHeight - player.groundHit.distance;
                player.transform.position = pos;
            }

            Rigidbody rb = player.rb;
            Vector3 velocity = rb.velocity;
            velocity.y = jumpStrength;
            rb.velocity = velocity;
            player.movementHandler = new AirborneMovementHandler();
            return;
        }

        // Coyote time allows jumps briefly after walking off a platform
        if (player.onGround)
        {
            coyoteTimer = .65f;
        }
        else
        {
            coyoteTimer -= 0.07f;
            if (coyoteTimer <= 0f || Vector3.Angle(player.groundHit.normal, Vector3.up) > 37.5f)
            {
                player.movementHandler = new AirborneMovementHandler();
                return;
            }
        }

        // Drag, opposite to current horizontal velocity
        Vector3 horizontalVelocity = player.rb.velocity;
        horizontalVelocity.y = 0;
        player.rb.AddForce(-horizontalVelocity * horizontalDrag, ForceMode.Acceleration);
    }
}