using UnityEngine;
using UnityEngine.AI;

public class RabbitBehaviour : MonoBehaviour
{

    private Vector3 RabbitSpawnPoint;

    private Vector3 Destination;

    private float TimeUntilMove = 10;

    private NavMeshAgent RabbitAgent;

    private Animator m_Animator;

    private bool IsWalking;

    private float TimeUntilStop = 0.2f;



    [SerializeField] private GameObject LapinMort;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RabbitAgent = GetComponent<NavMeshAgent>();
        m_Animator = gameObject.GetComponent<Animator>();

        RabbitSpawnPoint = transform.position;

        IsWalking = false;
    }

    void FixedUpdate()
    {
        if (TimeUntilMove > 0)
        {
            TimeUntilMove -= Time.deltaTime;
            if (TimeUntilMove < 0 )
            {

                Move();
                int randomValue = Random.Range(0, 9);
                TimeUntilMove = 4  + randomValue;
            }
        }

        if (!RabbitAgent.hasPath && !RabbitAgent.pathPending && IsWalking)
        {
            if (TimeUntilStop > 0)
            {
                TimeUntilStop -= Time.deltaTime;
                if (TimeUntilStop < 0)
                {
                    TimeUntilStop = 0.2f;
                    m_Animator.SetBool("IsWalking", false);
                    IsWalking = false;
                }

            }

        }
        else
            TimeUntilStop = 0.2f;


    }

    private void Move()
    {
        IsWalking = true;
        int randomValue = Random.Range(0, 9);
        Destination = Utilities.RandomPointAround(RabbitSpawnPoint, randomValue);
        RabbitAgent.SetDestination(Destination);

        m_Animator.SetBool("IsWalking", true);
    }

    public void Die()
    {
        Instantiate(LapinMort, this.transform.position, this.transform.rotation);
        Destroy(this.gameObject);
    }
}
