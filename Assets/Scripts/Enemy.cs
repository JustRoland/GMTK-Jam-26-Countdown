using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    private Agent _agent;
    private FieldOfView _fov;
    private CancellationTokenSource _wanderToken;
    private CancellationTokenSource _reactionToken;
    private Agent _otherAgentCache;
    
    [SerializeField] private float minWanderDelay;
    [SerializeField] private float maxWanderDelay;
    [SerializeField] private float reactionTime;


    public void Awake()
    {
        _agent = GetComponent<Agent>();
        _fov = GetComponent<FieldOfView>();
    }

    private void Start()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        _wanderToken = new CancellationTokenSource();
        if (LocationMarkerManager.Instance == null) return;
        Wander(_wanderToken.Token).Forget();
        _fov.AgentInViewEvent.AddListener(OnAgentInView);
    }

    private void OnDisable()
    {
        _wanderToken?.Cancel();
        _wanderToken?.Dispose();
        _reactionToken?.Cancel();
        _reactionToken?.Dispose();
        _fov.AgentInViewEvent.RemoveListener(OnAgentInView);
    }

    private async UniTask Wander(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            //Find location to move to
            var marker = LocationMarkerManager.Instance.RequestLocationMarker();
            if (marker)
            {
                //Move to location
                _agent.SetAgentLookTarget(_agent.Team, marker.position, marker.name);
                _agent.SetAgentDestination(_agent.Team, marker.position);
                await UniTask.WaitForEndOfFrame(cancellationToken);
                await UniTask.WaitUntil(() => _agent.Arrived, timing: PlayerLoopTiming.PostLateUpdate, cancellationToken: cancellationToken);
                _agent.SetAgentLookTarget(_agent.Team,
                    transform.position +
                    new Vector3(
                            LocationMarkerManager.Instance.RequestFlag(_agent.enemyTeam).Transform.position.x -
                            transform.position.x, 0, 0)
                        .normalized * 3, "Opponents");
            }
            

            //Wait for next wander
            var randomWander = UnityEngine.Random.Range(minWanderDelay, maxWanderDelay);
            await UniTask.WaitForSeconds(randomWander, cancellationToken: cancellationToken);
            if (marker) LocationMarkerManager.Instance.ReturnLocationMarker(marker); 
            
            //NOTE: Possible marker overlap if the marker gets returned, but no new marker is found. 
        }
    }

    private void OnAgentInView(Agent otherAgent, bool isVisible)
    {
        if (isVisible)
        {
            if (_otherAgentCache) return;
            _otherAgentCache = otherAgent;
            _reactionToken = new CancellationTokenSource();
            ReactionTimer(otherAgent, reactionTime, _reactionToken.Token).Forget();
        }
        else
        {
            if (otherAgent != _otherAgentCache) return;
            _reactionToken.Cancel();
            _reactionToken.Dispose();
            _otherAgentCache = null;
        }
    }

    private async UniTask ReactionTimer(Agent otherAgent, float delay, CancellationToken cancellationToken)
    {
        await UniTask.WaitForSeconds(delay, cancellationToken: cancellationToken);
        otherAgent.Eliminate(_agent.Team);
    }
}