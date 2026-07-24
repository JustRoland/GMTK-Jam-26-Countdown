using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [SerializeField] private int team;
    private InputSystem_Actions _inputSystem;

    [SerializeField] private Camera cam;
    [SerializeField] private Agent selectedAgent;

    [SerializeField] private LayerMask layerMask;

    public int Team => team;

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
        if (_inputSystem.UI.Click.WasPressedThisFrame()) HandleClick();
        if (_inputSystem.UI.RightClick.WasPressedThisFrame()) MoveSelectedAgent();
    }

    private void HandleClick()
    {
        if (!selectedAgent) SelectAgent();
        else SetLookTargetAgent();
    }

    private void SelectAgent()
    {
        var ray = cam.ScreenPointToRay(_inputSystem.UI.Point.ReadValue<Vector2>());

        var hit = Physics2D.GetRayIntersection(ray, 100, layerMask: layerMask);

        if (selectedAgent) selectedAgent.UpdateSelected(false);
        
        if (hit)
        {
            var agent = hit.transform.GetComponent<Agent>();
            Debug.Assert(agent, $"Agent Component is missing from {hit.transform.name}");
            selectedAgent = agent.Team == Team ? agent : selectedAgent;
            selectedAgent?.UpdateSelected(true);
        }
        else
        {
            selectedAgent = null;
        }
    }

    private void SetLookTargetAgent()
    {
        selectedAgent.SetAgentLookTarget(Team, GetWorldPointFromScreenPoint());
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