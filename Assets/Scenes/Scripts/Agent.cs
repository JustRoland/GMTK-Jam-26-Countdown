using System;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public int agentID;
    private NavMeshAgent _agent;

    private Vector3 _lookTarget;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

    }

    private void Update()
    {
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (_lookTarget == Vector3.zero) return;

        var trgt = transform.InverseTransformPoint(_lookTarget);
        var angle = Mathf.Atan2(trgt.y, trgt.x) * Mathf.Rad2Deg - 90;
        
        transform.Rotate(0, 0, angle);
    }

    public void SetAgentDestination(int playerID, Vector3 destination)
    {
        if (playerID != agentID) return;
        _agent.SetDestination(destination);
    }

    public void SetAgentLookTarget(int playerID, Vector3 target)
    {
        if (playerID != agentID) return;
        _lookTarget = target;
    }

}