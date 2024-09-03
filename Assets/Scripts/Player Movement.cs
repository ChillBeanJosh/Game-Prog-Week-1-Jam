using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [HideInInspector] public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    bool isSprinting;
    public float groundDrag = 1f;
    public float airResistance = 0.5f;
    public float slideSpeed;

    float desiredMoveSpeed;
    float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    RaycastHit slopeHit;
    bool exitingSlope;

    [Header("Others")] public Transform orientation;

    [HideInInspector] public float horizontalInput;
    //[HideInInspector] public float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

    [HideInInspector] public bool sliding;
    [HideInInspector] public MovementState state;

    private Vector3 initialScale;

    void Start()
    {
        // get rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;

        initialScale = transform.localScale;
    }

    void Update()
    {
        // ground check
        // We do this by shooting a ray down from our current position to half the height of the player plus a little bit more to see if we hit the ground.
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        speedControl();
        StateHandler();

        // handle drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airResistance;
        }

        FlipPlayer();
    }

    void FixedUpdate()
    {
        MovePlayer();

        // check for valid uncrouch - works for crouching and slide uncrouching
        if (transform.localScale != new Vector3(transform.localScale.x, startYScale, transform.localScale.z))
        {
            if (!Input.GetKey(crouchKey) && ValidUncrouch())
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                state = MovementState.walking;
            }
        }
    }

    // get input
    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // uncrouching - should probably implement checking if you can safely stand up first
        // if (Input.GetKeyUp(crouchKey) && validUncrouch())
        // {
        //     transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        // }
    }

    void StateHandler()
    {
        // sliding
        // COMPLETED: implement sliding only when sprinting so that auto-uncrouching doesn't screw up
        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        // crouching - first if statement because least specific condition
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // sprinting
        // NOTE: sprinting state won't change properly
        else if (grounded && Input.GetKey(sprintKey))
        {
            isSprinting = !isSprinting;
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // walking
        else if (grounded && !isSprinting)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // air
        else
        {
            state = MovementState.air;
        }

        // check if desiredMoveSpeed has changed dramatically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float lerpTime = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (lerpTime < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, lerpTime / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                lerpTime += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                lerpTime += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.right * horizontalInput; //+ orientation.forward* verticalInput 

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            // if moving forward on slope, keep the player down
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // on ground
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    void speedControl()
    {

        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        else
        {
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }

    }

    void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            // calculate how steep the slope we're standing on is
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            // return whether or not the angle of the slope we're on is less than the max slope angle and isn't flat
            return angle < maxSlopeAngle && angle != 0;
        }

        // raycast didn't hit anything
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public bool ValidUncrouch()
    {
        // perform a raycast upward to see if there's enough space to walk after uncrouch
        Debug.DrawRay(transform.position, Vector3.up, Color.blue, 1f);

        if (!Physics.Raycast(transform.position, Vector3.up, playerHeight))
        {
            return true;
        }

        return false;
    }

    void FlipPlayer()
    {
        if (horizontalInput > 0)
        {
            // Moving right, face right
            transform.localScale = new Vector3(initialScale.x, transform.localScale.y, transform.localScale.z); 
        }
        else if (horizontalInput < 0)
        {
            // Moving left, face left (flip by inverting the x scale)
            transform.localScale = new Vector3(-initialScale.x, transform.localScale.y, transform.localScale.z); 
        }
    }
}
