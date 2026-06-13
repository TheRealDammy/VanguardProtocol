namespace VanguardProtocol.AbilitySystem
{
    public static class GameplayTags
    {
        // -- AI States --
        public static readonly GameplayTag AIState_Idle = new GameplayTag("AIState.Idle");
        public static readonly GameplayTag AIState_Engaging = new GameplayTag("AIState.Engaging");
        public static readonly GameplayTag AIState_Retreating = new GameplayTag("AIState.Retreating");
        public static readonly GameplayTag AIState_Supporting = new GameplayTag("AIState.Supporting");
        public static readonly GameplayTag AIState_Flanking = new GameplayTag("AIState.Flanking");
        public static readonly GameplayTag AIState_Downed = new GameplayTag("AIState.Downed");

        // -- Character States --
        public static readonly GameplayTag State_InCombat = new GameplayTag("State.InCombat");
        public static readonly GameplayTag State_LowHealth = new GameplayTag("State.LowHealth");
        public static readonly GameplayTag State_Stationary = new GameplayTag("State.Stationary");
        public static readonly GameplayTag State_Downed = new GameplayTag("State.Downed");
        public static readonly GameplayTag State_Regenerating = new GameplayTag("State.Regenerating");

        // -- Roles --
        public static readonly GameplayTag Role_Assault = new GameplayTag("Role.Assault");
        public static readonly GameplayTag Role_Support = new GameplayTag("Role.Support");
        public static readonly GameplayTag Role_Sniper = new GameplayTag("Role.Sniper");
        public static readonly GameplayTag Role_Breacher = new GameplayTag("Role.Breacher");
        public static readonly GameplayTag Role_Medic = new GameplayTag("Role.Medic");
        public static readonly GameplayTag Role_Saboteur = new GameplayTag("Role.Saboteur");

        // -- Status Effects --
        public static readonly GameplayTag Status_Slowed = new GameplayTag("Status.Slowed");
        public static readonly GameplayTag Status_Blinded = new GameplayTag("Status.Blinded");
        public static readonly GameplayTag Status_Invisible = new GameplayTag("Status.Invisible");
        public static readonly GameplayTag Status_AbilityDisabled = new GameplayTag("Status.AbilityDisabled");
        public static readonly GameplayTag Status_Staggered = new GameplayTag("Status.Staggered");

        // -- Ability Blocking --
        public static readonly GameplayTag Ability_Blocked = new GameplayTag("Ability.Blocked");
        public static readonly GameplayTag Ability_Active = new GameplayTag("Ability.Active");

        // -- Events (used to drive event-based communication) --
        public static readonly GameplayTag Event_FocusFire = new GameplayTag("Event.FocusFire");
        public static readonly GameplayTag Event_CoverDestroyed = new GameplayTag("Event.CoverDestroyed");
        public static readonly GameplayTag Event_CharacterEliminated = new GameplayTag("Event.CharacterEliminated");
        public static readonly GameplayTag Event_PingEnemy = new GameplayTag("Event.PingEnemy");
        public static readonly GameplayTag Event_PingPosition = new GameplayTag("Event.PingPosition");
        public static readonly GameplayTag Event_PingDanger = new GameplayTag("Event.PingDanger");

        // -- Teams --
        public static readonly GameplayTag Team_Red = new GameplayTag("Team.Red");
        public static readonly GameplayTag Team_Blue = new GameplayTag("Team.Blue");

        // -- Rook Abilities
        public static readonly GameplayTag Ability_Rook_ShoulderBash = new GameplayTag("Ability.Rook.ShoulderBash");
        public static readonly GameplayTag Ability_Rook_Flashbang = new GameplayTag("Ability.Rook.Flashbang");
    }
}
