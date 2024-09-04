using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boulderPush : MonoBehaviour
{
    public Animator animator;                // Reference to the Animator component
    public float punchRadius = 2f;           // Radius of the OverlapSphere
    public LayerMask boulderLayer;           // Layer mask to detect the boulder
    public float pushDistance = 5f;          // Distance the boulder will be pushed back
    public float pushSpeed = 2f;             // Speed at which the boulder moves back
    public Transform punchOrigin;            // The origin point for the punch, usually the player's position
    public KeyCode punchFist = KeyCode.E;
    public float pushDelay = 0.5f;           // Adjustable delay before the boulder starts moving 

    private void Update()
    {
        if (Input.GetKeyDown(punchFist))
        {
            StartCoroutine(Punch());
        }
    }

    private IEnumerator Punch()
    {
        // Trigger punch animation
        animator.SetTrigger("Punch");

        // Detect objects within the punch radius
        Collider[] hitColliders = Physics.OverlapSphere(punchOrigin.position, punchRadius, boulderLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Boulder"))
            {
                // Determine the push direction based on the player's facing direction
                Vector3 pushDirection = transform.localScale.x == 1 ? Vector3.right : Vector3.left;

                // Wait for the specified delay before moving the boulder
                yield return new WaitForSeconds(pushDelay); 

                // Start the coroutine to move the boulder
                StartCoroutine(PushBoulder(hitCollider.transform, pushDirection));
            }
        }

        yield return null; // Ensure the coroutine has a return value for consistency
    }

    private IEnumerator PushBoulder(Transform boulder, Vector3 direction)
    {
        float distanceMoved = 0f;

        while (distanceMoved < pushDistance)
        {
            // Move the boulder
            float step = pushSpeed * Time.deltaTime;
            boulder.Translate(direction * step, Space.World);

            // Update the distance moved
            distanceMoved += step;

            yield return null;
        }
    }

    // Optional: Draw the overlap sphere in the editor for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(punchOrigin.position, punchRadius);
    }
}