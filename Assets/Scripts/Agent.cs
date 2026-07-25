using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    private static int _id;
    public readonly int ID = _id++;
    public int number;

    [SerializeField] private Team team;
    [SerializeField] private TextMeshPro numberText;
    [SerializeField] private GameObject fovVisual;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform lookTargetTransform;
    public Transform rotationPivotTransform;

    private NavMeshAgent _agent;

    private Vector3 _lookTarget;


    public readonly ObservableProperty<bool> IsVisible = new(false);
    public bool Arrived { get; private set; }
    public Team Team => team;

    public void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        
        numberText.text = number.ToString();
        if (Team != Team.Player1) numberText.gameObject.SetActive(false);
        fovVisual.SetActive(false);
        lineRenderer.gameObject.SetActive(false);
        lookTargetTransform.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        IsVisible.OnChanged.AddListener(UpdateVisibility);
    }

    private void OnDisable()
    {
        IsVisible.OnChanged.RemoveListener(UpdateVisibility);
    }

    
    private void Update()
    {
        UpdateRotation();
        UpdateLookTarget();
        Arrived = _agent.pathStatus == NavMeshPathStatus.PathComplete && _agent.remainingDistance < 0.1f;
    }

    private void UpdateRotation()
    {
        if (_lookTarget == Vector3.zero) return;

        var trgt = rotationPivotTransform.InverseTransformPoint(_lookTarget);
        var angle = Mathf.Atan2(trgt.y, trgt.x) * Mathf.Rad2Deg - 90;

        rotationPivotTransform.Rotate(0, 0, angle);
    }

    private void UpdateLookTarget()
    {
        if (!lineRenderer) return;
        if (_lookTarget == Vector3.zero) return;
        lookTargetTransform.position = _lookTarget;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, lookTargetTransform.localPosition);
    }

    public void Eliminate(Team eliminatedBy)
    {
        if (team == eliminatedBy) return;
        gameObject.SetActive(false);
    }

    public void SetAgentDestination(Team playerTeam, Vector3 destination)
    {
        if (playerTeam != Team) return;
        _agent.SetDestination(destination);
    }

    public void SetAgentLookTarget(Team playerTeam, Vector3 target)
    {
        if (playerTeam != Team) return;
        _lookTarget = target;
    }

    private void UpdateVisibility(bool visible)
    {
        switch (Team)
        {
            case Team.Player2:
                numberText.gameObject.SetActive(visible);
                break;
            case Team.Player1:
                numberText.color = visible ? Color.red : Color.white;
                break;
            case Team.None:
                print($"No Team assigned to {name}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SelectedVisuals(bool isSelected)
    {
        fovVisual.SetActive(isSelected);
        lineRenderer.gameObject.SetActive(isSelected);
        lookTargetTransform.gameObject.SetActive(isSelected);
    }

    public void HoverVisuals(bool isHovered)
    {
        fovVisual.SetActive(isHovered);
    }
    
}