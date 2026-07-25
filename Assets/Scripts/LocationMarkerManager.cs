using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

public class LocationMarkerManager : MonoBehaviour
{
    public static LocationMarkerManager Instance;
    
    private readonly List<LocationMarker> _locationMarkers = new();
    [Required]
    [SerializeField] private List<Transform> _flagTransforms = new(); 
    private readonly List<Flag> _flags = new(); 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        for (int i = 0; i < transform.childCount; i++)
        {
            _locationMarkers.Add(new LocationMarker(transform.GetChild(i)));
        }
        
        //Hardcoded flag assignment
        _flags.Add(new Flag(Team.Player1, _flagTransforms[0]));
        _flags.Add(new Flag(Team.Player2, _flagTransforms[1]));
    }

    public Transform RequestLocationMarker()
    {
        var filteredMarker = _locationMarkers.Where(lm => !lm.Reserved).ToList();
        var rand = new Random();
        var marker = filteredMarker[rand.Next(filteredMarker.Count)];
        if (marker != null) marker.Reserved = true;
        else print("No empty location marker");
        return marker?.Transform;
    }

    public void ReturnLocationMarker(Transform locationMarker)
    {
        _locationMarkers.Find(lm => lm.Transform == locationMarker).Reserved = false;
    }

    public Flag RequestFlag(Team team)
    {
        return _flags.Find(f => f.Team == team);
    }
}

public class LocationMarker: IEquatable<LocationMarker>
{
    public LocationMarker(Transform transform)
    {
        Transform = transform;
        Reserved = false;
    }
    
    public bool Reserved;
    public readonly Transform Transform;

    public bool Equals(LocationMarker other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(Transform, other.Transform);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LocationMarker)obj);
    }

    public override int GetHashCode()
    {
        return (Transform != null ? Transform.GetHashCode() : 0);
    }
}

public class Flag: IEquatable<Flag>
{
    public Flag(Team team, Transform transform)
    {
        Team = team;
        Transform = transform;
        Taken = false;
    }
    
    public bool Taken;
    public readonly Team Team;
    public readonly Transform Transform;

    public bool Equals(Flag other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(Transform, other.Transform);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Flag)obj);
    }

    public override int GetHashCode()
    {
        return (Transform != null ? Transform.GetHashCode() : 0);
    }
}


