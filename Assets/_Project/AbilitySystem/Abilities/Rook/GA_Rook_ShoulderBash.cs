using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace VanguardProtocol.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "GA_Rook_ShoulderBash", menuName = "Vanguard Protocol/Abilities/Rook/Shoulder Bash")]
    public class GA_Rook_ShoulderBash : GameplayAbility
    {
        [Header("Shoulder Bash")]
        [SerializeField] private float _dashDistance = 4f;
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private float _staggerRadius = 2.5f;

        private void OnEnable()
        {
            abilityTag = GameplayTags.Ability_Rook_ShoulderBash;
            cooldownDuration = 4f;

        }

        protected override IEnumerator ActivateAbility(AbilitySystemComponent asc)
        {
            var owner = asc.Owner;
            var controller = owner.GetComponent<CharacterController>();
            var agent = owner.GetComponent<NavMeshAgent>();

            Vector3 dir = owner.transform.forward;
            float speed = _dashDistance / _dashDuration;
            float elapsed = 0f;

            while (elapsed < _dashDuration)
            {
                Vector3 motion = dir * speed * Time.deltaTime;

                if (controller != null) controller.Move(motion);
                else if (agent != null) agent.Move(motion);

                elapsed += Time.deltaTime;
                yield return null;
            }

            var hits = Physics.OverlapSphere(owner.transform.position, _staggerRadius);

            AbilityCombatUtils.ApplyEffectToEnemiesInRadius(
                owner, owner.transform.position, _staggerRadius,
                effectsToApply, "ShoulderBash");
        }
    }
}