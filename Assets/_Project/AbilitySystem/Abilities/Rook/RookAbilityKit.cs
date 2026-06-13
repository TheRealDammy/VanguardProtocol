using UnityEngine;

namespace VanguardProtocol.AbilitySystem.Abilities
{
    [RequireComponent(typeof(AbilitySystemComponent))]
    [RequireComponent(typeof(RageModePassive))]
    public class RookAbilityKit : MonoBehaviour
    {
        [SerializeField] private GameplayAbility _shoulderBash;
        [SerializeField] private GameplayAbility _flashbang;

        private void Start()
        {
            var asc = GetComponent<AbilitySystemComponent>();
            asc.GrantAbility(_shoulderBash);
            asc.GrantAbility(_flashbang);
        }
    }
}