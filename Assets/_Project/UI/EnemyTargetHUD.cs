using UnityEngine;
using UnityEngine.UIElements;
using VanguardProtocol.Characters;
using VanguardProtocol.Combat;

namespace VanguardProtocol.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class EnemyTargetHUD : MonoBehaviour
    {
        [SerializeField] private AimTargetDetector _aimDetector;

        [Header("Layout")]
        [SerializeField] private float _width = 160f;
        [SerializeField] private float _height = 10f;
        [SerializeField] private float _verticalOffset = 40f;

        private VisualElement _container;
        private HealthBarElement _healthBar;
        private Label _nameLabel;
        private CharacterBase _trackedTarget;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _container = new VisualElement();
            _container.style.position = Position.Absolute;
            _container.style.left = Length.Percent(50);
            _container.style.top = Length.Percent(50);
            _container.style.width = _width;
            _container.style.marginLeft = -_width / 2f;
            _container.style.marginTop = -(_verticalOffset + _height + 16f);
            _container.pickingMode = PickingMode.Ignore;
            _container.style.opacity = 0f;

            _nameLabel = new Label("");
            _nameLabel.style.color = Color.white;
            _nameLabel.style.fontSize = 11f;
            _nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _nameLabel.style.marginBottom = 2f;
            _container.Add(_nameLabel);

            _healthBar = new HealthBarElement();
            _healthBar.style.height = _height;
            _container.Add(_healthBar);

            root.Add(_container);
        }

        private void Update()
        {
            if (_aimDetector == null) return;

            bool show = _aimDetector.IsAimingAtEnemy && _aimDetector.CurrentTarget != null;

            _container.style.opacity = Mathf.MoveTowards(
                _container.resolvedStyle.opacity, show ? 1f : 0f, Time.deltaTime * 6f);

            if (!show) return;

            var target = _aimDetector.CurrentTarget;

            if (target != _trackedTarget)
            {
                _trackedTarget = target;
                _nameLabel.text = target.gameObject.name;
                _healthBar.SetFractionInstant(GetFraction(target));
            }
            else
            {
                _healthBar.SetFraction(GetFraction(target));
            }
        }

        private float GetFraction(CharacterBase c)
        {
            var health = c.Attributes.health;
            var maxHealth = c.Attributes.maxHealth;
            return maxHealth.CurrentValue > 0 ? health.CurrentValue / maxHealth.CurrentValue : 0f;
        }
    }
}