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

        [Header("Ball Settings")]
        [SerializeField] private float ballRadius = 10f;
        [SerializeField] private Color ballColor = Color.yellow;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color failureColor = Color.red;

        [Header("Animation Settings")]
        [SerializeField] private float initialSpeed = 300f;
        [SerializeField] private float friction = 0.985f;
        [SerializeField] private float minSpeed = 30f;
        [SerializeField] private float bounceRandomness = 0.15f;
        [SerializeField] private float maxAnimationTime = 4f;

        // Animation state
        private Vector2 ballPosition;
        private Vector2 ballVelocity;
        private Vector2 targetPosition;
        private bool isAnimating = false;
        private bool isSuccess = false;
        private float animationTimer = 0f;

        // Pentagon data
        private Vector2[] pentagonVertices = new Vector2[5];
        private float pentagonRadius;
        private const float MAX_STAT = 60f;

        // Events
        public System.Action<bool> OnAnimationComplete;

        private void Awake()
        {
            if (ballImage != null)
            {
                ballImage.color = ballColor;
            }
        }

        /// <summary>
        /// Start the ball resolution animation with mission requirements
        /// </summary>
        /// <param name="mightReq">Mission might requirement</param>
        /// <param name="charmReq">Mission charm requirement</param>
        /// <param name="witReq">Mission wit requirement</param>
        /// <param name="agilityReq">Mission agility requirement</param>
        /// <param name="fortitudeReq">Mission fortitude requirement</param>
        /// <param name="targetPos">Normalized position where ball should land</param>
        /// <param name="success">Whether this is a success or failure</param>
        /// <param name="pentRadius">Radius of the pentagon display in pixels</param>
        public void StartAnimation(int mightReq, int charmReq, int witReq, int agilityReq, int fortitudeReq,
                                   Vector2 targetPos, bool success, float pentRadius)
        {
            if (isAnimating) return;

            pentagonRadius = pentRadius;
            isSuccess = success;
            isAnimating = true;
            animationTimer = 0f;

            // Calculate pentagon vertices based on mission requirements
            // Using same coordinate system as PentagonStatDisplay
            // Order: Might, Wit, Agility, Fortitude, Charm (for angles 0,1,2,3,4)
            int[] statValues = { mightReq, witReq, agilityReq, fortitudeReq, charmReq };

            for (int i = 0; i < 5; i++)
            {
                float angle = (i * 72f + 90f) * Mathf.Deg2Rad;
                float statRatio = Mathf.Max(statValues[i], 5f) / MAX_STAT; // Min 5 to ensure some area
                float vertexRadius = pentagonRadius * statRatio;

                pentagonVertices[i] = new Vector2(
                    Mathf.Cos(angle) * vertexRadius,
                    Mathf.Sin(angle) * vertexRadius
                );
            }

            // Calculate target position (scale by pentagon radius)
            targetPosition = targetPos * pentagonRadius * 0.5f; // Scale down since targetPos is normalized

            // Clamp target to be inside pentagon
            targetPosition = ClampToPentagon(targetPosition);

            // Start ball at center
            ballPosition = Vector2.zero;

            // Random initial velocity
            float startAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            ballVelocity = new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle)) * initialSpeed;

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

            Debug.Log($"[BallAnimator] Starting animation. Target: {targetPosition}, Success: {success}");
            StartCoroutine(AnimateBall());
        }

        /// <summary>
        /// Legacy method - uses default pentagon shape
        /// </summary>
        public void StartAnimation(Vector2 targetPos, bool success, float pentRadius = 100f)
        {
            // Use default values if called without mission requirements
            StartAnimation(30, 30, 30, 30, 30, targetPos, success, pentRadius);
        }

        private IEnumerator AnimateBall()
        {
            while (isAnimating)
            {
                animationTimer += Time.deltaTime;

                // Update ball position
                Vector2 newPosition = ballPosition + ballVelocity * Time.deltaTime;

                // Check for collision with pentagon edges
                bool bounced = CheckAndHandleCollision(ref newPosition, ref ballVelocity);

                ballPosition = newPosition;

                // Apply friction
                ballVelocity *= friction;

                // Check if we should start guiding to target
                float speed = ballVelocity.magnitude;
                float timeRatio = animationTimer / maxAnimationTime;

                if (speed < minSpeed || timeRatio > 0.7f)
                {
                    // Start guiding towards target
                    Vector2 toTarget = targetPosition - ballPosition;
                    float distanceToTarget = toTarget.magnitude;

                    if (distanceToTarget < 3f || animationTimer > maxAnimationTime)
                    {
                        // Close enough or time's up - snap to target and end
                        ballPosition = targetPosition;
                        isAnimating = false;
                        Debug.Log($"[BallAnimator] Animation ended. Final position: {ballPosition}");
                    }
                    else
                    {
                        // Smoothly guide towards target
                        float guideStrength = Mathf.Lerp(2f, 8f, timeRatio);
                        ballVelocity = Vector2.Lerp(ballVelocity, toTarget.normalized * minSpeed, Time.deltaTime * guideStrength);
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
        /// Check collision with pentagon edges and handle bounce
        /// </summary>
        private bool CheckAndHandleCollision(ref Vector2 position, ref Vector2 velocity)
        {
            bool bounced = false;

            for (int i = 0; i < 5; i++)
            {
                Vector2 v1 = pentagonVertices[i];
                Vector2 v2 = pentagonVertices[(i + 1) % 5];

                // Calculate edge and its inward-pointing normal
                Vector2 edge = v2 - v1;
                // Normal pointing inward (toward center)
                Vector2 edgeNormal = new Vector2(edge.y, -edge.x).normalized;

                // Check which side of the edge we're on
                // For a pentagon drawn clockwise from center, the normal should point inward
                // We need to verify this by checking against center (0,0)
                Vector2 edgeCenter = (v1 + v2) * 0.5f;
                if (Vector2.Dot(edgeNormal, -edgeCenter) < 0)
                {
                    edgeNormal = -edgeNormal; // Flip if pointing outward
                }

                // Distance from point to edge (positive = inside, negative = outside)
                Vector2 toPoint = position - v1;
                float signedDistance = Vector2.Dot(toPoint, edgeNormal);

                // If outside the edge (or very close to it)
                if (signedDistance < ballRadius)
                {
                    // Push back inside
                    position += edgeNormal * (ballRadius - signedDistance + 1f);

                    // Only bounce if moving toward the edge
                    if (Vector2.Dot(velocity, edgeNormal) < 0)
                    {
                        // Reflect velocity off the edge
                        velocity = Vector2.Reflect(velocity, edgeNormal);

                        // Add randomness
                        float randomAngle = Random.Range(-bounceRandomness, bounceRandomness) * Mathf.PI;
                        velocity = RotateVector(velocity, randomAngle);

                        // Energy loss
                        velocity *= 0.85f;

                        bounced = true;
                        Debug.Log($"[BallAnimator] Bounce! Edge {i}, New velocity: {velocity.magnitude}");
                    }
                }
            }

            return bounced;
        }

        /// <summary>
        /// Clamp a position to be inside the pentagon
        /// </summary>
        private Vector2 ClampToPentagon(Vector2 pos)
        {
            // Simple approach: if outside, move toward center until inside
            for (int attempt = 0; attempt < 10; attempt++)
            {
                bool inside = true;

                for (int i = 0; i < 5; i++)
                {
                    Vector2 v1 = pentagonVertices[i];
                    Vector2 v2 = pentagonVertices[(i + 1) % 5];

                    Vector2 edge = v2 - v1;
                    Vector2 edgeNormal = new Vector2(edge.y, -edge.x).normalized;

                    Vector2 edgeCenter = (v1 + v2) * 0.5f;
                    if (Vector2.Dot(edgeNormal, -edgeCenter) < 0)
                    {
                        edgeNormal = -edgeNormal;
                    }

                    Vector2 toPoint = pos - v1;
                    float signedDistance = Vector2.Dot(toPoint, edgeNormal);

                    if (signedDistance < 0)
                    {
                        inside = false;
                        pos += edgeNormal * (-signedDistance + 5f);
                        break;
                    }
                }

                if (inside) break;
            }

            return pos;
        }

        private IEnumerator ShowResult()
        {
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

            OnAnimationComplete?.Invoke(isSuccess);
        }

        private Vector2 RotateVector(Vector2 v, float angle)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        public void StopAnimation()
        {
            isAnimating = false;
            StopAllCoroutines();
        }

        public void HideBall()
        {
            if (ballTransform != null)
            {
                ballTransform.gameObject.SetActive(false);
            }
        }

        public bool IsAnimating => isAnimating;
    }
}
