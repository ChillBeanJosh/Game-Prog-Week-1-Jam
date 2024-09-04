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

    [Header("Others")]
    public Transform orientation;
    public Animator animator; // [NEW]

    [HideInInspector] public float horizontalInput;
    //[HideInInspector] public float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        crouchWalking,
        sliding,
        air,
        idle // [NEW]
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
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        speedControl();
        StateHandler();

        // handle drag
        rb.drag = grounded ? groundDrag : airResistance;

        FlipPlayer();
    }

    void FixedUpdate()
    {
        MovePlayer();

        // check for valid uncrouch
        if (transform.localScale != new Vector3(transform.localScale.x, startYScale, transform.localScale.z))
        {
            if (!Input.GetKey(crouchKey) && ValidUncrouch())
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                state = MovementState.walking;
            }
        }
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            animator.SetTrigger("Jump"); // [NEW]
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // uncrouching
        // if (Input.GetKeyUp(crouchKey) && validUncrouch())
        // {
        //     transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        // }
    }

    void StateHandler()
    {
        // sliding
        if (sliding)
        {
            state = MovementState.sliding;
            desiredMoveSpeed = OnSlope() && rb.velocity.y < 0.1f ? slideSpeed : sprintSpeed;
        }
        // crouching
        else if (Input.GetKey(crouchKey))
        {
            if (horizontalInput == 0)
            {
                state = MovementState.crouching;
                desiredMoveSpeed = crouchSpeed;
            }
            else
            {
                state = MovementState.crouchWalking;
                desiredMoveSpeed = crouchSpeed;
            }
        }
        // sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            isSprinting = !isSprinting;
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // walking
        else if (grounded && horizontalInput != 0)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // idle
        else if (grounded && horizontalInput == 0)
        {
            state = MovementState.idle;
            desiredMoveSpeed = walkSpeed; // Or 0 if you want the player to stop
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

        // Update animations
        UpdateAnimations(); // [NEW]
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
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public bool ValidUncrouch()
    {
        Debug.DrawRay(transform.position, Vector3.up, Color.blue, 1f);
        return !Physics.Raycast(transform.position, Vector3.up, playerHeight);
    }

    void FlipPlayer()
    {
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(initialScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-initialScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    void UpdateAnimations() // [NEW]
    {
        animator.SetBool("isWalking", state == MovementState.walking);
        animator.SetBool("isSprinting", state == MovementState.sprinting);
        animator.SetBool("isCrouching", state == MovementState.crouching);
        animator.SetBool("isCrouchWalking", state == MovementState.crouchWalking);
        //animator.SetBool("isSliding", state == MovementState.sliding);
        animator.SetBool("isIdle", state == MovementState.idle);
        // Add other animation parameters as needed
    }
}
