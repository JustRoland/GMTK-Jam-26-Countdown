using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Agent : MonoBehaviour
{
    private static int _id;
    public readonly int ID = _id++; 
    
    [SerializeField] private int team;
    private NavMeshAgent _agent;

    private Vector3 _lookTarget;

    public int Team => team;

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

    public void SetAgentDestination(int playerTeam, Vector3 destination)
    {
        if (playerTeam != Team) return;
        _agent.SetDestination(destination);
    }

    public void SetAgentLookTarget(int playerTeam, Vector3 target)
    {
        if (playerTeam != Team) return;
        _lookTarget = target;
    }

}