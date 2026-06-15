using UnityEngine;
using UnityEngine.UIElements;
using VanguardProtocol.Characters;

namespace VanguardProtocol.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private CharacterBase _player;

        [Header("Low HP Vignette")]
        [SerializeField] private float _lowHPThreshold = 0.25f;
        [SerializeField] private float _pulseSpeed = 3f;

        private HealthBarElement _healthBar;
        private Label _healthLabel;
        private VisualElement _vignette;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // Vignette — index 0 so it renders behind everything else
            _vignette = new VisualElement();
            _vignette.style.position = Position.Absolute;
            _vignette.style.left = 0; _vignette.style.top = 0;
            _vignette.style.right = 0; _vignette.style.bottom = 0;
            _vignette.pickingMode = PickingMode.Ignore;
            _vignette.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            root.Insert(0, _vignette);

            // Player health bar — bottom left
            var container = new VisualElement();
            container.style.position = Position.Absolute;
            container.style.left = 30f;
            container.style.bottom = 30f;
            container.style.width = 220f;
            container.pickingMode = PickingMode.Ignore;

            var nameLabel = new Label("YOU");
            nameLabel.style.color = Color.white;
            nameLabel.style.fontSize = 12f;
            nameLabel.style.marginBottom = 2f;
            container.Add(nameLabel);

            _healthBar = new HealthBarElement();
            _healthBar.style.height = 18f;
            _healthBar.style.width = Length.Percent(100);
            container.Add(_healthBar);

            _healthLabel = new Label("");
            _healthLabel.style.position = Position.Absolute;
            _healthLabel.style.left = 0; _healthLabel.style.right = 0;
            _healthLabel.style.top = 0; _healthLabel.style.bottom = 0;
            _healthLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _healthLabel.style.fontSize = 12f;
            _healthLabel.style.color = Color.white;
            _healthBar.Add(_healthLabel);

            root.Add(container);
        }

        private void Start()
        {
            if (_player == null) return;
            _player.OnHealthChanged += (_, _) => Refresh();
            Refresh();
        }

        private void Update()
        {
            if (_player == null) return;

            float frac = GetFraction();

            if (frac < _lowHPThreshold && _player.IsAlive)
            {
                float pulse = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
                float severity = Mathf.InverseLerp(_lowHPThreshold, 0f, frac);
                float alpha = Mathf.Lerp(0.15f, 0.5f, severity) * (0.6f + 0.4f * pulse);
                _vignette.style.backgroundColor = new Color(0.5f, 0f, 0f, alpha);
            }
            else
            {
                _vignette.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            }
        }

        private float GetFraction()
        {
            var health = _player.Attributes.health;
            var maxHealth = _player.Attributes.maxHealth;
            return maxHealth.CurrentValue > 0 ? health.CurrentValue / maxHealth.CurrentValue : 0f;
        }

        private void Refresh()
        {
            float frac = GetFraction();
            _healthBar.SetFraction(frac);

            var health = _player.Attributes.health;
            var maxHealth = _player.Attributes.maxHealth;
            _healthLabel.text = $"{Mathf.CeilToInt(health.CurrentValue)} / {Mathf.CeilToInt(maxHealth.CurrentValue)}";
        }
    }
}