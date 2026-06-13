using UnityEngine;
using VanguardProtocol.Characters;

namespace VanguardProtocol.AI.BehaviourTrees
{
    public class BehaviourTree : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float _tickRate = 0.1f; // 10 ticks per second

        private BTNode _rootNode;
        private BTContext _context;
        private float _tickTimer;
        private bool _isRunning;

        public BTContext Context => _context;

        public void Initialize(AICharacterBase owner, BTNode rootNode)
        {
            _rootNode = rootNode;
            _context = new BTContext(owner);    
            _rootNode.OnEnter();
            _isRunning = true;
        }

        public void Stop()
        {
            _isRunning = false;
            _rootNode?.OnExit(BTNodeStatus.Failure);
        }

        private void Update()
        {

            if (!_isRunning || _rootNode == null) return;

            _tickTimer -= Time.deltaTime;
            if (_tickTimer > 0) return;

            _tickTimer = _tickRate;
            _rootNode.Tick(_context); 
        }
    }
}
