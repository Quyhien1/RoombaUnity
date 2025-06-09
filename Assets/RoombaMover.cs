using UnityEngine;
using System.Collections; // Required for using Coroutines

[RequireComponent(typeof(Rigidbody2D))]
public class RoombaMover : MonoBehaviour
{
    public float speed = 2f;
    public float pauseDuration = 0.5f; // How long to pause and turn after a collision

    private Rigidbody2D rb;
    private Vector2 direction;
    private bool isMoving = true; // State to control movement

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        // A high angular damping helps prevent unintended spinning from physics interactions
        rb.angularDamping = 10f;
        PickInitialDirection();
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            // Apply constant velocity based on the object's forward direction (transform.up)
            rb.linearVelocity = transform.up * speed;
        }
        else
        {
            // Stop all movement when not in the 'moving' state
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only trigger the collision logic if the Roomba is currently moving.
        // This prevents new collisions from interrupting the turning sequence.
        if (isMoving && collision.contactCount > 0)
        {
            // Calculate the reflection vector from the first contact point
            Vector2 incomingDirection = transform.up;
            Vector2 normal = collision.contacts[0].normal;
            direction = Vector2.Reflect(incomingDirection, normal).normalized;

            // Start the pause-and-turn sequence
            StartCoroutine(HandleCollision());
        }
    }

    private IEnumerator HandleCollision()
    {
        // 1. Stop Movement
        isMoving = false;

        // 2. Turn the Head Slowly
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;

        // Calculate the target angle for the new direction.
        // We subtract 90 degrees because a sprite's "forward" is typically its 'up' vector (y-axis).
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        // This loop will execute over multiple frames, smoothly rotating the object
        while (elapsedTime < pauseDuration)
        {
            // Interpolate from the start rotation to the target rotation
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / pauseDuration);

            // Increment the timer
            elapsedTime += Time.deltaTime;

            // Wait for the next frame before continuing the loop
            yield return null;
        }

        // Ensure the rotation is exactly the target rotation at the end
        transform.rotation = targetRotation;

        // 3. Resume Movement
        isMoving = true;
    }

    void PickInitialDirection()
    {
        // Pick a random starting direction
        float angle = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0, 0, angle);
        isMoving = true;
    }
}