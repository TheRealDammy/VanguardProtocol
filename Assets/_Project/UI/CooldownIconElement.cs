using UnityEngine;
using UnityEngine.UIElements;

namespace VanguardProtocol.UI
{
    public class CooldownIconElement : VisualElement
    {
        private float _fraction; // 1 = full overlay (just used), 0 = ready

        public float Fraction
        {
            get => _fraction;
            set
            {
                float clamped = Mathf.Clamp01(value);
                if (Mathf.Approximately(clamped, _fraction)) return;
                _fraction = clamped;
                MarkDirtyRepaint();
            }
        }

        public CooldownIconElement()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (_fraction <= 0f) return;

            var rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            var painter = mgc.painter2D;
            Vector2 center = new Vector2(rect.width / 2f, rect.height / 2f);
            float radius = Mathf.Max(rect.width, rect.height);

            float startAngle = -90f; // 12 o'clock
            float endAngle = startAngle + 360f * _fraction;

            painter.fillColor = new Color(0f, 0f, 0f, 0.65f);
            painter.BeginPath();
            painter.MoveTo(center);
            painter.LineTo(center + AngleToDir(startAngle) * radius);
            painter.Arc(center, radius, Angle.Degrees(startAngle), Angle.Degrees(endAngle), ArcDirection.Clockwise);
            painter.LineTo(center);
            painter.ClosePath();
            painter.Fill();
        }

        private static Vector2 AngleToDir(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}