using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Vector2 difference;
    public Vector2 Difference
    {
        get => difference;
        set => difference = value;
    }

    Vector3 _lastPosition;
    Vector3 _currentWaypoint;
    int _waypointCounter;

    // Start is called before the first frame update
    void Start()
    {
        _waypointCounter = 0;
        _currentWaypoint = waypoints[_waypointCounter].position;
    }

    // Update is called once per frame
    void Update()
    {
        _lastPosition = transform.position;

        transform.position = Vector3.MoveTowards(transform.position, _currentWaypoint, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _currentWaypoint) < 0.1f)
        {
            _waypointCounter++;

            if (_waypointCounter >= waypoints.Length)
            {
                _waypointCounter = 0;
            }

            _currentWaypoint = waypoints[_waypointCounter].position;
        }

        difference = transform.position - _lastPosition;
    }
}
