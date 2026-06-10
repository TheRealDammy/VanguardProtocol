namespace VanguardProtocol.AI.BehaviourTrees
{
    public enum BTNodeStatus
    {
        Success,
        Failure,
        Running
    }

    public abstract class BTNode
    {
        public string Name { get; protected set; }
        protected BTNode(string name)
        {
            Name = name;
        }


        // called when node is first entered
        public virtual void OnEnter() { }

        // called every tick while node is active
        public abstract BTNodeStatus Tick(BTContext context);

        // called when node is exited (either success or failure)
        public virtual void OnExit(BTNodeStatus status) { }
    }
}
