using UnityEngine;

public class PathFollow : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints defining the path
    public float moveSpeed = 5f; // Speed at which the object moves
    public float rotationSpeed = 5f; // Speed at which the object rotates

    private int currentWaypointIndex = 0;

    private void Start()
    {
        transform.position = waypoints[0].position;
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints defined. Please assign waypoints in the inspector.");
            return;
        }

        MoveAlongPath();
    }

    /*void MoveAlongPath()
    {
        // Calculate the target rotation only around the Z-axis
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, waypoints[currentWaypointIndex].position - transform.position);
        // Slerp rotation towards the current waypoint with a smaller 't' for smoother rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 0.1f * Time.fixedDeltaTime);
        // Move towards the current waypoint
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, moveSpeed * Time.fixedDeltaTime);



        // Check if the object has reached the current waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
        {
            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

            // If we reached the last waypoint, snap back to the 0th waypoint
            if (currentWaypointIndex == 0)
            {
                transform.position = waypoints[0].position;
            }
        }
    }*/

    void MoveAlongPath()
    {



        if (currentWaypointIndex != 0)
        {
            // Calculate the direction to the current waypoint
            Vector3 directionToWaypoint = waypoints[currentWaypointIndex].position - transform.position;

            // Calculate the target rotation based on the direction to the waypoint
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToWaypoint);

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // Move towards the current waypoint
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, moveSpeed * Time.fixedDeltaTime);

        // Check if the object has reached the current waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
        {
            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

            // If we reached the last waypoint, snap back to the 0th waypoint
            if (currentWaypointIndex == 0)
            {
                transform.position = waypoints[0].position;
            }
        }
    }


    // Draw Gizmos for better visualization in the Unity Editor
    private void OnDrawGizmos()
    {
        DrawWaypointsGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawWaypointsGizmos();
    }

    private void DrawWaypointsGizmos()
    {
        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length; i++)
        {
            // Draw a sphere at each waypoint position
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            // Draw a line between waypoints
            if (i < waypoints.Length - 1)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            else
            {
                // Draw a line from the last waypoint to the first waypoint for looping effect
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
            }
        }
    }
}
