using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    public int playerID;
    private InputSystem_Actions _inputSystem;

    [SerializeField] private Camera cam;
    [SerializeField] private Agent selectedAgent;
    
    [SerializeField] private LayerMask layerMask; 


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
        print("Click");
        var ray = cam.ScreenPointToRay(_inputSystem.UI.Point.ReadValue<Vector2>());
        print($"{ray.origin}, {ray.direction}");
        
        var hit = Physics2D.GetRayIntersection(ray, 100, layerMask:layerMask);

        if (hit)
        {
            var agent = hit.transform.GetComponent<Agent>();
            selectedAgent = agent.agentID == playerID ? agent : selectedAgent;
        }
        else selectedAgent = null;

    }

    private void SetLookTargetAgent()
    {
        selectedAgent.SetAgentLookTarget(playerID, GetWorldPointFromScreenPoint());
    }

    private void MoveSelectedAgent()
    {
        if (!selectedAgent) return;
        selectedAgent.SetAgentDestination(playerID, GetWorldPointFromScreenPoint());
    }


    private Vector3 GetWorldPointFromScreenPoint()
    {
        var value = cam.ScreenToWorldPoint(_inputSystem.UI.Point.ReadValue<Vector2>());
        return new Vector3(value.x, value.y, 0);
    }
}
