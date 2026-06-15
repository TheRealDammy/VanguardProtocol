using UnityEngine;
using VanguardProtocol.Characters;
using VanguardProtocol.AbilitySystem.Abilities;

namespace VanguardProtocol.Combat
{
    public class AimTargetDetector : MonoBehaviour
    {
        [SerializeField] private float _range = 100f;
        [SerializeField] private LayerMask _hitMask;

        private Camera _camera;
        private CharacterBase _owner;

        public bool IsAimingAtEnemy { get; private set; }
        public CharacterBase CurrentTarget { get; private set; }

        private void Awake()
        {
            _camera = Camera.main;
            _owner = GetComponent<CharacterBase>();
        }

        private void Update()
        {
            IsAimingAtEnemy = false;
            CurrentTarget = null;

            Ray ray = _camera.ScreenPointToRay(
                new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

            if (!Physics.Raycast(ray, out RaycastHit hit, _range, _hitMask))
                return;

            var target = hit.collider.GetComponentInParent<CharacterBase>();
            if (target == null || target == _owner || !target.IsAlive)
                return;

            CurrentTarget = target;
            IsAimingAtEnemy = AbilityCombatUtils.IsEnemy(_owner, target);
        }
    }
}