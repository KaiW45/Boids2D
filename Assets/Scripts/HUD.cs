using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        Boid currentboid = gameObject.GetComponent<Boid>();
        Vector3 endOne = currentboid.transform.position + new Vector3(currentboid.perceptionDistance * Mathf.Cos(Mathf.Deg2Rad * currentboid.fov / 2), currentboid.perceptionDistance * Mathf.Sin(Mathf.Deg2Rad * currentboid.fov / 2));
        Vector3 endTwo = currentboid.transform.position + new Vector3(currentboid.perceptionDistance * Mathf.Cos(Mathf.Deg2Rad * -currentboid.fov / 2), currentboid.perceptionDistance * Mathf.Sin(Mathf.Deg2Rad * -currentboid.fov / 2));
        Gizmos.DrawLine(currentboid.transform.position, endOne);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(currentboid.transform.position, endTwo);
    }
}
