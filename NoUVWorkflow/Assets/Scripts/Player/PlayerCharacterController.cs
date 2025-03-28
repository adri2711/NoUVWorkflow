using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCharacterController : MonoBehaviour
{
    public bool braincell = false;
    public Transform orientation;
    public LayerMask groundMask;
    public RaycastHit groundHit;

    [Header("Properties")]
    [Range(1,3)]
    public int jumps = 1;

    [Header("Cooldowns")]
    public float ability1Cooldown = 3f;
    public float ability2Cooldown = 5f;

    [Header("Vertical Forces")]
    public float gravityStrength = 45f;
    public float hoverHeight = 2f;
    public float hoverStrength = 45f;
    public float hoverDamp = 20f;

    [Header("Rotation Correction")]
    public float rotationCorrectionStrength = 100f;
    public float rotationCorrectionDamp = 70f;

    public Rigidbody rb { get; private set; }
    public FirstPersonCamera cam { get; private set; }
    public bool onGround { get; private set; }
    public float xInput { get; private set; }
    public float yInput { get; private set; }
    public bool jumpInput { get; private set; }
    public bool ability1Input { get; private set; }
    public bool ability2Input { get; private set; }

    public float ability1Time = 0f;
    public float ability2Time = 0f;

    public MovementHandler movementHandler;

    public Transform hand;
    public GameObject display;
    public Canvas crosshairCanvas;
    private TextMeshProUGUI crosshair;

    public float airTime = 0f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<FirstPersonCamera>();
        if (braincell)
        {
            display.SetActive(false);
        }
        else
        {
            cam.gameObject.SetActive(false);
        }
        movementHandler = new GroundedMovementHandler();
        crosshairCanvas.gameObject.SetActive(true);
        crosshair = crosshairCanvas.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        CheckGround();
        CheckInput();
        HandWobble();
        CalculateAirTime();
        CalculateCooldowns();
    }

    private void FixedUpdate()
    {
        if (movementHandler.ShouldGravityApply(this))
        {
            ApplyGravity();
        }

        if (movementHandler.ShouldHoverApply(this))
        {
            Hover();
        }

        CorrectRotation();
        movementHandler.Move(this);
    }

    private void ApplyGravity()
    {
        float gravity = jumpInput && jumps > 1 && rb.velocity.y < -4f ? gravityStrength * 0.2f : gravityStrength;
        rb.AddForce(Vector3.down * gravity , ForceMode.Acceleration);
    }

    private void Hover()
    {
        if (!onGround) return;
        float heightDiff = groundHit.distance - hoverHeight;
        rb.AddForce(Vector3.down * (heightDiff * hoverStrength + rb.velocity.y * hoverDamp), ForceMode.Acceleration);
    }

    private void CorrectRotation()
    {
        Quaternion correction = Quaternion.FromToRotation(transform.up, Vector3.up);
        correction.ToAngleAxis(out float alpha, out Vector3 axis);
        rb.AddTorque(axis.normalized * alpha * Mathf.Deg2Rad * rotationCorrectionStrength - rb.angularVelocity * rotationCorrectionDamp, ForceMode.Acceleration);
    }

    private void ApplyTerminalVelocity()
    {
        float tv = 12.5f;
        if (rb.velocity.y <= tv || rb.velocity.y > 0) return;
        Vector3 terminalVelocity = rb.velocity;
        terminalVelocity.y = tv;
        rb.velocity = terminalVelocity;
    }
    private void CheckGround()
    {
        float raycastMargin = 0.5f;
        onGround = Physics.Raycast(transform.position + Vector3.up * (raycastMargin + 0.05f), Vector3.down, out groundHit, hoverHeight * 5.0f, groundMask);
        if (!onGround) groundHit.distance = float.PositiveInfinity;
        onGround &= groundHit.distance < hoverHeight + raycastMargin && Vector3.Angle(groundHit.normal, Vector3.up) < 37.5f;
    }

    private void CheckInput()
    {
        if (!braincell)
        {
            yInput = 0f;
            xInput = 0f;
            jumpInput = false;
            ability1Input = false;
            return;
        }
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");
        jumpInput = Input.GetButton("Jump");
        ability1Input = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Return);
    }

    private void HandWobble()
    {
        if (!onGround) return;
        Vector3 wandPos = new Vector3(Mathf.Sin(Time.time * 5f), Mathf.Sin(Time.time * 6f));
        float wobbleMagnitude = Mathf.Clamp(rb.velocity.magnitude / 25f, 0f, 1f);
        wandPos *= wobbleMagnitude * 0.05f;
        hand.localPosition = wandPos;
    }

    private void CalculateAirTime()
    {
        if (!onGround)
        {
            airTime += Time.deltaTime;
            if (airTime > 5f)
            {
            }
        }
    }

    private void CalculateCooldowns()
    {
        ability1Time = Mathf.Max(0f, ability1Time - Time.deltaTime);
        ability2Time = Mathf.Max(0f, ability2Time - Time.deltaTime);
    }

    public void SetCrosshairColor(Color color)
    {
        crosshair.color = color;
    }
}
