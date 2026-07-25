using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private Agent agent;

    public float viewRadius;
    [Range(0, 360)] public float viewAngle = 90f;

    [SerializeField] private float searchDelay = .2f;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    /// <summary>
    /// Refers to Agents whose number is visible.
    /// </summary>
    [HideInInspector] public List<Agent> visibleTargets = new();

    private List<Agent> _visibleTargetsOld = new();

    public UnityEvent<Agent, bool> AgentInViewEvent = new();

    [SerializeField] private float meshResolution;
    [SerializeField] private MeshFilter meshFilter;
    private Mesh _mesh;

    private CancellationTokenSource _targetingToken;

    private void Awake()
    {
        _mesh = new Mesh();
        _mesh.name = "FieldOfView";
        meshFilter.mesh = _mesh;
    }

    private void OnEnable()
    {
        _targetingToken = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _targetingToken.Cancel();
        _targetingToken.Dispose();
        visibleTargets.ForEach(t => t.IsVisible.Value = false);
        AgentInViewEvent.Invoke(agent, false);
    }

    private void Start()
    {
        FindTargetsWithDelay(searchDelay, _targetingToken.Token).Forget();
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    private async UniTask FindTargetsWithDelay(float delay, CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested == false)
        {
            await UniTask.WaitForSeconds(delay, cancellationToken: cancellationToken);
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        _visibleTargetsOld.Clear();
        _visibleTargetsOld.AddRange(visibleTargets);
        visibleTargets.Clear();
        Collider2D[] targetColliders = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);


        for (int i = 0; i < targetColliders.Length; i++)
        {
            var target = targetColliders[i].transform;
            var direction = (target.position - transform.position).normalized;
            var angle = Vector3.Angle(agent.rotationPivotTransform.up, direction);

            if (!(angle < viewAngle / 2)) continue;
            var distance = Vector2.Distance(transform.position, target.position);
            if (Physics2D.Raycast(transform.position, direction, distance, obstacleMask)) continue;
            if (!target.TryGetComponent(out Agent otherAgent)) continue;
            if (agent.ID != otherAgent.ID && agent.Team != otherAgent.Team)
            {
                var ang = Vector3.Angle(agent.rotationPivotTransform.up, otherAgent.rotationPivotTransform.up);
                if (ang < 180 - viewAngle / 2) continue;
                otherAgent.IsVisible.Value = true;
                AgentInViewEvent.Invoke(otherAgent, true);
                visibleTargets.Add(otherAgent);
            }
        }

        foreach (var target in _visibleTargetsOld.Where(target => !visibleTargets.Contains(target)))
        {
            target.IsVisible.Value = false;
            AgentInViewEvent.Invoke(target, false);
        }
    }

    private void DrawFieldOfView()
    {
        var rayCount = Mathf.RoundToInt(meshResolution * viewAngle);
        var rayAngleSize = viewAngle / rayCount;
        List<Vector3> viewPoints = new();
        for (var i = 0; i <= rayCount; i++)
        {
            var angle = agent.rotationPivotTransform.eulerAngles.z - viewAngle / 2 + rayAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(-angle);
            viewPoints.Add(newViewCast.point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = agent.rotationPivotTransform.InverseTransformPoint(viewPoints[i]);
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees -= agent.rotationPivotTransform.eulerAngles.z;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = DirFromAngle(globalAngle, true);


        var hit = Physics2D.Raycast(transform.position, direction, viewRadius, obstacleMask);
        return hit
            ? new ViewCastInfo(true, hit.point, hit.distance, globalAngle)
            : new ViewCastInfo(false, transform.position + direction * viewRadius, viewRadius, globalAngle);
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }
}