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

            Debug.Log($"[GA] OnEnable — AbilityTag: {abilityTag} | Hash: {abilityTag.hash}");
        }

        protected override IEnumerator ActivateAbility(AbilitySystemComponent asc)
        {
            var owner = asc.Owner;
            Vector3 target = GetThrowTarget(owner);

            Debug.Log($"[Flashbang] {owner.name} threw toward {target} " +
                      $"(distance from owner: {Vector3.Distance(owner.transform.position, target):F1})");

            yield return new WaitForSeconds(_fuseTime);

            Debug.Log($"[Flashbang] DETONATE at {target} — checking radius {_blindRadius}");

            var hits = Physics.OverlapSphere(target, _blindRadius);
            Debug.Log($"[Flashbang] OverlapSphere found {hits.Length} colliders");
            foreach (var h in hits)
                Debug.Log($"  - {h.gameObject.name} (layer: {LayerMask.LayerToName(h.gameObject.layer)})");

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