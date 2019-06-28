
// Script by Clément Audras 2019 - Game/Level Design Portfolio: www.clementaudras.com

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage the player's input and move the character in the 3D space, smoothly on the geometry.
/// </summary>
public class CharacterController : MonoBehaviour
{
    [Header("Player's Input Properties")]
    public float moveSpeed = 1.8f;                                      // The speed of the controller
    public float rotateSpeed = 180f;                                    // The rotation speed of the controller

    [Space]

    [Header("Controller Properties")]
    public float legRotationWeight = 1f;                                // The weight of rotating the body to each leg
    public float rootPositionSpeed = 5f;                                // The speed of positioning the root
    public float rootRotationSpeed = 30f;                               // The slerp speed of rotating the root to leg heights
    public float height = 0.45f;                                        // Height from ground
    public float scale = 1.0f;                                          // Scale of the character should be 1.0f

    [Header("The constelation's points")]
    public Transform centerTopPoint;                                    // The point above the character where all the internal raycasts starts (green)
    public List<Transform> topPointsCircle = new List<Transform>();     // The list of all the top points circle where all the external raycasts starts (one raycast per point) (blue)
    public List<Transform> btmPointsCircle = new List<Transform>();     // The list of all the bottom points circle
    public List<Transform> groundPoints = new List<Transform>();        // The list of all the ground points circle
    public List<bool> canActivateIntRay = new List<bool>();             // The list of all the bool 

    [Space]

    [Header("Flying Controller Bug Container")]
    public bool canRespawnPlayerIfFlyingControllerBug;                  // If true, teleport the player on a specific position after a delay, when the character is flying
    public List<bool> isGroundPointsNotOnSurface = new List<bool>();    // List of all the ground points that is (false) or not (true) on a surface
    public bool canDebugGroundPointBugName;                             // If true, debug the name of the bugged ground point in the console
    public float respawnDelay = 1.0f;                                   // The time delay before the player is respawned
    [Range(0, 8)] public int sensibility;                               // The minimum number of bugged ground points to respawn the player
    public Transform respawnPoint;                                      // The position where the player should respawn
    private int groudPointsBugedNum = 0;                                // The number of groundpoints not on a surface
    private bool _canRespawnPlayer = true;                              // Condition for respawning the player once per bug

    [Space]

    [Header("Debug & Gizmos")]
    public float raycastRaySize = 1.0f;                                 // The size of each raycast for foot positioning
    public float radius = 0.1f;                                         // The radius of each foots gizmos
    public Color color = Color.cyan;                                    // The color of each gizmos foot
    public Vector3 _cubeSize;                                           // The size of the body gizmo

    private RaycastHit _hit;                                            // The raycast hit for all the raycasts

    void Update()
    {
        if (canRespawnPlayerIfFlyingControllerBug) { FlyingControllerBugContainer(); }
    }

    // FixedUpdate is called every fixed framerate frame (0.02 sec)
    void FixedUpdate()
    {
        AlignCharacterBody();
        GroundPointsPosition();
        MovePlayer();
    }

