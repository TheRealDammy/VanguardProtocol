using UnityEngine;
using VanguardProtocol.Characters;
using VanguardProtocol.Systems;

namespace VanguardProtocol.Systems
{
    public class MatchBootstrapper : MonoBehaviour
    {
        [Header("Blue Team")]
        [SerializeField] private PlayerCharacter _player;

        [Header("Red Team")]
        [SerializeField] private AICharacterBase[] _enemies;

        private void Start()
        {
            // Register player
            TeamManager.Instance.RegisterCharacter(_player, Team.Blue);

            // Register enemies
            foreach (var enemy in _enemies)
                TeamManager.Instance.RegisterCharacter(enemy, Team.Red);

            // Start match
            MatchStateManager.Instance.StartMatch();
        }
    }
}