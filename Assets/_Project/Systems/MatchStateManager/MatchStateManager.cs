using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using VanguardProtocol.Systems;

namespace VanguardProtocol.Systems
{
    public enum MatchState
    {
        Idle, // Waiting for players to join
        Deployment, // Character Spawning and setup
        Combat, // Active gameplay
        Resolution // Post-match summary and rewards
    }

    public class MatchStateManager : MonoBehaviour
    {
        // -- Singleton Implementation --
        public static MatchStateManager Instance { get; private set; }

        // -- State --
        public MatchState CurrentState { get; private set; } = MatchState.Idle;
        public float MatchTime { get; private set; }
        public float CombatTimer { get; private set; }

        [Header("Config")]
        [SerializeField] private float _deploymentDuration = 3f; // Time for players to deploy
        [SerializeField] private float _maxMatchDuration = 300f; // Max combat time (5 minutes)

        // -- Events --
        public event Action<MatchState, MatchState> OnStateChanged; // (oldState, newState)
        public event Action OnMatchStarted;
        public event Action OnCombatStarted;
        public event Action<Team> OnMatchResolved;

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

        private void Start()
        {
            if (TeamManager.Instance == null)
            {
                Debug.LogError("MatchStateManager requires TeamManager to function.");
                return;
            }
            else
            {
                TeamManager.Instance.OnMatchOver += HandleMatchOver;
            }
        }

        private void Update()
        {
            if (CurrentState == MatchState.Idle)
                return;

            else if (CurrentState == MatchState.Combat)
            {
                MatchTime += Time.deltaTime;
                CombatTimer += Time.deltaTime;

                if (CombatTimer >= _maxMatchDuration)
                {
                    // Force end match if max duration is reached
                    HandleMatchOver(Team.Blue); // Arbitrarily declare Blue as winner for timeout
                }
            }
        }

        // -- Public Methods --
        public void StartMatch()
        {
            if (CurrentState != MatchState.Idle)
            {
                Debug.LogWarning("Cannot start match. Current state: " + CurrentState);
                return;
            }
            StartCoroutine(MatchSequence());
        }

        public void RestartMatch()
        {
            StopAllCoroutines();
            // Reset state and timers
            MatchTime = 0f;
            CombatTimer = 0f;
            TransitionTo(MatchState.Idle);
            StartMatch();
        }

        public bool IsInCombat() => CurrentState == MatchState.Combat;
        public bool IsDeploying() => CurrentState == MatchState.Deployment;
        public bool IsResloved() => CurrentState == MatchState.Resolution;
        public string GetMatchTime() => FormatTime(CombatTimer);

        // -- Private Methods --
        private IEnumerator MatchSequence()
        {
            // Deployment Phase
            TransitionTo(MatchState.Deployment);
            OnMatchStarted?.Invoke();

            Debug.Log($"[MatchStateManager] Match started. Deployment phase begins" + $"- {_deploymentDuration} until combat");
            yield return new WaitForSeconds(_deploymentDuration);

            // Combat Phase
            TransitionTo(MatchState.Combat);
            OnCombatStarted?.Invoke();

            Debug.Log($"[MatchStateManager] Combat phase begins. Max duration: {_maxMatchDuration} seconds");

            while (CombatTimer < _maxMatchDuration)
            {
                yield return null;
            }

            Debug.Log($"[MatchStateManager] Max match duration reached. Ending match.");
        }

        private void HandleMatchOver(Team winningTeam)
        {
            if (CurrentState == MatchState.Resolution)
                return;
            TransitionTo(MatchState.Resolution);
            OnMatchResolved?.Invoke(winningTeam);

            Debug.Log($"[MatchStateManager] Match over! Winning team: {winningTeam} | Time: {GetMatchTime()}");
        }

        private void TransitionTo(MatchState newState)
        {
            MatchState oldState = CurrentState;
            CurrentState = newState;

            OnStateChanged?.Invoke(oldState, newState);

            Debug.Log($"[MatchStateManager] State changed: {oldState} -> {newState}");
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}