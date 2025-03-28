using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AirborneMovementHandler : MovementHandler
{
    public static float strafeSpeed = 30f;
    public static float torqueStrength = 0.6f;
    public float horizontalDrag = 2.85f;
    public static float slideSpeed = 50f;
    public static float slideDistance = 2.5f;
    public static float slideAngle = 37.5f;

    public static float jumpStrength = 18f;
    public static float jumpForwardStrength = 11f;

    private float groundedTimer = 0f;
    public float jumpTimer = 0.6f;

    public float maxVelocity = 30f;

    int doubleJumps = 0;
    bool canDoubleJump = false;

    public void Move(PlayerCharacterController player)
    {
        // Mid-air strafe, with less control than grounded movement
        Vector3 direction = (player.orientation.right * player.xInput + player.orientation.forward * player.yInput).normalized;
        player.rb.AddForce(direction * strafeSpeed, ForceMode.Acceleration);

        // Lean in the direction we are moving
        Vector3 horizontalVelocity = player.rb.velocity;
        horizontalVelocity.y = 0;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * horizontalVelocity * torqueStrength, ForceMode.Acceleration);
        player.rb.AddForce(-horizontalVelocity * horizontalDrag, ForceMode.Acceleration);

        // If we aren't moving upwards and hit the ground, we are grounded
        if (player.onGround)
        {
            groundedTimer += 0.1f;
            if (player.rb.velocity.y < 0.01 || groundedTimer > 0.3f)
            {
                player.movementHandler = new GroundedMovementHandler();
            }            
        }
        else
        {
            groundedTimer = 0f;
        }

        // Double jump
        if (jumpTimer > 0.0f) jumpTimer -= 0.1f;

        if (canDoubleJump && doubleJumps < player.jumps - 1 && player.jumpInput && jumpTimer <= 0f)
        {
            Rigidbody rb = player.rb;
            Vector3 velocity = rb.velocity;
            velocity.y = jumpStrength;
            Vector3 forwardVelocity = player.orientation.transform.forward * jumpForwardStrength * player.yInput;
            velocity += forwardVelocity;
            rb.velocity = velocity;
            jumpTimer = 0.8f;
            doubleJumps++;
            canDoubleJump = false;
            player.cam.FovWarp(2.8f, .35f);
        }

        if (!canDoubleJump && !player.jumpInput && jumpTimer <= 0f)
        {
            canDoubleJump = true;
        }

        // Slide down steep slopes
        if (player.groundHit.distance < slideDistance && Vector3.Angle(player.groundHit.normal, Vector3.up) > slideAngle)
        {
            player.rb.AddForce(Vector3.down * slideSpeed, ForceMode.Acceleration);
        }

        //Cap velocity
        if (player.rb.velocity.magnitude > maxVelocity)
        {
            player.rb.AddForce(player.rb.velocity.normalized * (maxVelocity - player.rb.velocity.magnitude), ForceMode.VelocityChange);
        }
    }

    public bool ShouldHoverApply(PlayerCharacterController player)
    {
        return false;
    }
}