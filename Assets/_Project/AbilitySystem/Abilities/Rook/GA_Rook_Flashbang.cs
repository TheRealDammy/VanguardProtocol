using System.Collections;
using UnityEngine;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "GA_Rook_Flashbang", menuName = "Vanguard Protocol/Abilities/Rook/Flashbang")]
    public class GA_Rook_Flashbang : GameplayAbility
    {
        [Header("Flashbang")]
        [SerializeField] private float _throwRange = 12f;
        [SerializeField] private float _blindRadius = 5f;
        [SerializeField] private float _fuseTime = 0.75f;

        private void OnEnable()
        {
            abilityTag = GameplayTags.Ability_Rook_Flashbang;
            cooldownDuration = 8f;
        }

        protected override IEnumerator ActivateAbility(AbilitySystemComponent asc)
        {
            var owner = asc.Owner;
            Vector3 target = GetThrowTarget(owner);

            yield return new WaitForSeconds(_fuseTime);

            var hits = Physics.OverlapSphere(target, _blindRadius);

            AbilityCombatUtils.ApplyEffectToEnemiesInRadius(
                owner, target, _blindRadius, effectsToApply, "Flashbang");
        }

        private Vector3 GetThrowTarget(CharacterBase owner)
        {
            if (owner is PlayerCharacter)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    var ray = cam.ScreenPointToRay(
                        new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

                    if (Physics.Raycast(ray, out var hit, _throwRange))
                        return hit.point;

                    return ray.origin + ray.direction * _throwRange;
                }
            }

            if (owner is AICharacterBase ai && ai.CurrentTarget != null)
                return ai.CurrentTarget.transform.position;

            return owner.transform.position + owner.transform.forward * _throwRange;
        }
    }
}