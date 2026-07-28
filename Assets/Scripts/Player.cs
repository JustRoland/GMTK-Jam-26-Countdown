using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Team
{
    None,
    Blue,
    Red,
}

public class Player : MonoBehaviour
{
    [SerializeField] private Team team;

    private InputSystem_Actions _inputSystem;

    [SerializeField] private Camera cam;
    [SerializeField] private Agent selectedAgent;

    [SerializeField] private LayerMask layerMask;

    private Agent _hoverAgentCache;

    //TODO: Hide mechanic

    public void Awake()
    {
        _inputSystem = new InputSystem_Actions();
        _inputSystem.UI.Enable();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (_inputSystem.UI.Click.WasPerformedThisFrame()) SelectAgent();
        if (_inputSystem.UI.RightClick.WasPressedThisFrame()) MoveSelectedAgent();
        if (_inputSystem.UI.Hold.IsPressed() && _inputSystem.UI.Hold.phase == InputActionPhase.Performed)
            SetAgentLookTarget();

        Hover();
    }

    private void Hover()
    {
        var hit = ShootRayFromScreenPoint();

        if (CoverManager.Instance)
        {
            if (selectedAgent)
                CoverManager.Instance.ReturnNearestCoverPosition(GetWorldPointFromScreenPoint(), drawLine: true);
            else CoverManager.Instance.EnableLineRenderer(false);
        }

        if (hit && hit.transform.TryGetComponent(out Agent agent))
        {
            if (agent == selectedAgent) return;
            agent.HoverVisuals(true);
            _hoverAgentCache = agent;
        }
        else
        {
            if (_hoverAgentCache == selectedAgent) return;
            _hoverAgentCache?.HoverVisuals(false);
        }
    }

    private void SelectAgent()
    {
        var hit = ShootRayFromScreenPoint();


        if (!hit || !hit.transform.TryGetComponent(out Agent agent))
        {
            if (selectedAgent) selectedAgent.SelectedVisuals(false);
            selectedAgent = null;
            return;
        }

        if (agent.Team == team)
        {
            if (selectedAgent) selectedAgent.SelectedVisuals(false);
            selectedAgent = agent;
            selectedAgent.SelectedVisuals(true);
        }
        else if (agent.IsVisible.Value)
        {
            agent.Eliminate(team);
        }
    }

    private void SetAgentLookTarget()
    {
        selectedAgent?.SetAgentLookTarget(team, GetWorldPointFromScreenPoint());
    }

    private void MoveSelectedAgent()
    {
        if (!selectedAgent) return;
        selectedAgent.SetAgentDestination(team, GetWorldPointFromScreenPoint());
    }

    private RaycastHit2D ShootRayFromScreenPoint()
    {
        var ray = cam.ScreenPointToRay(_inputSystem.UI.Point.ReadValue<Vector2>());

        return Physics2D.GetRayIntersection(ray, 100, layerMask: layerMask);
    }

    private Vector3 GetWorldPointFromScreenPoint()
    {
        var value = cam.ScreenToWorldPoint(_inputSystem.UI.Point.ReadValue<Vector2>());
        return new Vector3(value.x, value.y, 0);
    }
}