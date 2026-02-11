using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OneShotSupport.UI.Components
{
    public class BallResolutionAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform ballTransform;
        [SerializeField] private Image ballImage;

        [Header("Visual Settings")]
        [SerializeField] private float ballRadius = 10f; 
        [SerializeField] private Color ballColor = Color.yellow;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color failureColor = Color.red;

        [Header("Physics Settings")]
        [SerializeField] private float initialSpeed = 500f;  
        [SerializeField] private float minBounces = 3f;      
        [SerializeField] private float maxTime = 6f;         
        [SerializeField] private float stopSpeedThreshold = 10f;

        [Header("Stat Config")]
        // FIX: Default to 60 to match PentagonStatDisplay constant!
        [SerializeField] private float maxStatValue = 60f; 

        // Physics State
        private Vector2 ballPosition;
        private Vector2 ballVelocity;
        private bool isAnimating = false;
        private float currentTime;

        // Polygon Data
        private Vector2[] missionVertices = new Vector2[5]; // The Walls
        private Vector2[] heroVertices = new Vector2[5];    // The Goal
        private float pentagonRadius;

        public System.Action<bool> OnAnimationComplete;

        private void Awake()
        {
            if (ballImage != null) ballImage.color = ballColor;
            if (ballTransform != null) ballTransform.gameObject.SetActive(false);
        }

        public void StartPhysicsSimulation(
            int mightReq, int charmReq, int witReq, int agilityReq, int fortitudeReq, 
            int heroMight, int heroCharm, int heroWit, int heroAgility, int heroFortitude, 
            float pentRadius)
        {
            if (isAnimating) return;

            pentagonRadius = pentRadius;
            isAnimating = true;
            currentTime = 0f;

            // 1. Generate Walls (Mission)
            // CRITICAL: Order must match PentagonStatDisplay: Might, Wit, Agility, Fortitude, Charm
            int[] missionStats = { mightReq, witReq, agilityReq, fortitudeReq, charmReq };
            GeneratePolygon(missionStats, ref missionVertices);

            // 2. Generate Win Zone (Hero)
            // CRITICAL: Same order
            int[] heroStats = { heroMight, heroWit, heroAgility, heroFortitude, heroCharm };
            GeneratePolygon(heroStats, ref heroVertices);

            // 3. Reset Ball
            ballPosition = Vector2.zero;
            
            // 4. Launch
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            ballVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * initialSpeed;

            // 5. Show
            if (ballTransform != null)
            {
                ballTransform.gameObject.SetActive(true);
                ballTransform.anchoredPosition = ballPosition;
                // Force size to match logic
                ballTransform.sizeDelta = new Vector2(ballRadius * 2, ballRadius * 2);
            }
            if (ballImage != null) ballImage.color = ballColor;

            StartCoroutine(PhysicsLoop());
        }

        private void GeneratePolygon(int[] stats, ref Vector2[] vertices)
        {
            for (int i = 0; i < 5; i++)
            {
                // Angle logic must match PentagonStatDisplay.cs exactly
                // (i * 72 + 90) starts at Top (90) and rotates CCW
                float angle = (i * 72f + 90f) * Mathf.Deg2Rad;
                
                // Scale based on visual max (60)
                float statRatio = Mathf.Max(stats[i], 5f) / maxStatValue;
                float vertexRadius = pentagonRadius * statRatio;

                vertices[i] = new Vector2(
                    Mathf.Cos(angle) * vertexRadius,
                    Mathf.Sin(angle) * vertexRadius
                );
            }
        }

        private IEnumerator PhysicsLoop()
        {
            while (isAnimating)
            {
                float dt = Time.deltaTime;
                currentTime += dt;

                // Move
                Vector2 nextPosition = ballPosition + ballVelocity * dt;

                // Collide with Walls (Mission)
                CheckWallCollision(ref nextPosition, ref ballVelocity);
                
                ballPosition = nextPosition;

                // Friction: slippery at first, slows down later
                float currentFriction = (currentTime < 2.5f) ? 0.999f : 0.985f;
                ballVelocity *= currentFriction;

                // Stop condition
                if (currentTime > maxTime || (ballVelocity.magnitude < stopSpeedThreshold && currentTime > 2.5f))
                {
                    isAnimating = false;
                }

                if (ballTransform != null) ballTransform.anchoredPosition = ballPosition;

                yield return null;
            }

            // Resolution
            bool success = CheckSuccess(ballPosition);
            
            Debug.Log($"[Resolution] Final Pos: {ballPosition}. Success: {success}");
            StartCoroutine(ShowResultSequence(success));
        }

        private void CheckWallCollision(ref Vector2 pos, ref Vector2 vel)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 v1 = missionVertices[i];
                Vector2 v2 = missionVertices[(i + 1) % 5];

                Vector2 edge = v2 - v1;
                // Normal is perpendicular to edge
                Vector2 normal = new Vector2(edge.y, -edge.x).normalized;

                // Ensure normal points inward
                Vector2 edgeCenter = (v1 + v2) * 0.5f;
                if (Vector2.Dot(normal, -edgeCenter) < 0) normal = -normal;

                Vector2 toPoint = pos - v1;
                float dist = Vector2.Dot(toPoint, normal);

                // If overlapping wall
                if (dist < ballRadius)
                {
                    pos += normal * (ballRadius - dist);

                    // Bounce
                    if (Vector2.Dot(vel, normal) < 0)
                    {
                        vel = Vector2.Reflect(vel, normal);
                        vel = RotateVector(vel, Random.Range(-0.1f, 0.1f));
                        vel *= 0.95f; 
                    }
                }
            }
        }

        private bool CheckSuccess(Vector2 p)
        {
            // Strict Check: Only succeed if ball center is inside the overlap polygon
            // This prevents false positives where ball is clearly outside but close to edge
            return IsPointInPolygon(p, heroVertices);
        }

        private bool IsPointInPolygon(Vector2 p, Vector2[] poly)
        {
            int j = poly.Length - 1;
            bool inside = false;
            for (int i = 0; i < poly.Length; j = i++)
            {
                if (((poly[i].y <= p.y && p.y < poly[j].y) || (poly[j].y <= p.y && p.y < poly[i].y)) &&
                    (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private bool IsTouchingPolygon(Vector2 p, Vector2[] poly, float margin)
        {
            // Check distance to each edge
            for (int i = 0; i < poly.Length; i++)
            {
                Vector2 v1 = poly[i];
                Vector2 v2 = poly[(i + 1) % poly.Length];
                
                // If distance from center to line is less than radius, we are touching
                float dist = HandleUtility_DistancePointToLineSegment(p, v1, v2);
                if (dist <= margin) return true;
            }
            return false;
        }

        float HandleUtility_DistancePointToLineSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        private IEnumerator ShowResultSequence(bool success)
        {
            Color targetColor = success ? successColor : failureColor;
            if (ballImage != null)
            {
                ballImage.color = targetColor;
                yield return new WaitForSeconds(0.1f);
                ballImage.color = ballColor;
                yield return new WaitForSeconds(0.1f);
                ballImage.color = targetColor;
            }
            yield return new WaitForSeconds(0.4f);
            OnAnimationComplete?.Invoke(success);
        }

        private Vector2 RotateVector(Vector2 v, float radians)
        {
            float c = Mathf.Cos(radians);
            float s = Mathf.Sin(radians);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
        
        public void StopAnimation()
        {
            isAnimating = false;
            StopAllCoroutines();
            if (ballTransform != null) ballTransform.gameObject.SetActive(false);
        }

        // --- DEBUG: VISUALIZE PHYSICS ---
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && isAnimating)
            {
                // Draw Mission Walls (RED)
                Gizmos.color = Color.red;
                DrawPolyGizmo(missionVertices);

                // Draw Win Zone (GREEN)
                Gizmos.color = Color.green;
                DrawPolyGizmo(heroVertices);

                // Draw Ball (YELLOW)
                Gizmos.color = Color.yellow;
                Vector2 bPos = TransformPoint(ballPosition);
                Gizmos.DrawWireSphere(bPos, ballRadius);
            }
        }

        private void DrawPolyGizmo(Vector2[] verts)
        {
            if (verts == null || verts.Length == 0) return;
            for (int i = 0; i < 5; i++)
            {
                Vector2 start = TransformPoint(verts[i]);
                Vector2 end = TransformPoint(verts[(i + 1) % 5]);
                Gizmos.DrawLine(start, end);
            }
        }

        private Vector2 TransformPoint(Vector2 localPos)
        {
            if (ballTransform != null && ballTransform.parent != null)
                return ballTransform.parent.TransformPoint(localPos);
            return localPos;
        }
    }
}