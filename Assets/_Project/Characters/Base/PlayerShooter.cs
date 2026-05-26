using UnityEngine;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.Characters;
using static UnityEngine.UI.GridLayoutGroup;

namespace VanguardProtocol.Combat
{
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float _fireRate = 0.1f;
        [SerializeField] private float _range = 10f;
        [SerializeField] private LayerMask _hitMask;

        [Header("References")]
        [SerializeField] private Transform _firePoint;

        [Header("VFX")]
        //[SerializeField] private ParticleSystem _muzzleFlash;

        private PlayerInputHandler _inputHandler;
        private CharacterBase _character;
        private Camera _mainCamera;
        private float _nextFireTime;

        private void Awake()
        {
            _inputHandler = GetComponentInParent<PlayerInputHandler>();
            _character = GetComponentInParent<CharacterBase>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_character.isAlive)
                return;
            if (Time.time < _nextFireTime)
                return;
            if (_inputHandler.FirePressed)
            {
                _nextFireTime = Time.time + _fireRate;
            }

            Fire();
        }

        private void Fire()
        {
            _nextFireTime = Time.time + _fireRate;

            //_muzzleFlash?.Play();

            Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

            if (!Physics.Raycast(ray, out RaycastHit hit, _range, _hitMask))
            {
                DrawDebugRay(ray.origin, ray.direction * _range, Color.red);
                return;
            }

            Vector3 firePoint = _firePoint != null ? _firePoint.position : ray.origin;
            DrawDebugRay(firePoint, ray.direction * hit.distance, Color.green);

            CharacterBase target = hit.collider.GetComponentInParent<CharacterBase>();
            if (target == null)
                return;
            if (target == _character)
                return;

            // Team check
            if (target.teamTag == _character.teamTag)
                return;

            float damage = _character.attributes.attackDamage.CurrentValue;
            target.TakeDamage(damage, _character);

            Debug.Log($"[Shooter] {_character.name} hit {target.name} " +
                      $"for {damage}. Target HP: {target.attributes.health.CurrentValue:F1}");
        }

        private void DrawDebugRay(Vector3 origin, Vector3 direction, Color color)
        {
            Debug.DrawRay(origin, direction, color, 1f);

        }
    }
}