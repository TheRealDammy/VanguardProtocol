using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VanguardProtocol.Characters;
using VanguardProtocol.Systems;

namespace VanguardProtocol.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class TeamHUD : MonoBehaviour
    {
        [SerializeField] private CharacterBase _localPlayer; // excluded from roster
        [SerializeField] private Team _team = Team.Blue;

        [Header("Layout")]
        [SerializeField] private float _entryWidth = 180f;
        [SerializeField] private float _entryHeight = 36f;
        [SerializeField] private float _entrySpacing = 6f;

        [Header("Damage Flash")]
        [SerializeField] private float _heavyDamageThreshold = 0.15f;
        [SerializeField] private float _flashDuration = 0.3f;

        private VisualElement _container;
        private readonly Dictionary<CharacterBase, TeamEntry> _entries = new();

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _container = new VisualElement();
            _container.style.position = Position.Absolute;
            _container.style.left = 30f;
            _container.style.top = 30f;
            _container.pickingMode = PickingMode.Ignore;

            root.Add(_container);
        }

        private void Start()
        {
            if (TeamManager.Instance == null) return;

            TeamManager.Instance.OnCharacterRegistered += HandleRegistered;

            foreach (var c in TeamManager.Instance.GetTeamMembers(_team))
                AddEntry(c);
        }

        private void OnDestroy()
        {
            if (TeamManager.Instance != null)
                TeamManager.Instance.OnCharacterRegistered -= HandleRegistered;

            foreach (var entry in _entries.Values)
                entry.Unsubscribe();
        }

        private void HandleRegistered(CharacterBase character, Team team)
        {
            if (team == _team) AddEntry(character);
        }

        private void AddEntry(CharacterBase character)
        {
            if (character == _localPlayer) return;
            if (_entries.ContainsKey(character)) return;

            var entry = new TeamEntry(character, _entryWidth, _entryHeight,
                _flashDuration, _heavyDamageThreshold);

            _entries[character] = entry;
            _container.Add(entry.Root);

            var spacer = new VisualElement();
            spacer.style.height = _entrySpacing;
            _container.Add(spacer);
        }

        private class TeamEntry
        {
            public VisualElement Root;

            private readonly CharacterBase _character;
            private readonly HealthBarElement _healthBar;
            private readonly VisualElement _icon;
            private readonly float _flashDuration;
            private readonly float _heavyDamageThreshold;
            private float _flashTimer;

            private readonly Action<float, float> _onHealthChanged;
            private readonly Action<CharacterBase, float> _onDamageTaken;
            private readonly Action<CharacterBase> _onDeath;

            public TeamEntry(CharacterBase character, float width, float height,
                float flashDuration, float heavyDamageThreshold)
            {
                _character = character;
                _flashDuration = flashDuration;
                _heavyDamageThreshold = heavyDamageThreshold;

                Root = new VisualElement();
                Root.style.width = width;
                Root.style.height = height;
                Root.style.flexDirection = FlexDirection.Row;
                Root.pickingMode = PickingMode.Ignore;

                _icon = new VisualElement();
                _icon.style.width = height;
                _icon.style.height = height;
                _icon.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
                _icon.style.borderTopWidth = _icon.style.borderBottomWidth =
                    _icon.style.borderLeftWidth = _icon.style.borderRightWidth = 1f;
                var borderColor = new Color(1f, 1f, 1f, 0.3f);
                _icon.style.borderTopColor = _icon.style.borderBottomColor =
                    _icon.style.borderLeftColor = _icon.style.borderRightColor = borderColor;
                _icon.style.marginRight = 6f;

                string initialChar = character.gameObject.name.Length > 0
                    ? character.gameObject.name.Substring(0, 1) : "?";
                var initial = new Label(initialChar);
                initial.style.unityTextAlign = TextAnchor.MiddleCenter;
                initial.style.color = Color.white;
                initial.style.fontSize = 16f;
                initial.style.flexGrow = 1;
                _icon.Add(initial);
                Root.Add(_icon);

                var barContainer = new VisualElement();
                barContainer.style.flexGrow = 1;
                barContainer.style.justifyContent = Justify.Center;

                var nameLabel = new Label(character.gameObject.name);
                nameLabel.style.color = Color.white;
                nameLabel.style.fontSize = 11f;
                nameLabel.style.marginBottom = 2f;
                barContainer.Add(nameLabel);

                _healthBar = new HealthBarElement();
                _healthBar.style.height = 12f;
                barContainer.Add(_healthBar);
                Root.Add(barContainer);

                _onHealthChanged = (_, _) => Refresh();
                _onDamageTaken = HandleDamageTaken;
                _onDeath = _ => SetEliminated();

                character.OnHealthChanged += _onHealthChanged;
                character.OnDamageTaken += _onDamageTaken;
                character.OnDeath += _onDeath;

                Refresh();

                bool scheduled = false;
                Root.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    if (scheduled) return;
                    scheduled = true;
                    Root.schedule.Execute(Tick).Every(16);
                });
            }

            public void Unsubscribe()
            {
                _character.OnHealthChanged -= _onHealthChanged;
                _character.OnDamageTaken -= _onDamageTaken;
                _character.OnDeath -= _onDeath;
            }

            private void SetEliminated()
            {
                Root.style.opacity = 0.35f;
                _healthBar.SetFraction(0f);
            }

            private void HandleDamageTaken(CharacterBase source, float amount)
            {
                float maxHP = _character.Attributes.maxHealth.CurrentValue;
                if (maxHP <= 0f) return;

                if (amount / maxHP >= _heavyDamageThreshold)
                {
                    _flashTimer = _flashDuration;
                    _icon.style.backgroundColor = new Color(0.9f, 0.2f, 0.2f, 0.9f);
                }
            }

            private void Refresh()
            {
                var health = _character.Attributes.health;
                var maxHealth = _character.Attributes.maxHealth;
                float frac = maxHealth.CurrentValue > 0
                    ? health.CurrentValue / maxHealth.CurrentValue : 0f;
                _healthBar.SetFraction(frac);
            }

            private void Tick()
            {
                if (_flashTimer <= 0f) return;

                _flashTimer -= 0.016f;
                if (_flashTimer <= 0f)
                    _icon.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            }
        }
    }
}