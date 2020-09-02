using System.Collections.Generic;
using System.Net;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Boid : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;
    float triangeHeight = 1;
    float triangleWidth = 0.5f;

    public LayerMask collisionMask;
    public float maxSpeed;
    public float maxForce;
    public float dampingDistance;
    [Range(0, 10)]
    public float perceptionDistance;
    [Range(0, 360)]
    public float fov;
    float desiredRaySpacing = 10f;
    [Range(0, 2)]
    public float seekForce;
    [Range(0, 2)]
    public float separationForce;
    [Range(0, 2)]
    public float alignmentForce;
    [Range(0, 2)]
    public float cohesionForce;
    public Vector3 velocity;

    Vector3 targetPos;


    GameObject[] boidObjectArray;

    Camera cam;
    float leftConstraint = 0f;
    float rightConstraint = 0f;
    float topConstraint = 0f;
    float bottomConstraint = 0f;
    [Range(0f,5f)]
    float buffer=1f;
    float zDistance = 0f;



    private void Awake()
    {
        GenerateMesh();
    }

    void Start()
    {
        updateBoidList();

        cam = Camera.main;
        zDistance = -cam.transform.position.z;
        leftConstraint = cam.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, zDistance)).x;
        rightConstraint = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, zDistance)).x;
        topConstraint = cam.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, zDistance)).y;
        bottomConstraint = cam.ScreenToWorldPoint(new Vector3(0f, 0.0f, zDistance)).y;



    }

    // Update is called once per frame
    void Update()
    {
        //target coordinates
        Vector3 mouse = Input.mousePosition;
        targetPos = Camera.main.ScreenToWorldPoint(mouse);
        targetPos.z = 0f;
        GameObject[] visibleBoids = findVisibleBoids();

        Vector3 force = seek(targetPos) * seekForce + Separation() * separationForce + Alignment(visibleBoids) * alignmentForce + Cohesion(visibleBoids) * cohesionForce;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit, Mathf.Min(maxSpeed, perceptionDistance), collisionMask)) { 
            Move(AvoidObstacles());
            //.DrawRay(transform.position, transform.up * hit.distance, Color.green);
        } else
        {
            Move(force);
        }
        Wrapping();
    }

    Vector3 seek(Vector3 targetPos)
    {
        float toTargetDistance = Vector3.Distance(targetPos, transform.position);
        Vector3 toTargetDirection = Vector3.Normalize(targetPos - transform.position);
        Vector3 desiredVelocity = (toTargetDistance < dampingDistance) ? toTargetDirection * map(toTargetDistance, 0, dampingDistance, 0, maxSpeed) : toTargetDirection * maxSpeed;
        return Vector3.ClampMagnitude(desiredVelocity - velocity, maxForce); //returns acceleration force
    }

    GameObject[] findVisibleBoids()
    {
        List<GameObject> visibleBoids = new List<GameObject>();
        foreach (GameObject boidObject in boidObjectArray)
        {
            if (!boidObject.Equals(gameObject))
            {
                Vector3 toCurrentBoid = boidObject.transform.position - transform.position;
                float angle = Vector3.Angle(transform.up, toCurrentBoid);
                if (angle <= fov / 2 && Vector3.SqrMagnitude(toCurrentBoid) < Mathf.Pow(perceptionDistance, 2))
                {
                    visibleBoids.Add(boidObject);
                }

            }
        }
        return visibleBoids.ToArray();
    }

    Vector3 Separation()
    {
        Vector3 resultantDirection = Vector3.zero;
        int count = 0;
        foreach (GameObject boidObject in boidObjectArray)
        {
            if (!boidObject.Equals(gameObject))
            {
                Vector3 toCurrentBoid = boidObject.transform.position - transform.position;
                float angle = Vector3.Angle(transform.up, toCurrentBoid);
                if (angle <= fov / 2 && Vector3.SqrMagnitude(toCurrentBoid) < Mathf.Pow(perceptionDistance/2, 2))
                {
                    resultantDirection -= Vector3.Normalize(toCurrentBoid) / Vector3.Distance(boidObject.transform.position, transform.position);
                    count++;
                }

            }
        }
        if (count == 0)
        {
            return Vector3.zero;
        }
        Vector3 targetVelocity = Vector3.Normalize(resultantDirection /count) * maxSpeed;
        return Vector3.ClampMagnitude(targetVelocity - velocity, maxForce);
    }
    
    Vector3 AvoidObstacles()
    {
        int numberOfRays = Mathf.FloorToInt(fov / desiredRaySpacing);

        float raySpacing = (float) fov / numberOfRays;
        Vector3 firstClearDirection = -Vector3.Normalize(velocity)*maxSpeed;
        for(int i = 0; i < numberOfRays/2; i++)
        {
            float angle = i * raySpacing;
            
            Quaternion rotationVector = Quaternion.Euler(0, 0, angle);
            Vector3 testingDirection = rotationVector * transform.up;
            RaycastHit hit;
            Vector3 edge2 = transform.up * -triangeHeight - transform.right * triangleWidth / 2;
            Vector3 edge1 = transform.up * -triangeHeight + transform.right * triangleWidth / 2;
            if (!Physics.Raycast(transform.position,testingDirection,out hit, Mathf.Min(maxSpeed, perceptionDistance), collisionMask))
            {
                firstClearDirection = testingDirection * Vector3.Magnitude(velocity);
                break;
            }
            Debug.DrawRay(transform.position, testingDirection * Mathf.Min(maxSpeed, perceptionDistance), Color.white);
            rotationVector = Quaternion.Euler(0, 0, -angle);
            testingDirection = rotationVector * transform.up;
            if (!Physics.Raycast(transform.position, testingDirection, out hit, Mathf.Min(maxSpeed, perceptionDistance), collisionMask))
            {
                firstClearDirection = testingDirection * Vector3.Magnitude(velocity);
                break;
            }
        
            Debug.DrawRay(transform.position, testingDirection * Mathf.Min(maxSpeed, perceptionDistance), Color.white);
        }

        return Vector3.ClampMagnitude(firstClearDirection - velocity, maxForce);

    }
    
    Vector3 Alignment(GameObject[] boids)
    {
        if (boids.Length == 0)
        {
            return Vector3.zero;
        }
        Vector3 resultantDirection = Vector3.zero;
        foreach (GameObject boid in boids)
        {
            resultantDirection += boid.GetComponent<Boid>().velocity;
        }
        Vector3 targetVelocity = Vector3.Normalize(resultantDirection / boids.Length) * maxSpeed;
        return Vector3.ClampMagnitude(targetVelocity - velocity, maxForce);
    }

    Vector3 Cohesion(GameObject[] boids)
    {
        if (boids.Length == 0)
        {
            return Vector3.zero;
        }
        Vector3 resultantPosition = Vector3.zero;
        foreach (GameObject boid in boids)
        {
            resultantPosition += boid.transform.position;
        }
        return seek(resultantPosition / boids.Length);
    }

    void Move(Vector3 force)
    {
        velocity = Vector3.ClampMagnitude(velocity + force, maxSpeed);

        transform.Translate(velocity * Time.deltaTime, Space.World);
        if (velocity.x != 0 && velocity.y != 0)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            Quaternion desiredRotQ = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, angle-90);
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotQ, Time.deltaTime * 8);
        }
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        vertices = new Vector3[3];
        uv = new Vector2[3];
        triangles = new int[3];

        vertices[0] = new Vector3(0, 0);
        vertices[1] = new Vector3(triangleWidth / 2, -triangeHeight);
        vertices[2] = new Vector3(-triangleWidth / 2, -triangeHeight);
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 1);
        uv[2] = new Vector2(0, 1);

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void updateBoidList()
    {
        boidObjectArray = GameObject.FindGameObjectsWithTag("Boid");
        print("updated");
    }

    float map(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
    }

    void Wrapping()
    {
        Vector3 currentPosition = transform.position;
        Vector3 newPosition = currentPosition;
        if (currentPosition.x < leftConstraint - buffer)
        {
            newPosition.x = rightConstraint + buffer;
        }
        if (currentPosition.x > rightConstraint + buffer)
        {
            newPosition.x = leftConstraint - buffer;
        }
        if (currentPosition.y < bottomConstraint - buffer)
        {
            newPosition.y = topConstraint + buffer;
        }
        if (currentPosition.y > topConstraint + buffer)
        {
            newPosition.y = bottomConstraint - buffer;
        }
        transform.position = newPosition;

    }

}
