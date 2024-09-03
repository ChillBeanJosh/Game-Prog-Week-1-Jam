using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoll : MonoBehaviour
{
    [Header("Refrences")]
    public Transform orientation;
    //public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;

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

        // Determine the dash direction based on the player's facing direction
        Vector3 dashDirection;
        if (pm.horizontalInput > 0) // Moving right
        {
            dashDirection = transform.right; // [NEW]
        }
        else if (pm.horizontalInput < 0) // Moving left
        {
            dashDirection = -transform.right; // [NEW]
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

    }
}
