using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Pentagon visualization for 5-stat system
    /// Draws a pentagon showing Might, Charm, Wit, Agility, Fortitude
    /// Can overlay requirement pentagon with hero stat pentagon
    /// </summary>
    public class PentagonStatDisplay : Graphic
    {
        [Header("Pentagon Configuration")]
        [Tooltip("Radius of the pentagon (how big it is)")]
        [Range(50f, 300f)]
        public float radius = 150f;

        [Tooltip("Line thickness for pentagon outline")]
        [Range(1f, 10f)]
        public float lineThickness = 3f;

        [Header("Stat Values (0-60 range)")]
        [Tooltip("Might stat value")]
        [Range(0, 60)]
        public int might = 30;

        [Tooltip("Charm stat value")]
        [Range(0, 60)]
        public int charm = 30;

        [Tooltip("Wit stat value")]
        [Range(0, 60)]
        public int wit = 30;

        [Tooltip("Agility stat value")]
        [Range(0, 60)]
        public int agility = 30;

        [Tooltip("Fortitude stat value")]
        [Range(0, 60)]
        public int fortitude = 30;

        [Header("Fill Settings")]
        [Tooltip("Whether to fill the pentagon")]
        public bool fillPentagon = true;

        [Tooltip("Fill color")]
        public Color fillColor = new Color(0.2f, 0.6f, 1f, 0.3f);

        [Tooltip("Outline color")]
        public Color outlineColor = new Color(0.2f, 0.6f, 1f, 1f);

        [Header("Base Pentagon (Background)")]
        [Tooltip("Show base pentagon outline at maximum stat range")]
        public bool showBasePentagon = true;

        [Tooltip("Base pentagon outline color")]
        public Color basePentagonColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Tooltip("Base pentagon line thickness")]
        [Range(1f, 10f)]
        public float basePentagonThickness = 2f;

        [Header("Overlay (Optional)")]
        [Tooltip("Show overlay pentagon (for comparing requirements vs hero stats)")]
        public bool showOverlay = false;

        [Tooltip("Overlay stat values")]
        [Range(0, 60)]
        public int overlayMight = 20;
        [Range(0, 60)]
        public int overlayCharm = 20;
        [Range(0, 60)]
        public int overlayWit = 20;
        [Range(0, 60)]
        public int overlayAgility = 20;
        [Range(0, 60)]
        public int overlayFortitude = 20;

        [Tooltip("Overlay fill color")]
        public Color overlayFillColor = new Color(1f, 0.5f, 0.2f, 0.3f);

        [Tooltip("Overlay outline color")]
        public Color overlayOutlineColor = new Color(1f, 0.5f, 0.2f, 1f);

        private const float MAX_STAT = 60f; // Maximum stat value for scaling

        /// <summary>
        /// Update stat values programmatically
        /// </summary>
        public void SetStats(int m, int c, int w, int a, int f)
        {
            might = Mathf.Clamp(m, 0, 60);
            charm = Mathf.Clamp(c, 0, 60);
            wit = Mathf.Clamp(w, 0, 60);
            agility = Mathf.Clamp(a, 0, 60);
            fortitude = Mathf.Clamp(f, 0, 60);
            SetVerticesDirty();
        }

        /// <summary>
        /// Update overlay stat values programmatically
        /// </summary>
        public void SetOverlayStats(int m, int c, int w, int a, int f)
        {
            overlayMight = Mathf.Clamp(m, 0, 60);
            overlayCharm = Mathf.Clamp(c, 0, 60);
            overlayWit = Mathf.Clamp(w, 0, 60);
            overlayAgility = Mathf.Clamp(a, 0, 60);
            overlayFortitude = Mathf.Clamp(f, 0, 60);
            showOverlay = true;
            SetVerticesDirty();
        }

        /// <summary>
        /// Clear overlay
        /// </summary>
        public void ClearOverlay()
        {
            showOverlay = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            // Draw base pentagon background (max stat outline)
            if (showBasePentagon)
            {
                DrawBasePentagon(vh);
            }

            // Draw main pentagon (requirements or main stats)
            DrawPentagon(vh, might, charm, wit, agility, fortitude, fillColor, outlineColor);

            // Draw overlay pentagon if enabled (hero stats)
            if (showOverlay)
            {
                DrawPentagon(vh, overlayMight, overlayCharm, overlayWit, overlayAgility, overlayFortitude,
                           overlayFillColor, overlayOutlineColor);
            }
        }

        /// <summary>
        /// Draw base pentagon background at maximum size
        /// </summary>
        private void DrawBasePentagon(VertexHelper vh)
        {
            Vector2[] points = new Vector2[5];

            // Draw pentagon at maximum size (60 for all stats)
            for (int i = 0; i < 5; i++)
            {
                float angle = (i * 72f + 90f) * Mathf.Deg2Rad; // Start at top (90°), rotate clockwise

                points[i] = new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
            }

            // Draw outline only (no fill for base)
            DrawPentagonOutline(vh, points, basePentagonColor, basePentagonThickness);
        }

        /// <summary>
        /// Draw a pentagon with given stat values
        /// </summary>
        private void DrawPentagon(VertexHelper vh, int m, int c, int w, int a, int f, Color fill, Color outline)
        {
            // Calculate the 5 corner positions
            Vector2[] points = new Vector2[5];
            float[] statValues = { m, w, a, f, c }; // Reordered for visual balance

            // Pentagon points (starting from top, going clockwise)
            // Top: Might (90°)
            // Top-Right: Wit (162°)
            // Bottom-Right: Agility (234°)
            // Bottom-Left: Fortitude (306°)
            // Top-Left: Charm (18°)

            for (int i = 0; i < 5; i++)
            {
                float angle = (i * 72f + 90f) * Mathf.Deg2Rad; // Start at top (90°), rotate clockwise
                float statRatio = statValues[i] / MAX_STAT;
                float currentRadius = radius * statRatio;

                points[i] = new Vector2(
                    Mathf.Cos(angle) * currentRadius,
                    Mathf.Sin(angle) * currentRadius
                );
            }

            // Draw fill if enabled
            if (fillPentagon)
            {
                DrawFilledPentagon(vh, points, fill);
            }

            // Draw outline
            DrawPentagonOutline(vh, points, outline, lineThickness);
        }

        /// <summary>
        /// Draw filled pentagon using triangulation
        /// </summary>
        private void DrawFilledPentagon(VertexHelper vh, Vector2[] points, Color fillColor)
        {
            int startIndex = vh.currentVertCount;

            // Add center vertex
            UIVertex center = UIVertex.simpleVert;
            center.position = Vector3.zero;
            center.color = fillColor;
            vh.AddVert(center);

            // Add corner vertices
            foreach (var point in points)
            {
                UIVertex vert = UIVertex.simpleVert;
                vert.position = new Vector3(point.x, point.y, 0);
                vert.color = fillColor;
                vh.AddVert(vert);
            }

            // Create triangles (fan from center)
            for (int i = 0; i < 5; i++)
            {
                int next = (i + 1) % 5;
                vh.AddTriangle(startIndex, startIndex + i + 1, startIndex + next + 1);
            }
        }

        /// <summary>
        /// Draw pentagon outline using thick lines
        /// </summary>
        private void DrawPentagonOutline(VertexHelper vh, Vector2[] points, Color outlineColor, float thickness)
        {
            for (int i = 0; i < 5; i++)
            {
                int next = (i + 1) % 5;
                DrawThickLine(vh, points[i], points[next], thickness, outlineColor);
            }
        }

        /// <summary>
        /// Draw a thick line between two points
        /// </summary>
        private void DrawThickLine(VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color color)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);

            int startIndex = vh.currentVertCount;

            // Create 4 vertices for the line quad
            UIVertex v1 = UIVertex.simpleVert;
            v1.position = new Vector3(start.x + perpendicular.x, start.y + perpendicular.y, 0);
            v1.color = color;
            vh.AddVert(v1);

            UIVertex v2 = UIVertex.simpleVert;
            v2.position = new Vector3(start.x - perpendicular.x, start.y - perpendicular.y, 0);
            v2.color = color;
            vh.AddVert(v2);

            UIVertex v3 = UIVertex.simpleVert;
            v3.position = new Vector3(end.x - perpendicular.x, end.y - perpendicular.y, 0);
            v3.color = color;
            vh.AddVert(v3);

            UIVertex v4 = UIVertex.simpleVert;
            v4.position = new Vector3(end.x + perpendicular.x, end.y + perpendicular.y, 0);
            v4.color = color;
            vh.AddVert(v4);

            // Create triangles for the quad
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif
    }
}
