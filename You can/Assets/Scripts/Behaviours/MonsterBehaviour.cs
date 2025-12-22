using UnityEngine;
using UnityEngine.AI; // Nécessaire pour utiliser le NavMeshAgent

public class MonstreAI : MonoBehaviour
{
    public LayerMask obstacleMask;

    private const int MaxSightDistance = 35;
    private const int MaxChaseTime = 20;

    public Animator animator;

    private GameObject joueur; // Référence au joueur, à assigner dans l'inspecteur
    private NavMeshAgent agent; // Composant NavMeshAgent du monstre

    private bool IsFleeing = false;
    private float FleeTime = 0f;
    private bool IsChasing = false;
    private float ChaseTime = 0.0f;

    public AudioSource audioSource;
    public AudioClip audioClip;

    private Renderer objectRenderer;

    private Camera targetCamera; // Reference to the camera (child of GameObject B)

    void Start()
    {

        agent = GetComponent<NavMeshAgent>(); // Récupération du NavMeshAgent attaché à l'objet
        joueur = GameObject.FindWithTag("Player");

        animator.SetBool("IsChasing", false);
        animator.SetBool("IsFleing", false);

        if (joueur == null)
        {

            Debug.LogError("Le joueur n'est pas assigné à " + gameObject.name);
        }
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Debug.LogError("Camera is not assigned.");
        }

        objectRenderer = GetComponent<Renderer>();

        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        float Distance = Vector3.Distance(joueur.transform.position, gameObject.transform.position);

        //Si le monstre est vu, ne cherche pas à fuir ou chasse déjà, il se met à chasser
        if ((IsVisibleFrom(targetCamera) || IsChasing || Distance < 15) && !IsFleeing) Chase();
        
        //Si il ne chasse pas et ne fuis pas, il suit discrétement le joueur
        else if (!IsFleeing) Follow();

        //Si il a fuit et qu'il a atteint sa destination de fuite, il se détruit
        else
        {
            FleeTime += Time.deltaTime;
            if (FleeTime>10)
            {
                Destroy(gameObject);
            }
        }

    }

    void Chase()
    {

        agent.SetDestination(joueur.transform.position);
        agent.speed = 7f;
        if (ChaseTime == 0)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
            animator.SetBool("IsChasing", true);
        }
        ChaseTime += Time.deltaTime;

        
        //se met à fuir à la fin d'une chasse
        if (ChaseTime > MaxChaseTime)
        { 
            IsChasing = false; 
            ChaseTime = 0;
            agent.speed = 20.0f;
            IsFleeing = true;
            animator.SetBool("IsFleing", true);
            GoAway();

        }
        else IsChasing = true;
    }

    void Follow()
    {
        agent.SetDestination(joueur.transform.position);
        agent.speed = 2f;
    }

    void GoAway()
    {
        Vector3 directionAway = (transform.position - joueur.transform.position).normalized;
        
        float maxDistance = 500f; // Distance maximale à tester
        float step = 0.5f; // Distance entre chaque test
        Vector3 bestPoint = transform.position;

        for (float dist = step; dist <= maxDistance; dist += step)
        {
            Vector3 testPoint = transform.position + directionAway * dist;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(testPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                bestPoint = hit.position;
            }
            else
            {
                // On s’arrête si on sort du NavMesh dans cette direction
                break;
            }
        }
        agent.SetDestination(bestPoint);
    }

    bool IsVisibleFrom(Camera cam)
    {
        Renderer objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
            return false;

        // Vérifie la distance entre le joueur et l'objet
        if (Vector3.Distance(joueur.transform.position, transform.position) > MaxSightDistance)
            return false;

        Vector3 camPos = cam.transform.position + Vector3.up * 0.4f; 
        Vector3 targetCenter = objectRenderer.bounds.center;
        Vector3 dirToTarget = targetCenter - camPos;

        // Vérifie que l'objet est dans le champ de vision de la caméra
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        if (!GeometryUtility.TestPlanesAABB(planes, objectRenderer.bounds))
            return false;

        // Vérifie s'il y a un obstacle entre la caméra et l'objet
        if (Physics.Raycast(camPos, dirToTarget.normalized, out RaycastHit hit, dirToTarget.magnitude, obstacleMask))
        {
            // Si on touche un objet autre que soi-même, la visibilité est bloquée
            if (hit.transform.gameObject != gameObject)
                return false;
        }

        return true;
    }


}
