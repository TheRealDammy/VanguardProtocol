using UnityEngine;
using UnityEngine.UIElements;
using VanguardProtocol.AbilitySystem;

namespace VanguardProtocol.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class AbilityCooldownUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AbilitySystemComponent _asc;

        [Header("Layout")]
        [SerializeField] private float _iconSize = 56f;
        [SerializeField] private float _iconGap = 12f;

        [Header("Icons")]
        [SerializeField] private Texture2D _ability1Icon; // Shoulder Bash
        [SerializeField] private Texture2D _ability2Icon; // Flashbang

        private AbilityIcon _icon1; // Ability1 — Shoulder Bash
        private AbilityIcon _icon2; // Ability2 — Flashbang

        private void Awake()
        {
            BuildUI();
        }

        private void Update()
        {
            UpdateIcon(_icon1, GameplayTags.Ability_Rook_ShoulderBash);
            UpdateIcon(_icon2, GameplayTags.Ability_Rook_Flashbang);
        }

        private void BuildUI()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            var container = new VisualElement();
            container.style.position = Position.Absolute;
            container.style.bottom = 30f;
            container.style.right = 30f;
            container.style.flexDirection = FlexDirection.Row;
            container.pickingMode = PickingMode.Ignore;

            _icon1 = CreateIcon("Q", _ability1Icon);
            _icon2 = CreateIcon("E", _ability2Icon);

            container.Add(_icon1.Root);

            var spacer = new VisualElement();
            spacer.style.width = _iconGap;
            container.Add(spacer);

            container.Add(_icon2.Root);

            root.Add(container);
        }

        private AbilityIcon CreateIcon(string keyLabel, Texture2D icon)
        {
            var root = new VisualElement();
            root.style.width = _iconSize;
            root.style.height = _iconSize;
            root.style.overflow = Overflow.Hidden;
            root.pickingMode = PickingMode.Ignore;

            // Icon image — fills the square
            if (icon != null)
            {
                var iconElement = new VisualElement();
                iconElement.style.position = Position.Absolute;
                iconElement.style.left = 0; iconElement.style.top = 0;
                iconElement.style.right = 0; iconElement.style.bottom = 0;
                iconElement.style.backgroundImage = new StyleBackground(icon);
                iconElement.style.backgroundSize = new StyleBackgroundSize(
                    new BackgroundSize(BackgroundSizeType.Cover));
                iconElement.pickingMode = PickingMode.Ignore;
                root.Add(iconElement);
            }

            var overlay = new CooldownIconElement();
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0; overlay.style.top = 0;
            overlay.style.right = 0; overlay.style.bottom = 0;
            root.Add(overlay);

            var timer = new Label("");
            timer.style.position = Position.Absolute;
            timer.style.left = 0; timer.style.right = 0;
            timer.style.top = 0; timer.style.bottom = 0;
            timer.style.unityTextAlign = TextAnchor.MiddleCenter;
            timer.style.fontSize = 18f;
            timer.style.color = Color.white;
            root.Add(timer);

            return new AbilityIcon { Root = root, Overlay = overlay, TimerLabel = timer };
        }

        private void UpdateIcon(AbilityIcon icon, GameplayTag abilityTag)
        {
            if (_asc == null) return;

            var ability = _asc.GetAbilityByTag(abilityTag);
            if (ability == null)
            {
                icon.Overlay.Fraction = 0f;
                icon.TimerLabel.text = "";
                return;
            }

            float remaining = ability.GetCooldownRemaining();
            float total = ability.cooldownDuration;

            float fraction = total > 0f ? remaining / total : 0f;
            icon.Overlay.Fraction = fraction;

            icon.TimerLabel.text = remaining > 0.1f
                ? Mathf.CeilToInt(remaining).ToString()
                : "";
        }

        private class AbilityIcon
        {
            public VisualElement Root;
            public CooldownIconElement Overlay;
            public Label TimerLabel;
        }
    }
}