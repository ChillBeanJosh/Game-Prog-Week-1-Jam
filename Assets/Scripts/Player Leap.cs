using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLeap : MonoBehaviour
{
    [Header("Refrences")]
    public Transform orientation;
    //public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;
    public Animator animator;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.Space;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
        {
            Dashing();
        }

        if (dashCdTimer > 0)
        {
            dashCdTimer -= Time.deltaTime;

        }

    }
    private void Dashing()
    {

        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        animator.SetTrigger("Leap");



        Vector3 forceToApply = orientation.up * dashForce + orientation.up * dashUpwardForce;
        rb.AddForce(forceToApply, ForceMode.Impulse);
        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash()
    {

    }
}
