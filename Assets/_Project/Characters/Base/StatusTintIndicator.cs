using UnityEngine;
using VanguardProtocol.AbilitySystem;

namespace VanguardProtocol.Characters
{
    [RequireComponent(typeof(CharacterBase))]
    public class StatusTintIndicator : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _staggerColor = Color.yellow;
        [SerializeField] private Color _blindColor = new Color(0.6f, 0.6f, 1f);
        [SerializeField] private Color _rageColor = new Color(1f, 0.3f, 0.1f);

        private CharacterBase _character;
        private Color _baseColor;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _mpb = new MaterialPropertyBlock();

            if (_renderer == null)
                _renderer = GetComponentInChildren<Renderer>();

            if (_renderer != null)
                _baseColor = _renderer.sharedMaterial.color;
        }

        private void Start()
        {
            _character.Tags.OnTagAdded += _ => Refresh();
            _character.Tags.OnTagRemoved += _ => Refresh();
        }

        private void Refresh()
        {
            if (_renderer == null) return;

            Color target = _baseColor;

            if (_character.Tags.HasTag(GameplayTags.State_LowHealth))
                target = _rageColor;
            if (_character.Tags.HasTag(GameplayTags.Status_Blinded))
                target = _blindColor;
            if (_character.Tags.HasTag(GameplayTags.Status_Staggered))
                target = _staggerColor;

            _mpb.SetColor("_Color", target);
            _mpb.SetColor("_BaseColor", target); // URP Lit shader uses _BaseColor
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}