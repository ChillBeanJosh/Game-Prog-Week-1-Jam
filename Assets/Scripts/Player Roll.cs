using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoll : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    //public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;
    public Animator animator;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;
    public float dashTravelTime;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.Q;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
        {
            StartCoroutine(Dashing());
        }

        if (dashCdTimer > 0)
        {
            dashCdTimer -= Time.deltaTime;
        }
    }

    private IEnumerator Dashing()
    {
        if (dashCdTimer > 0) yield break;
        dashCdTimer = dashCd;

        // Trigger roll animation
        animator.SetTrigger("Roll");

        // Wait for a brief moment before starting the dash
        yield return new WaitForSeconds(0.3f); // Adjust this delay as needed

        // Determine the dash direction based on the player's facing direction
        Vector3 dashDirection;
        if (pm.horizontalInput > 0) // Moving right
        {
            dashDirection = transform.right; 
        }
        else if (pm.horizontalInput < 0) // Moving left
        {
            dashDirection = -transform.right; 
        }
        else
        {
            yield break; // No horizontal input, exit coroutine
        }

        float elapsedTime = 0f;
        while (elapsedTime < dashTravelTime)
        {
            // Apply force gradually
            Vector3 forceToApply = dashDirection * dashForce + Vector3.up * dashUpwardForce; // [NEW]
            rb.AddForce(forceToApply * Time.deltaTime / dashTravelTime, ForceMode.VelocityChange); // [NEW]

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void ResetDash()
    {
        // Implement if needed
    }
}
