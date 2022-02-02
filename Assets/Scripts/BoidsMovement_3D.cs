using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoidsMovement_3D : MonoBehaviour
{
    private Vector3 velocity;

    public string tagObstacle;
    public float desiredSeparation = 0.5f;
    public float maxSpeed = 3;
    public float neighBordist = 1.5f;
    public Vector3 center;
    public float radius = 15f; // Rayons dans lequel doivent rester les agents

    public float weight_Separate = 2; // Poids de la fonction de Séparation
    public float weight_Alignement = 1; // Poids de la fonction d'Alignement

    public float weight_StayInRadius = 0.1f; // Poids de la fonction permettant de rester au centre
    public float weight_SteeredCohesion = 4; // Poids de la fonction de Cohésion
    public float weight_SeparateObstacle = 5; // Poids de la fonction de Séparation d'obstacles
    private float squareAvoidanceRadius;
    public List<RaycastHit> contextHit;
    public List<Transform> context;
    public Vector3[] directions;
    private void Start()
    {
        
        velocity = new Vector3(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
        float squareDesiredSeparation = 1.5f * 1.5f;
        squareAvoidanceRadius = desiredSeparation * desiredSeparation;
         directions = new Vector3[50];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < 50; i++)
        {
            float t = (float)i / 50;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            directions[i] = new Vector3(x, y, z);
        }

    }
    private void Update()
    {
        context = GetNerbyObjects();
        //context = GetNerbyObjects();
        Vector3 sep = Separate(context); // Calcul de la séparation
        Vector3 ali = Align(context); // Calcul de l'alignement
        Vector3 coh = SteeredCohesion(context);// Calcul de la cohésion
        Vector3 stayRadius = StayInRadius();
        Vector3 sepObstacle = Vector3.zero;
        Vector3 obstacle = findUnobstructedDirection();
        sep = normalizeDir(sep, weight_Separate);
        ali = normalizeDir(ali, weight_Alignement);
        coh = normalizeDir(coh, weight_SteeredCohesion);
        stayRadius = normalizeDir(stayRadius, weight_StayInRadius);
        sepObstacle = normalizeDir(sepObstacle, weight_SeparateObstacle);


        velocity = sep + ali + coh + stayRadius + sepObstacle;
        if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
        Move(velocity);
    }

    Vector3 findUnobstructedDirection()
    {
        Vector3 bestDir = transform.forward;
        float furthestObstacle = 0;
        RaycastHit hit;
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(directions[i]);
            if (Physics.SphereCast(transform.position, neighBordist, dir, out hit, 30))
            {
                Debug.DrawRay(transform.position, dir);
                if (hit.distance > furthestObstacle)
                {
                    
                    bestDir = dir;
                    furthestObstacle = hit.distance;
                }
            }
            else
            {
                return dir;
            }
        }
        return bestDir;
    }
    List<Transform> GetNerbyObjects()
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(transform.position, neighBordist);
        RaycastHit[] hitArray;
        var increment = 10;
        /*for (int angle = -20; angle < 200; angle = angle + increment)
        {
            var point = transform.position + Quaternion.Euler(0, 0, angle) * transform.right * radius;
            hitArray = Physics.RaycastAll(transform.position, (point - transform.position).normalized, neighBordist);
            Debug.DrawRay(transform.position, (point - transform.position).normalized, Color.green);

            foreach (RaycastHit hit in hitArray)
            {
                if (hit.collider != null && hit.transform != transform)
                {
                    var countContext = context.Where(go => go.transform == hit.transform);
                    int nb = countContext.Count();
                    if (nb == 0)
                    {
                        context.Add(hit);
                    }
                }
            }
        }*/
        foreach(Collider col in contextColliders)
        {
            context.Add(col.transform);
        }
        return context;


    }

    private Vector3 normalizeDir(Vector3 partialMove, float weight)
    {
        partialMove *= weight;
        if (partialMove != Vector3.zero)
        {
            if (partialMove.sqrMagnitude > weight * weight)
            {
                partialMove.Normalize();
                partialMove *= weight;
            }

        }
        return partialMove;
    }

    /// <summary>
    /// Permet le mouvement
    /// </summary>
    /// <param name="velocity">Mouvement vers lequel se diriger</param>
    public void Move(Vector3 velocity)
    {
        transform.position += velocity * Time.deltaTime;
        transform.up = velocity;
        
    }

    /// <summary>
    /// Fonction de cohésion
    /// </summary>
    /// <param name="context">Liste des agents autour</param>
    /// <returns>Direction de cohésion</returns>
    Vector3 SteeredCohesion(List<Transform> context)
    {
        if (context.Count == 0)
            return Vector3.zero;

        //add all points together and average
        Vector3 cohesionMove = Vector3.zero;
        foreach (Transform item in context)
        {
            if (item.tag != tagObstacle)
            {
                cohesionMove += item.position;
            }
        }
        cohesionMove /= context.Count;

        //create offset from agent position
        cohesionMove -= transform.position;
        cohesionMove = Vector3.SmoothDamp(transform.up, cohesionMove, ref velocity, 0.5f);
        return cohesionMove;
    }
    /// <summary>
    /// Fonction de direction vers le centre
    /// </summary>
    /// <returns>Direction vers le centre</returns>
    Vector3 StayInRadius()
    {
        Vector3 centerOffset = center - transform.position;
        float t = centerOffset.magnitude / radius;
        if (t < 0.9f)
        {
            return Vector3.zero;
        }

        return centerOffset * t * t;
    }
    /// <summary>
    /// Fonction d'alignement
    /// </summary>
    /// <param name="context">Liste des agents autour</param>
    /// <returns>Direction d'Alignement</returns>
    Vector3 Align(List<Transform> context)
    {
        //if no neighbors, maintain current alignment
        if (context.Count == 0)
            return transform.up;

        //add all points together and average
        Vector3 alignmentMove = Vector3.zero;
        foreach (Transform item in context)
        {
            alignmentMove += item.transform.up;
        }
        alignmentMove /= context.Count;

        return alignmentMove;
    }

    /// <summary>
    /// Fonction de séparation
    /// </summary>
    /// <param name="context">Liste des agents autour</param>
    /// <param name="forObstacle">True: Utilisation pour obstacle; False: Utilisation pour agents</param>
    /// <returns>Direction de séparation</returns>
    public Vector3 Separate(List<Transform> context)
    {
        if (context.Count == 0)
            return Vector3.zero;

        //add all points together and average
        Vector3 avoidanceMove = Vector3.zero;
        int nAvoid = 0;
        foreach (Transform item in context)
        {
            if (item.tag != tagObstacle)
                {
                    if (Vector3.SqrMagnitude(item.position - transform.position) < squareAvoidanceRadius)
                    {
                        nAvoid++;
                        avoidanceMove += (transform.position - item.position);
                    }
                }
        }
        if (nAvoid > 0)
        {
            avoidanceMove /= nAvoid;
        }

        return avoidanceMove;

    }

    public Vector3 SeparateObstacle(List<RaycastHit> context)
    {
        if (context.Count == 0)
            return Vector3.zero;

        //add all points together and average
        Vector3 avoidanceMove = Vector3.zero;
        int nAvoid = 0;
        foreach (RaycastHit item in context)
        {
            if (item.transform.tag == tagObstacle)
                {
                 if (Vector3.SqrMagnitude(item.point - transform.position) < squareAvoidanceRadius)
                    {
                        nAvoid++;
                        avoidanceMove += (transform.position - item.point);
                    }
                }
            
        }
        if (nAvoid > 0)
        {
            avoidanceMove /= nAvoid;
        }

        return avoidanceMove;
    }

    public Vector3 Cohesion(List<Transform> context)
    {
        if (context.Count == 0)
            return Vector3.zero;

        //add all points together and average
        Vector3 cohesionMove = Vector3.zero;
        foreach (Transform item in context)
        {
            if (item.tag != tagObstacle)
            {
                cohesionMove += item.position;
            }
        }
        cohesionMove /= context.Count;

        //create offset from agent position
        cohesionMove -= transform.position;
        return cohesionMove;
    }
}
