using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Agent _agent;
    private FieldOfView _fov;
    private CancellationTokenSource _wanderToken;
    private CancellationTokenSource _reactionToken;
    private Agent _otherAgentCache;

    [SerializeField] private Team team;
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
        _agent.SetAgentLookTarget(team,
            transform.position +
            new Vector3(transform.position.x - LocationMarkerManager.Instance.RequestFlag(Team.Player2).Transform.position.x, 0, 0)
                .normalized * 3);
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
            _agent.SetAgentDestination(team, marker.position);
            _agent.SetAgentLookTarget(team, marker.position);

            //Move to location
            await UniTask.WaitUntil(() => _agent.Arrived, cancellationToken: cancellationToken);
            _agent.SetAgentLookTarget(team,
                transform.position +
                new Vector3(LocationMarkerManager.Instance.RequestFlag(Team.Player2).Transform.position.x - transform.position.x, 0, 0)
                    .normalized * 3);

            //Wait for next wander
            var randomWander = UnityEngine.Random.Range(minWanderDelay, maxWanderDelay);
            await UniTask.WaitForSeconds(randomWander, cancellationToken: cancellationToken);
            LocationMarkerManager.Instance.ReturnLocationMarker(marker);
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
        otherAgent.Eliminate(team);
    }
}