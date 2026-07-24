using System;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    public int playerID;
    private InputSystem_Actions _inputSystem;

    [SerializeField] private Agent _selectedAgent;
    
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
        if (_inputSystem.UI.Click.WasPressedThisFrame()) SelectAgent();
        if (_inputSystem.UI.RightClick.WasPressedThisFrame()) MoveSelectedAgent();
        

    }


    private void SelectAgent()
    {
        print("Click");
        var ray = Camera.main.ScreenPointToRay(_inputSystem.UI.Point.ReadValue<Vector2>());
        print($"{ray.origin}, {ray.direction}");
        
        var hit = Physics2D.GetRayIntersection(ray, 100, layerMask:layerMask);

        if (hit)
        {
            var agent = hit.transform.GetComponent<Agent>();
            _selectedAgent = agent.agentID == playerID ? agent : _selectedAgent;
            print($"Selected agent {_selectedAgent.name}");
        }
        else _selectedAgent = null;

    }

    private void MoveSelectedAgent()
    {
        if (!_selectedAgent) return;
        var value = Camera.main.ScreenToWorldPoint(_inputSystem.UI.Point.ReadValue<Vector2>());
        value = new Vector3(value.x, value.y, 0);
        _selectedAgent.SetAgentDestination(playerID, value);
    }
}
