using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;


public class Agent : MonoBehaviour
{
    private static int _id;
    public readonly int ID = _id++;
    public int number;

    [SerializeField] private int team;
    [SerializeField] private TextMeshPro numberText;
    [SerializeField] private GameObject fovVisual;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform lookTargetTransform;

    private NavMeshAgent _agent;

    private Vector3 _lookTarget;


    public ObservableProperty<bool> isVisible = new(false);
    public int Team => team;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        
        numberText.text = number.ToString();
        numberText.gameObject.SetActive(false);
        fovVisual.SetActive(false);
        lineRenderer.gameObject.SetActive(false);
        lookTargetTransform.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        isVisible.OnChanged.AddListener(UpdateVisibility);
    }

    private void OnDisable()
    {
        isVisible.OnChanged.RemoveListener(UpdateVisibility);
    }

    private void Update()
    {
        UpdateRotation();
        UpdateLookTarget();
    }

    private void UpdateRotation()
    {
        if (_lookTarget == Vector3.zero) return;

        var trgt = transform.InverseTransformPoint(_lookTarget);
        var angle = Mathf.Atan2(trgt.y, trgt.x) * Mathf.Rad2Deg - 90;

        transform.Rotate(0, 0, angle);
    }

    private void UpdateLookTarget()
    {
        if (!lineRenderer) return;
        if (_lookTarget == Vector3.zero) return;
        lookTargetTransform.position = _lookTarget;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, lookTargetTransform.localPosition);
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

    private void UpdateVisibility(bool visible)
    {
        numberText.gameObject.SetActive(visible);
    }

    public void UpdateSelected(bool isSelected)
    {
        fovVisual.SetActive(isSelected);
        lineRenderer.gameObject.SetActive(isSelected);
        lookTargetTransform.gameObject.SetActive(isSelected);
    }
}