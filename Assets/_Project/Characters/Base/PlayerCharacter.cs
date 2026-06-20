using System.Collections;
using UnityEngine;
using VanguardProtocol.AbilitySystem;

namespace VanguardProtocol.Characters
{
    [RequireComponent(typeof(AbilitySystemComponent))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(CameraController))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : CharacterBase
    {
        [Header("Settings")]
        [SerializeField] private float _sprintMultiplier = 1.6f;
        //[SerializeField] private float _rotationSpeed = 12f;
        [SerializeField] private float _rollCooldown = 0.6f;
        [SerializeField] private float _rollDistance = 4f;
        [SerializeField] private float _rollDuration = 0.3f;
        [SerializeField] private float _gravity = -19.62f;

        [Header("Camera")]
        [SerializeField] private Transform _cameraPivot;

        // -- Components --
        private PlayerInputHandler _inputHandler;
        private CharacterController _characterController;

        // -- State --
        private Vector3 _velocity;
        private float _rollCooldownTimer;
        private bool _isRolling;
        private bool _isGrounded;

        protected override void Awake()
        {
            base.Awake();

            _inputHandler = GetComponent<PlayerInputHandler>();
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!IsAlive) return;

            HandleMovement();
            HandleGravity();
            HandleRolling();
            HandleAbilityInput();
        }

        // -- Movement --
        private void HandleGravity()
        {
            _isGrounded = _characterController.isGrounded;

            if (_isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f; // Small negative to keep grounded
            }

            _velocity.y += _gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void HandleMovement()
        {
            if (_isRolling) return;

            float speed = Attributes.moveSpeed.CurrentValue;
            if (_inputHandler.SprintHeld)
            {
                speed *= _sprintMultiplier;
            }

           Vector3 move = transform.forward * _inputHandler.MoveInput.y + transform.right * _inputHandler.MoveInput.x;
            move = move.normalized * speed * Time.deltaTime;
            move.y = _velocity.y * Time.deltaTime; // Preserve vertical velocity

            _characterController.Move(move);
        }

        // -- Rolling --

        private void HandleRolling()
        {
            if (_rollCooldownTimer > 0f)
            {
                _rollCooldownTimer -= Time.deltaTime;
            }
            if (_inputHandler.RollPressed && _rollCooldownTimer <= 0f && !_isRolling)
            {
                StartCoroutine(RollCoroutine());
            }
        }

        private IEnumerator RollCoroutine()
        {
            _isRolling = true;
            _rollCooldownTimer = _rollCooldown;

            Tags.AddTag(GameplayTags.Status_Invisible);

            Vector3 rollDirection = transform.forward;

            if (_inputHandler.MoveInput != Vector2.zero)
            {
                rollDirection = transform.forward * _inputHandler.MoveInput.y + transform.right * _inputHandler.MoveInput.x;
                rollDirection = rollDirection.normalized;
            }

            float elapsed = 0f;
            float speed = _rollDistance / _rollDuration;

            while (elapsed < _rollDuration)
            {
                Vector3 motion = rollDirection * speed * Time.deltaTime;
                motion.y = _velocity.y * Time.deltaTime; // Preserve vertical velocity
                _characterController.Move(motion);

                elapsed += Time.deltaTime;
                yield return null;
            }

            Tags.RemoveTag(GameplayTags.Status_Invisible);
            _isRolling = false;
        }

        // -- Abilities --
        private void HandleAbilityInput()
        {
            if (AbilitySystem == null) return;

            if (_inputHandler.Ability1Pressed)
            {
                AbilitySystem.TryActivateAbility(GameplayTags.Ability_Rook_ShoulderBash);
            }

            if (_inputHandler.Ability2Pressed)
            {
                AbilitySystem.TryActivateAbility(GameplayTags.Ability_Rook_Flashbang);
            }
        }
    }
}
