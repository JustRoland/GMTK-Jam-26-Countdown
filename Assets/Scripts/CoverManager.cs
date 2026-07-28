using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CoverManager : MonoBehaviour
{
    public static CoverManager Instance;
    private LineRenderer _lineRenderer;
    private List<Collider2D> _covers;
    [SerializeField] private int coverLayer;
    [SerializeField] private float maxSearchDistance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        _lineRenderer = GetComponent<LineRenderer>();
        
        //Find all Transforms on the "cover layer"
        _covers = FindObjectsByType<Collider2D>(sortMode: FindObjectsSortMode.None).Where(c => c.gameObject.layer == coverLayer).ToList();
    }

    public (Vector3, float) ReturnNearestCoverPosition(Vector2 point, bool drawLine = false)
    {
        var distances = _covers.Select(c => Vector2.Distance(c.ClosestPoint(point), point)).ToList();
        var indexOfMin = distances.IndexOfMin();
        if (distances[indexOfMin] > maxSearchDistance)
        {
            if (drawLine) _lineRenderer.enabled = false;
            return (Vector3.zero, 0f);
        }
        var closestPoint = _covers[indexOfMin].ClosestPoint(point);
        if (drawLine)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, point);
            _lineRenderer.SetPosition(1, closestPoint);
        }
        return (closestPoint, distances[indexOfMin]);
    }

    public void EnableLineRenderer(bool enable)
    {
        _lineRenderer.enabled = enable;
    }
}
