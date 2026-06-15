// Assets/_Project/UI/CrosshairUI.cs
using UnityEngine;
using UnityEngine.UIElements;
using VanguardProtocol.Combat;

namespace VanguardProtocol.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class CrosshairUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AimTargetDetector _aimDetector;
        [SerializeField] private Combat.PlayerShooter _shooter;

        [Header("Appearance")]
        [SerializeField] private float _lineLength = 8f;
        [SerializeField] private float _lineThickness = 2f;
        [SerializeField] private float _baseGap = 6f;

        [Header("Spread")]
        [SerializeField] private float _maxSpreadGap = 16f;
        [SerializeField] private float _spreadPerShot = 6f;
        [SerializeField] private float _spreadRecoverySpeed = 40f; // px/sec

        [Header("Colors")]
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _enemyColor = new Color(1f, 0.25f, 0.25f);

        private VisualElement _top, _bottom, _left, _right, _dot;
        private float _currentGap;

        private void Awake()
        {
            _currentGap = _baseGap;
            BuildCrosshair();
        }

        private void Start()
        {
            if (_shooter != null)
                _shooter.OnFired += HandleFired;
        }

        private void OnDestroy()
        {
            if (_shooter != null)
                _shooter.OnFired -= HandleFired;
        }

        private void Update()
        {
            _currentGap = Mathf.Max(_baseGap,
                _currentGap - _spreadRecoverySpeed * Time.deltaTime);

            UpdatePositions();
            UpdateColor();
        }

        private void BuildCrosshair()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            var container = new VisualElement();
            container.style.position = Position.Absolute;
            container.style.left = Length.Percent(50);
            container.style.top = Length.Percent(50);
            container.pickingMode = PickingMode.Ignore;

            _top = CreateBar();
            _bottom = CreateBar();
            _left = CreateBar();
            _right = CreateBar();
            _dot = CreateBar();

            container.Add(_top);
            container.Add(_bottom);
            container.Add(_left);
            container.Add(_right);
            container.Add(_dot);

            root.Add(container);
        }

        private VisualElement CreateBar()
        {
            var bar = new VisualElement();
            bar.style.position = Position.Absolute;
            bar.pickingMode = PickingMode.Ignore;
            return bar;
        }

        private void UpdatePositions()
        {
            float g = _currentGap;
            float l = _lineLength;
            float t = _lineThickness;

            _top.style.left = -t / 2f;
            _top.style.top = -(g + l);
            _top.style.width = t;
            _top.style.height = l;

            _bottom.style.left = -t / 2f;
            _bottom.style.top = g;
            _bottom.style.width = t;
            _bottom.style.height = l;

            _left.style.left = -(g + l);
            _left.style.top = -t / 2f;
            _left.style.width = l;
            _left.style.height = t;

            _right.style.left = g;
            _right.style.top = -t / 2f;
            _right.style.width = l;
            _right.style.height = t;

            _dot.style.left = -1f;
            _dot.style.top = -1f;
            _dot.style.width = 2f;
            _dot.style.height = 2f;
        }

        private void UpdateColor()
        {
            Color c = (_aimDetector != null && _aimDetector.IsAimingAtEnemy)
                ? _enemyColor
                : _defaultColor;

            _top.style.backgroundColor = c;
            _bottom.style.backgroundColor = c;
            _left.style.backgroundColor = c;
            _right.style.backgroundColor = c;
            _dot.style.backgroundColor = c;
        }

        private void HandleFired()
        {
            _currentGap = Mathf.Min(_maxSpreadGap, _currentGap + _spreadPerShot);
        }
    }
}