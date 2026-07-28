using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Agent : MonoBehaviour
{
    private static int _id;
    public readonly int ID = _id++;
    public int number;

    public Team myTeam;
    public Team enemyTeam;
    [SerializeField] private TextMeshPro numberText;
    [SerializeField] private SpriteRenderer numberBackground;
    private Color _normalBackgroundColor;
    [SerializeField] private GameObject fovVisual;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform lookTargetTransform;
    public Transform rotationPivotTransform;
    [SerializeField] private float coverDistance = 0.3f;
    [SerializeField] private float coverAngle = 90;

    private NavMeshAgent _navAgent;

    private Vector3 _lookTarget;


    public readonly ObservableProperty<bool> IsVisible = new(false);
    public bool HasArrived { get; private set; }
    public bool InCover { get; private set; }
    public Team Team => myTeam;

    public void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.updateRotation = false;
        _navAgent.updateUpAxis = false;

        _normalBackgroundColor = numberBackground.color;
        numberText.text = number.ToString();
        if (Team != Team.Blue) numberText.gameObject.SetActive(false);
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
        if (LocationMarkerManager.Instance != null) LocationMarkerManager.Instance.ReturnFlag(this);
        IsVisible.OnChanged.RemoveListener(UpdateVisibility);
    }

    private void Start()
    {
        if (LocationMarkerManager.Instance == null) return;
        SetAgentLookTarget(myTeam,
            transform.position +
            new Vector3(
                    LocationMarkerManager.Instance.RequestFlag(enemyTeam).Transform.position.x - transform.position.x,
                    0, 0)
                .normalized * 3);
    }


    private void Update()
    {
        HasArrived = _navAgent.velocity.sqrMagnitude < 0.001f && _navAgent.remainingDistance < 0.001f;
        UpdateLookTarget();
        UpdateRotation();
        if (CoverManager.Instance)
        {
            InCover = CheckForCover();
            numberBackground.color = InCover ? Color.gray : _normalBackgroundColor;
        }
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

    private bool CheckForCover()
    {
        //If facing a nearby obstacle, InCover = true;
        var cover = CoverManager.Instance.ReturnNearestCoverPosition(transform.position);
        if (cover.Item1 == Vector3.zero) return false;
        if (Vector3.Distance(cover.Item1, transform.position) > coverDistance) return false;
        if (cover.Item2 <= 0) return false;
        return Vector3.Angle(rotationPivotTransform.up, cover.Item1 - transform.position) < coverAngle;
    }

    public void Eliminate(Team eliminatedBy)
    {
        if (myTeam == eliminatedBy) return;
        gameObject.SetActive(false);
    }

    public void SetAgentDestination(Team playerTeam, Vector3 destination)
    {
        if (playerTeam != Team) return;
        HasArrived = false;
        _navAgent.SetDestination(destination);
    }

    public void SetAgentLookTarget(Team playerTeam, Vector3 target)
    {
        if (playerTeam != Team) return;
        _lookTarget = target;
    }

    /// <summary>
    /// Since agents are controlled by both Player and Enemy, this method decides how they react to being seen based on which team they are on.
    /// </summary>
    /// <param name="visible"></param>
    private void UpdateVisibility(bool visible)
    {
        switch (Team)
        {
            case Team.Red:
                numberText.gameObject.SetActive(visible && !InCover);
                break;
            case Team.Blue:
                numberText.color = visible && !InCover ? Color.red : Color.white;
                break;
            default:
                print($"No Team assigned to {name}");
                break;
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