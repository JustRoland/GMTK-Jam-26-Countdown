using System;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public int agentID;
    private NavMeshAgent _agent;

    private Vector3 _targetDestination;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

    }

    public void SetAgentDestination(int playerID, Vector3 destination)
    {
        if (playerID != agentID) return;
        _agent.SetDestination(destination);
    }
}