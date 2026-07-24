using UnityEngine;
using UnityEngine.InputSystem;

public enum Team
{
    None,
    Player1,
    Player2,
}

public class Player : MonoBehaviour
{
    public Team Team => team;
    [SerializeField] private Team team;
    
    private InputSystem_Actions _inputSystem;

    [SerializeField] private Camera cam;
    [SerializeField] private Agent selectedAgent;

    [SerializeField] private LayerMask layerMask;

    private Agent _hoverAgentCache;
    
    //TODO: Hide mechanic

    private void Awake()
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
        if (_inputSystem.UI.Hold.IsPressed() && _inputSystem.UI.Hold.phase == InputActionPhase.Performed) SetAgentLookTarget();
        
        Hover();
    }

    private void Hover()
    {
        var ray = cam.ScreenPointToRay(_inputSystem.UI.Point.ReadValue<Vector2>());

        var hit = Physics2D.GetRayIntersection(ray, 100, layerMask: layerMask);


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
        var ray = cam.ScreenPointToRay(_inputSystem.UI.Point.ReadValue<Vector2>());

        var hit = Physics2D.GetRayIntersection(ray, 100, layerMask: layerMask);

        if (selectedAgent) selectedAgent.SelectedVisuals(false);
        
        if (hit && hit.transform.TryGetComponent(out Agent agent))
        {
            selectedAgent = agent.Team == Team ? agent : selectedAgent;
            selectedAgent?.SelectedVisuals(true);
        }
        else
        {
            selectedAgent = null;
        }
    }

    private void SetAgentLookTarget()
    {
        selectedAgent?.SetAgentLookTarget(Team, GetWorldPointFromScreenPoint());
    }

    private void MoveSelectedAgent()
    {
        if (!selectedAgent) return;
        selectedAgent.SetAgentDestination(Team, GetWorldPointFromScreenPoint());
    }


    private Vector3 GetWorldPointFromScreenPoint()
    {
        var value = cam.ScreenToWorldPoint(_inputSystem.UI.Point.ReadValue<Vector2>());
        return new Vector3(value.x, value.y, 0);
    }
}