    // Take player's input and move the controller
    void MovePlayer()
    {
        transform.Translate(0f, 0f, Input.GetAxis("Vertical") * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(0.0f, Input.GetAxis("Horizontal") * rotateSpeed * Time.fixedDeltaTime, 0.0f);
    }

    #region ControllerSystem
    // Method that place all the GroundPoints, on a collider's surface
    void GroundPointsPosition()
    {
        // Loop through all of the points in the sequence of points
        for (int i = 0; i < topPointsCircle.Count; i++)
        {
            Vector3 _rayOffset = btmPointsCircle[i].position - topPointsCircle[i].position;
            Vector3 _rayOffsetOutward = btmPointsCircle[i].position - centerTopPoint.position;

            Debug.DrawRay(centerTopPoint.position, _rayOffsetOutward * raycastRaySize, Color.green);

            if (topPointsCircle[i].GetComponent<ObstructableTopPoint>()._isObstructed == false)
            {
                Debug.DrawRay(topPointsCircle[i].position, _rayOffset * raycastRaySize, Color.cyan);
                if (Physics.Raycast(topPointsCircle[i].position, _rayOffset * raycastRaySize, out _hit, raycastRaySize))
                {
                    Debug.DrawRay(topPointsCircle[i].position, _rayOffset * raycastRaySize, Color.blue);
                    groundPoints[i].position = _hit.point;
                    isGroundPointsNotOnSurface[i] = false;
                }
                else
                {
                    isGroundPointsNotOnSurface[i] = true;
                    groudPointsBugedNum++;

                    if (canDebugGroundPointBugName)
                    {
                        Debug.LogWarning("Foot: " + groundPoints[i].name + " has bugged!");
                    }
                }
            }
            else if (topPointsCircle[i].GetComponent<ObstructableTopPoint>()._isObstructed == true)
            {
                if (Physics.Raycast(centerTopPoint.position, _rayOffsetOutward * raycastRaySize, out _hit, raycastRaySize))
                {
                    Debug.DrawRay(centerTopPoint.position, _rayOffsetOutward * raycastRaySize, Color.magenta);
                    groundPoints[i].position = _hit.point;
                }
            }
        }
    }

    // Rotate and align the character's body toward the average of all the GroundPoints's position and normals
    void AlignCharacterBody()
    {
        // Find the normal of the plane defined by the GroundPoints positions
        Vector3 groundpointsPlaneNormal = GetGroundpointsCirclePlaneNormal();

        // Rotating the root
        Quaternion fromTo = Quaternion.FromToRotation(transform.up, groundpointsPlaneNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, fromTo * transform.rotation, Time.fixedDeltaTime * rootRotationSpeed);

        // Positioning the root
        Vector3 groundpointCentroid = GetGroundpointCentroid();

        Vector3 heightOffset = Vector3.Project((groundpointCentroid + transform.up * height * scale) - transform.position, transform.up);
        transform.position += heightOffset * Time.fixedDeltaTime * (rootPositionSpeed * scale);
    }

    // Calculate the normal of the plane defined by the GroundPoints positions, so we know how to rotate the body
    private Vector3 GetGroundpointsCirclePlaneNormal()
    {
        Vector3 normal = transform.up;

        if (legRotationWeight <= 0f) return normal;

        float legWeight = 1f / Mathf.Lerp(groundPoints.Count, 1f, legRotationWeight);

        // Go through all the groundpoints, rotate the normal by it's offset
        for (int i = 0; i < groundPoints.Count; i++)
        {
            // Direction from the root to the leg
            Vector3 legDirection = groundPoints[i].position - (transform.position - transform.up * height * scale);

            // Find the tangent to transform.up
            Vector3 legTangent = legDirection;
            Vector3 legNormal = transform.up;
            Vector3.OrthoNormalize(ref legNormal, ref legTangent);

            // Find the rotation offset from the tangent to the direction
            // Smooth the character's rotation
            Quaternion fromTo = Quaternion.FromToRotation(legTangent, legDirection);
            fromTo = Quaternion.Lerp(Quaternion.identity, fromTo, legWeight);

            // Rotate the normal
            normal = fromTo * normal;
        }

        return normal;
    }

    // Calculate the normal of the plane defined by GroundPoints positions, so we know how to rotate the character's body
    private Vector3 GetGroundpointCentroid()
    {
        Vector3 position = Vector3.zero;

        float footWeight = 1f / groundPoints.Count;

        // Go through all the groundPoints, rotate the normal by it's offset
        for (int i = 0; i < groundPoints.Count; i++)
        {
            position += groundPoints[i].position * footWeight;
        }

        return position;
    }
    #endregion

    #region FlyingControllerBugContainer
    // A workaround to fix the bug when the character is flying
    void FlyingControllerBugContainer()
    {
        int numBugFoot = count(isGroundPointsNotOnSurface, true);
        int neg = count(isGroundPointsNotOnSurface, false);

        if (numBugFoot >= sensibility)
        {
            Debug.Log("Player has died/respawn due to the raycast not having found a surface");
            _canRespawnPlayer = true;
            StartCoroutine(RespawnPlayerDelay(respawnDelay));
        }
    }

    public static int count(List<bool> list, bool flag)
    {
        int value = 0;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == flag) value++;
        }

        return value;
    }
    
    // here we tp the player at a respawn point
    // in "Code S-44" we kill the player
    void RespawnPlayer()
    {
        transform.position = respawnPoint.position;
        transform.up = respawnPoint.up;
        transform.forward = respawnPoint.forward;
        _canRespawnPlayer = false;
    }


    IEnumerator RespawnPlayerDelay(float time)
    {
        yield return new WaitForSeconds(time);

        if (_canRespawnPlayer) { RespawnPlayer(); }

        yield return null;
    }
    #endregion

    // Show each foot's position
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position, _cubeSize);

        Gizmos.color = Color.white;

        for (int i = 0; i < topPointsCircle.Count; i++)
        {
            Gizmos.DrawWireSphere(topPointsCircle[i].position, 0.1f);
        }
    }
}
