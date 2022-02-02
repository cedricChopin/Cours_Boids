using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsMovement : MonoBehaviour
{
    private SpawnBoids spawn;
    private Rigidbody2D rb;
    public float desiredSeparation = 1.0f;
    public float desiredSeparation_obs = 3.0f;
    public float maxSpeed = 3;
    public float maxForce = 0.05f;
    public float neighBordist = 50f;
    private GameObject[] obstacles;
    private Vector3 previousPos;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawn = GameObject.Find("Spawn").GetComponent<SpawnBoids>();
        rb.velocity = new Vector2(Random.Range(0, 2), Random.Range(0, 2));
        previousPos = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 sep = Separate(); // Calcul de la séparation
        Vector2 sep_obs = Separate_Obstacles(); // Calcul de la séparation
        Vector2 ali = Align(); // Calcul de l'alignement
        Vector2 coh = Cohesion(); // Calcul de la cohésion
        sep = sep * 1.5f;
        sep_obs = sep_obs * 3f;
        rb.velocity += sep + ali + coh + sep_obs; // On ajoute tout les calculs à la velocité
        Vector2 target = ((Vector2)transform.position - (Vector2)previousPos).normalized; // Prochaine position
        float desiredAngle = Vector2.Angle(target, Vector2.up); // Angle calculé afin d'orienter le boid (Optionnel)
        Vector3 rotation = new Vector3();
        rotation.z = desiredAngle; // Angle affecté à la rotation Z ( rotation d'un sprite)

        if (transform.position.x < -18)
        {
            transform.position = new Vector3(transform.position.x + 36, transform.position.y, 0);
        }
        if (transform.position.x > 18)
        {
            transform.position = new Vector3(transform.position.x - 36, transform.position.y, 0);
        }

        if (transform.position.y > 11)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 22, 0);
        }

        if (transform.position.y < -11)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 22, 0);
        }
        //transform.rotation = Quaternion.Euler(rotation);
        transform.up = rb.velocity;
        previousPos = transform.position;
    }

    Vector2 Separate_Obstacles()
    {
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        Vector2 steer = Vector2.zero;
        int count = 0;

        for (int i = 0; i < obstacles.Length; i++)
        {
            float d = Vector2.Distance(this.transform.position, obstacles[i].transform.position);
            if ((d > 0) && (d < desiredSeparation_obs))
            {
                Vector2 diff = transform.position - obstacles[i].transform.position;
                diff.Normalize();
                diff = diff / d;
                steer += diff;
                count++;
            }
        }
        if (count > 0)
        {
            steer /= count;
        }

        if (steer.magnitude > 0)
        {
            steer.Normalize();
            steer = steer * maxSpeed;
            steer = steer - rb.velocity;
            steer = Vector2.ClampMagnitude(steer, maxForce);
        }

        return steer;
    }

    Vector2 Separate()
    {
        Vector2 steer = Vector2.zero;
        int count = 0;

        for (int i = 0; i < spawn.allBoids.Count; i++)
        {
            float d = Vector2.Distance(this.transform.position, spawn.allBoids[i].transform.position);
            if ((d > 0) && (d < desiredSeparation))
            {
                Vector2 diff = transform.position - spawn.allBoids[i].transform.position;
                diff.Normalize();
                diff = diff / d;
                steer += diff;
                count++;
            }
        }
        if (count > 0)
        {
            steer /= count;
        }

        if (steer.magnitude > 0)
        {
            steer.Normalize();
            steer = steer * maxSpeed;
            steer = steer - rb.velocity;
            steer = Vector2.ClampMagnitude(steer, maxForce);
        }

        return steer;
    }

    Vector2 Align()
    {

        Vector2 sum = Vector2.zero;
        float count = 0;

        for (int i = 0; i < spawn.allBoids.Count; i++)
        {
            float d = Vector2.Distance(this.transform.position, spawn.allBoids[i].transform.position);

            if ((d > 0) && (d < neighBordist))
            {
                sum += (spawn.allBoids[i].GetComponent<Rigidbody2D>().velocity);
                count++;
            }

        }

        if (count > 0)
        {

            sum /= count;
            sum.Normalize();
            sum *= maxSpeed;
            Vector2 steer = sum - rb.velocity;
            steer = Vector2.ClampMagnitude(steer, maxForce);
            return steer;
        }
        else
        {
            return Vector2.zero;
        }


    }

    Vector2 Cohesion()
    {
        Vector2 sum = Vector2.zero;
        float count = 0;
        for (int i = 0; i < spawn.allBoids.Count; i++)
        {
            float d = Vector2.Distance(this.transform.position, (Vector2)spawn.allBoids[i].transform.position);
            if ((d > 0) && (d < neighBordist))
            {
                sum += (Vector2)spawn.allBoids[i].transform.position;
                count++;

            }
        }
        if (count > 0)
        {
            sum /= count;
            return Seek(sum);
        }
        else
        {
            return Vector2.zero;
        }

    }

    Vector2 Seek(Vector2 target)
    {
        Vector2 desired = target - (Vector2)transform.position;
        desired.Normalize();
        desired *= maxSpeed;
        Vector2 steer = desired - rb.velocity;
        steer = Vector2.ClampMagnitude(steer, maxForce);
        return steer;
    }
}
