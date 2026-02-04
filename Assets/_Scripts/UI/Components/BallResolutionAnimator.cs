using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Animates a bouncing ball inside a pentagon shape for mission resolution
    /// Similar to Dispatch game - ball bounces around and lands to determine success/failure
    /// </summary>
    public class BallResolutionAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform ballTransform;
        [SerializeField] private Image ballImage;
        [SerializeField] private RectTransform pentagonBounds; // The pentagon area

        [Header("Ball Settings")]
        [SerializeField] private float ballRadius = 10f;
        [SerializeField] private Color ballColor = Color.yellow;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color failureColor = Color.red;

        [Header("Animation Settings")]
        [SerializeField] private float initialSpeed = 400f;
        [SerializeField] private float friction = 0.98f; // Velocity multiplier per frame
        [SerializeField] private float minSpeed = 20f; // Speed at which we start guiding to target
        [SerializeField] private float bounceRandomness = 0.2f; // How much randomness in bounce direction
        [SerializeField] private float maxAnimationTime = 5f; // Maximum time before forcing end

        [Header("Pentagon Settings")]
        [SerializeField] private float pentagonRadius = 100f; // Radius of the pentagon bounds

        // Animation state
        private Vector2 ballPosition;
        private Vector2 ballVelocity;
        private Vector2 targetPosition;
        private bool isAnimating = false;
        private bool isSuccess = false;
        private float animationTimer = 0f;

        // Pentagon vertices (normalized, will be scaled by radius)
        private Vector2[] pentagonVertices;

        // Events
        public System.Action<bool> OnAnimationComplete; // passes isSuccess

        private void Awake()
        {
            // Calculate pentagon vertices (starting from top, going clockwise)
            // Might (top), Charm (top-right), Fortitude (bottom-right), Agility (bottom-left), Wit (top-left)
            pentagonVertices = new Vector2[5];
            for (int i = 0; i < 5; i++)
            {
                float angle = (90f - i * 72f) * Mathf.Deg2Rad; // Start at top, go clockwise
                pentagonVertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            if (ballImage != null)
            {
                ballImage.color = ballColor;
            }
        }

        /// <summary>
        /// Start the ball resolution animation
        /// </summary>
        /// <param name="targetPos">Normalized position where ball should land (-1 to 1 range)</param>
        /// <param name="success">Whether this is a success or failure</param>
        /// <param name="pentRadius">Radius of the pentagon in pixels</param>
        public void StartAnimation(Vector2 targetPos, bool success, float pentRadius = 100f)
        {
            if (isAnimating) return;

            pentagonRadius = pentRadius;
            targetPosition = targetPos * pentagonRadius;
            isSuccess = success;
            isAnimating = true;
            animationTimer = 0f;

            // Start ball at random position inside pentagon
            ballPosition = GetRandomPositionInPentagon() * 0.5f; // Start closer to center

            // Random initial velocity
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            ballVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * initialSpeed;

            // Show the ball
            if (ballTransform != null)
            {
                ballTransform.gameObject.SetActive(true);
                ballTransform.anchoredPosition = ballPosition;
            }

            if (ballImage != null)
            {
                ballImage.color = ballColor;
            }

            StartCoroutine(AnimateBall());
        }

        /// <summary>
        /// Main animation coroutine
        /// </summary>
        private IEnumerator AnimateBall()
        {
            while (isAnimating)
            {
                animationTimer += Time.deltaTime;

                // Update ball position
                ballPosition += ballVelocity * Time.deltaTime;

                // Check for collision with pentagon edges
                CheckPentagonCollision();

                // Apply friction
                ballVelocity *= friction;

                // Check if we should start guiding to target
                float speed = ballVelocity.magnitude;
                if (speed < minSpeed || animationTimer > maxAnimationTime * 0.7f)
                {
                    // Start guiding towards target
                    Vector2 toTarget = targetPosition - ballPosition;
                    float distanceToTarget = toTarget.magnitude;

                    if (distanceToTarget < 5f || animationTimer > maxAnimationTime)
                    {
                        // Close enough or time's up - snap to target and end
                        ballPosition = targetPosition;
                        isAnimating = false;
                    }
                    else
                    {
                        // Guide towards target
                        ballVelocity = Vector2.Lerp(ballVelocity, toTarget.normalized * minSpeed * 0.5f, Time.deltaTime * 3f);
                    }
                }

                // Update visual position
                if (ballTransform != null)
                {
                    ballTransform.anchoredPosition = ballPosition;
                }

                yield return null;
            }

            // Animation complete - show result
            yield return StartCoroutine(ShowResult());
        }

        /// <summary>
        /// Check if ball hit pentagon edge and bounce
        /// </summary>
        private void CheckPentagonCollision()
        {
            // Check each edge of the pentagon
            for (int i = 0; i < 5; i++)
            {
                Vector2 v1 = pentagonVertices[i] * pentagonRadius;
                Vector2 v2 = pentagonVertices[(i + 1) % 5] * pentagonRadius;

                // Check if ball is outside this edge
                Vector2 edge = v2 - v1;
                Vector2 edgeNormal = new Vector2(-edge.y, edge.x).normalized;

                // Distance from ball to edge line
                Vector2 toPoint = ballPosition - v1;
                float distance = Vector2.Dot(toPoint, edgeNormal);

                // If ball is past the edge (outside pentagon)
                if (distance > 0)
                {
                    // Push ball back inside
                    ballPosition -= edgeNormal * (distance + ballRadius * 0.1f);

                    // Reflect velocity
                    ballVelocity = Vector2.Reflect(ballVelocity, -edgeNormal);

                    // Add some randomness to the bounce
                    float randomAngle = Random.Range(-bounceRandomness, bounceRandomness) * 90f * Mathf.Deg2Rad;
                    ballVelocity = RotateVector(ballVelocity, randomAngle);

                    // Lose some energy on bounce
                    ballVelocity *= 0.9f;
                }
            }
        }

        /// <summary>
        /// Show the final result with color change
        /// </summary>
        private IEnumerator ShowResult()
        {
            // Flash the ball color based on result
            Color targetColor = isSuccess ? successColor : failureColor;

            if (ballImage != null)
            {
                // Pulse effect
                for (int i = 0; i < 3; i++)
                {
                    ballImage.color = targetColor;
                    yield return new WaitForSeconds(0.15f);
                    ballImage.color = ballColor;
                    yield return new WaitForSeconds(0.1f);
                }
                ballImage.color = targetColor;
            }

            yield return new WaitForSeconds(0.5f);

            // Notify listeners
            OnAnimationComplete?.Invoke(isSuccess);
        }

        /// <summary>
        /// Get a random position inside the pentagon
        /// </summary>
        private Vector2 GetRandomPositionInPentagon()
        {
            // Simple approach: random point in circle, scaled down to fit in pentagon
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(0.2f, 0.7f) * pentagonRadius;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        /// <summary>
        /// Rotate a vector by an angle in radians
        /// </summary>
        private Vector2 RotateVector(Vector2 v, float angle)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        /// <summary>
        /// Stop animation immediately
        /// </summary>
        public void StopAnimation()
        {
            isAnimating = false;
            StopAllCoroutines();
        }

        /// <summary>
        /// Hide the ball
        /// </summary>
        public void HideBall()
        {
            if (ballTransform != null)
            {
                ballTransform.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Check if animation is currently playing
        /// </summary>
        public bool IsAnimating => isAnimating;
    }
}
