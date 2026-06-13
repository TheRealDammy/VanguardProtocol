using UnityEngine;
using System;
using System.Collections.Generic;
using VanguardProtocol.AbilitySystem;

namespace VanguardProtocol.Characters
{
    public class AIStateMachine : MonoBehaviour
    {
        // -- State --
        public AIStateType CurrentState { get; private set; } = AIStateType.Idle;
        public AIStateType PreviousState { get; private set; } = AIStateType.Idle;
        public float TimeInState { get; private set; } = 0f;

        // -- Events --
        public event Action<AIStateType, AIStateType> OnStateChanged;
        private AICharacterBase _owner;

        private void Awake()
        {
            _owner = GetComponent<AICharacterBase>();
        }

        private void Update()
        {
            TimeInState += Time.deltaTime;
        }

        // -- Public Methods --
        public void TransitionTo(AIStateType newState)
        {
            if (newState == CurrentState) return;

            PreviousState = CurrentState;
            CurrentState = newState;
            TimeInState = 0f;

            // sync gameplay tags with the new state
            SyncStateTags(PreviousState, CurrentState);

            OnStateChanged?.Invoke(PreviousState, CurrentState);

            Debug.Log($"[AIStateMachine] {_owner.name} Transitioned from {PreviousState} to {CurrentState}");
        }

        public bool IsInState(AIStateType state) => CurrentState == state;

        public bool WasInState(AIStateType state) => PreviousState == state;

        // -- Private Methods --

        private void SyncStateTags(AIStateType oldState, AIStateType newState)
        {
            // Remove old state tag
            var oldTag = GetTagForState(oldState);
            if (oldTag.IsValid())
            {
                _owner.Tags.RemoveTag(oldTag);
            }
            // Add new state tag
            var newTag = GetTagForState(newState);
            if (newTag.IsValid())
            {
                 _owner.Tags.AddTag(newTag);
            }
        }

        private GameplayTag GetTagForState(AIStateType state)
        {
            // Map AI states to gameplay tags
            return state switch
            {
                AIStateType.Idle => GameplayTags.AIState_Idle,
                AIStateType.Engaging => GameplayTags.AIState_Engaging,
                AIStateType.Retreating => GameplayTags.AIState_Retreating,
                AIStateType.Supporting => GameplayTags.AIState_Supporting,
                AIStateType.Flanking => GameplayTags.AIState_Flanking,
                AIStateType.Downed => GameplayTags.AIState_Downed,
                _ => GameplayTag.none
            };
        }
    }

    public enum AIStateType
    {
        Idle,
        Engaging,
        Retreating,
        Supporting,
        Flanking,
        Downed
    }
}