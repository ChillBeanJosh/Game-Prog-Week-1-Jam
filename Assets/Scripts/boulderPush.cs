using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boulderPush : MonoBehaviour
{
    //public Animator animator;                // Reference to the Animator component
    public float punchRadius = 2f;           // Radius of the OverlapSphere
    public LayerMask boulderLayer;           // Layer mask to detect the boulder
    public float pushDistance = 5f;          // Distance the boulder will be pushed back
    public float pushSpeed = 2f;             // Speed at which the boulder moves back
    public Transform punchOrigin;           // The origin point for the punch, usually the player's position
    public KeyCode punchFist = KeyCode.E;


    void Update()
    {
        if (Input.GetKeyDown(punchFist))
        {
            Punch();
        }
    }

    void Punch()
    {
        // Trigger punch animation
        //animator.SetTrigger("Punch");

        // Detect objects within the punch radius
        Collider[] hitColliders = Physics.OverlapSphere(punchOrigin.position, punchRadius, boulderLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Boulder"))
            {
                // Calculate the push direction (opposite of the player's forward direction)
                Vector3 pushDirection = transform.right;

                // Start the coroutine to move the boulder
                StartCoroutine(PushBoulder(hitCollider.transform, pushDirection));
            }
        }
    }

    System.Collections.IEnumerator PushBoulder(Transform boulder, Vector3 direction)
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
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(punchOrigin.position, punchRadius);
    }
}
