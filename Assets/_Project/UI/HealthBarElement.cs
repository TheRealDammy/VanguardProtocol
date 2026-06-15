using UnityEngine;
using UnityEngine.UIElements;

namespace VanguardProtocol.UI
{
    public class HealthBarElement : VisualElement
    {
        private readonly VisualElement _fill;
        private readonly VisualElement _damageTrail;

        private float _targetFraction = 1f;
        private float _displayFraction = 1f;
        private float _trailFraction = 1f;
        private bool _scheduled;

        public HealthBarElement()
        {
            style.position = Position.Relative;
            style.overflow = Overflow.Hidden;
            style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
            style.borderTopWidth = style.borderBottomWidth =
                style.borderLeftWidth = style.borderRightWidth = 1f;
            var borderColor = new Color(1f, 1f, 1f, 0.25f);
            style.borderTopColor = style.borderBottomColor =
                style.borderLeftColor = style.borderRightColor = borderColor;

            _damageTrail = new VisualElement();
            _damageTrail.style.position = Position.Absolute;
            _damageTrail.style.left = 0; _damageTrail.style.top = 0;
            _damageTrail.style.bottom = 0;
            _damageTrail.style.backgroundColor = new Color(0.9f, 0.85f, 0.2f, 0.6f);
            Add(_damageTrail);

            _fill = new VisualElement();
            _fill.style.position = Position.Absolute;
            _fill.style.left = 0; _fill.style.top = 0;
            _fill.style.bottom = 0;
            Add(_fill);

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                if (_scheduled) return;
                _scheduled = true;
                schedule.Execute(Tick).Every(16);
            });
        }

        public void SetFraction(float fraction) =>
            _targetFraction = Mathf.Clamp01(fraction);

        public void SetFractionInstant(float fraction)
        {
            _targetFraction = Mathf.Clamp01(fraction);
            _displayFraction = _targetFraction;
            _trailFraction = _targetFraction;
            Apply();
        }

        private void Tick()
        {
            _displayFraction = Mathf.MoveTowards(_displayFraction, _targetFraction, 0.05f);

            _trailFraction = _trailFraction > _displayFraction
                ? Mathf.MoveTowards(_trailFraction, _displayFraction, 0.01f)
                : _displayFraction;

            Apply();
        }

        private void Apply()
        {
            _fill.style.width = Length.Percent(_displayFraction * 100f);
            _damageTrail.style.width = Length.Percent(_trailFraction * 100f);
            _fill.style.backgroundColor = ColorForFraction(_displayFraction);
        }

        private static Color ColorForFraction(float frac)
        {
            if (frac > 0.5f) return new Color(0.25f, 0.85f, 0.3f);
            if (frac > 0.25f) return new Color(0.95f, 0.8f, 0.2f);
            return new Color(0.9f, 0.25f, 0.25f);
        }
    }
}