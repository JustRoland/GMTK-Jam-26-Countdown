using System;
using UnityEngine;

public class FlagObject : MonoBehaviour
{
    public Team team;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Agent agent)) return;
        if (agent.Team == team)
        {
            if (!LocationMarkerManager.Instance.CheckFlagHolder(agent)) return;
            
            LocationMarkerManager.Instance.ReturnFlag(agent);
            GameManager.Instance.WinGame(agent.Team);

        }
        else
        {
            LocationMarkerManager.Instance.GrabFlag(agent);
        }
    }
}