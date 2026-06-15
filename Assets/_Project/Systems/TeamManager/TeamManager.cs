using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanguardProtocol.AbilitySystem;
using VanguardProtocol.Characters;

namespace VanguardProtocol.Systems
{
    public enum Team
    {
        Red,
        Blue
    }

    public class TeamManager : MonoBehaviour
    {
        // -- Singleton --
        public static TeamManager Instance { get; private set; }

        // -- Registry --
        private readonly List<CharacterEntry> _blueTeam = new List<CharacterEntry>();
        private readonly List<CharacterEntry> _redTeam = new List<CharacterEntry>();

        // -- Events --
        public event Action<Characters.CharacterBase, Team> OnCharacterRegistered;
        public event Action<Characters.CharacterBase, Team> OnCharacterEliminated;
        public event Action<Team> OnTeamWiped;
        public event Action<Team> OnMatchOver;

        // -- State --
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // -- Registration --
        // TeamManager.cs — inside RegisterCharacter, before/after adding to the list
        public void RegisterCharacter(CharacterBase character, Team team)
        {
            var entry = new CharacterEntry(character, team);

            if (team == Team.Blue)
            {
                _blueTeam.Add(entry);
                character.SetTeam(GameplayTags.Team_Blue);
            }
            else
            {
                _redTeam.Add(entry);
                character.SetTeam(GameplayTags.Team_Red);
            }

            character.OnDeath += OnCharacterDied;
            OnCharacterRegistered?.Invoke(character, team);

            Debug.Log($"[TeamManager] Registered {character.name} → Team {team}");
        }

        // -- Queeries --
        public List<Characters.CharacterBase> GetTeamMembers(Team team)
        {
            var list = team == Team.Red ? _redTeam : _blueTeam;
            return list.Select(e => e.character).ToList();
        }

        public List<Characters.CharacterBase> GetAliveTeamMembers(Team team)
        {
            var list = team == Team.Red ? _redTeam : _blueTeam;
            return list.Where(e => e.character.IsAlive).Select(e => e.character).ToList();
        }

        public List<Characters.CharacterBase> GetAliveEnemies(Team team)
        {
            Team enemyTeam = team == Team.Red ? Team.Blue : Team.Red;
            return GetAliveTeamMembers(enemyTeam);
        }

        public Characters.CharacterBase GetLowestHPAlly(Team team)
        {
            var aliveAllies = GetAliveTeamMembers(team);
            if (aliveAllies.Count == 0) return null;
            return aliveAllies.OrderBy(c => c.Attributes.health.CurrentValue).First();
        }

        public Characters.CharacterBase GetNearestEnemy(Vector3 position, Team requestingTeam)
        {
            Team enemyTeam = requestingTeam == Team.Red ? Team.Blue : Team.Red;
            var aliveEnemies = GetAliveTeamMembers(enemyTeam);
            if (aliveEnemies.Count == 0) return null;
            return aliveEnemies.OrderBy(e => Vector3.Distance(position, e.transform.position)).FirstOrDefault();
        }

        public Characters.CharacterBase GetNearestAlly(Vector3 position, Team requestingTeam, Characters.CharacterBase excludeCharacter = null)
        {
            var aliveAllies = GetAliveTeamMembers(requestingTeam).Where(c => c != excludeCharacter).ToList();
            if (aliveAllies.Count == 0) return null;
            return aliveAllies.OrderBy(a => Vector3.Distance(position, a.transform.position)).FirstOrDefault();
        }

        public TeamHealthState GetTeamHealthState(Team team)
        {
            var aliveMembers = GetAliveTeamMembers(team);
            if (aliveMembers.Count == 0) return TeamHealthState.Wiped;

            float totalCurrentHealth = aliveMembers.Sum(c => c.Attributes.health.CurrentValue);
            float totalMaxHealth = aliveMembers.Sum(c => c.Attributes.maxHealth.CurrentValue);
            float healthPercentage = totalCurrentHealth / totalMaxHealth;

            if (healthPercentage > 0.6f) return TeamHealthState.Healthy;
            if (healthPercentage > 0.3f) return TeamHealthState.Damaged;
            return TeamHealthState.Critical;
        }

        public int GetAliveCount(Team team)
        {
            return GetAliveTeamMembers(team).Count;
        }

        public bool IsMatchOver()
        {
            return IsGameOver;
        }

        //-- Targeting --
        // returns how many characters on a team are targeting a specific enemy character

        public int GetTargetingCount(Characters.CharacterBase target, Team attackingTeam)
        {
            var team = attackingTeam == Team.Red ? _redTeam : _blueTeam;
            return team.Count(e => e.CurrentTarget == target && e.character.IsAlive);
        }

        public void SetCharcterTarget(Characters.CharacterBase character, Characters.CharacterBase target)
        {
            var entry = _redTeam.Concat(_blueTeam).FirstOrDefault(e => e.character == character);
            if (entry != null)
            {
                entry.CurrentTarget = target;
            }
        }

        // -- Internal Logic --

        private void OnCharacterDied(Characters.CharacterBase character)
        {
            var entry = FindEntry(character);
            if (entry == null) return;

            character.OnDeath -= OnCharacterDied;

            Debug.Log($"[TeamManager] {character.name} from {entry.Team} team has been eliminated.");

            OnCharacterEliminated?.Invoke(character, entry.Team);

            EvaluateMatchState(entry.Team);
        }

        private void EvaluateMatchState(Team eliminatedFromTeam)
        {
            if (IsGameOver) return;

            bool redTeamWiped = _redTeam.All(c => !c.character.IsAlive);
            bool blueTeamWiped = _blueTeam.All(c => !c.character.IsAlive);

            if (redTeamWiped && blueTeamWiped)
            {
                IsGameOver = true;
                Debug.Log("[TeamManager] Both teams wiped! It's a draw!");
            }
            else if (redTeamWiped)
            {
                IsGameOver = true;
                Debug.Log("[TeamManager] Red team wiped! Blue team wins!");
                OnTeamWiped?.Invoke(Team.Blue);
                EndMatch(Team.Blue);
            }
            else if (blueTeamWiped)
            {
                IsGameOver = true;
                Debug.Log("[TeamManager] Blue team wiped! Red team wins!");
                OnTeamWiped?.Invoke(Team.Red);
                EndMatch(Team.Red);
            }
        }

        private void EndMatch(Team winningTeam)
        {
            IsGameOver = true;
            Debug.Log($"[TeamManager] Match over! Team {winningTeam} wins!");
            OnMatchOver?.Invoke(winningTeam);
        }

        private CharacterEntry FindEntry(Characters.CharacterBase character)
        {
            return _blueTeam.FirstOrDefault(e => e.character == character) ?? _redTeam.FirstOrDefault(e => e.character == character);
        }

        // -- Helper Classes --
        private class CharacterEntry
        {
            public Characters.CharacterBase character;
            public Team Team;
            public Characters.CharacterBase CurrentTarget;
            public CharacterEntry(Characters.CharacterBase character, Team team)
            {
                this.character = character;
                this.Team = team;
                this.CurrentTarget = null;
            }
        }
    }

    public enum TeamHealthState
    {
        Healthy,
        Damaged,
        Critical,
        Wiped
    }
}
