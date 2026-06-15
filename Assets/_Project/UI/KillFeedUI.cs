using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VanguardProtocol.Characters;
using VanguardProtocol.Systems;

namespace VanguardProtocol.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class KillFeedUI : MonoBehaviour
    {
        [SerializeField] private float _entryLifetime = 4f;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private int _maxEntries = 5;

        private VisualElement _container;
        private readonly List<VisualElement> _entries = new();

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _container = new VisualElement();
            _container.style.position = Position.Absolute;
            _container.style.right = 30f;
            _container.style.top = 30f;
            _container.style.width = 240f;
            _container.style.alignItems = Align.FlexEnd;
            _container.pickingMode = PickingMode.Ignore;

            root.Add(_container);
        }

        private void Start()
        {
            if (TeamManager.Instance != null)
                TeamManager.Instance.OnCharacterEliminated += HandleEliminated;
        }

        private void OnDestroy()
        {
            if (TeamManager.Instance != null)
                TeamManager.Instance.OnCharacterEliminated -= HandleEliminated;
        }

        private void HandleEliminated(CharacterBase character, Team team)
        {
            var entry = new Label($"{character.gameObject.name} eliminated");
            entry.style.color = team == Team.Blue
                ? new Color(0.4f, 0.6f, 1f) : new Color(1f, 0.4f, 0.4f);
            entry.style.fontSize = 14f;
            entry.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
            entry.style.paddingLeft = 8f;
            entry.style.paddingRight = 8f;
            entry.style.paddingTop = 4f;
            entry.style.paddingBottom = 4f;
            entry.style.marginBottom = 4f;

            _container.Add(entry);
            _entries.Add(entry);

            if (_entries.Count > _maxEntries)
            {
                var oldest = _entries[0];
                _entries.RemoveAt(0);
                _container.Remove(oldest);
            }

            entry.schedule.Execute(() => FadeOut(entry))
                .StartingIn((long)(_entryLifetime * 1000));
        }

        private void FadeOut(VisualElement entry)
        {
            entry.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("opacity") };
            entry.style.transitionDuration = new List<TimeValue> { new TimeValue(_fadeDuration, TimeUnit.Second) };
            entry.style.opacity = 0f;

            entry.schedule.Execute(() =>
            {
                _entries.Remove(entry);
                _container.Remove(entry);
            }).StartingIn((long)(_fadeDuration * 1000));
        }
    }
}