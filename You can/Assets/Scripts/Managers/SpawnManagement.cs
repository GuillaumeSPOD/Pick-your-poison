using UnityEngine;

public class SpawnManagement : MonoBehaviour
{
    private float TimeUntilNextMonsterSpawn;
    public GameObject Monstre;
    public GameObject joueur;


    void Start()
    {
        // Initialiser la valeur après que le MonoBehaviour ait été créé
        TimeUntilNextMonsterSpawn =  Random.Range(10, 15);
    }

    void Update()
    {
        //TimeUntilNextMonsterSpawn -= Time.deltaTime;
        //if (TimeUntilNextMonsterSpawn < 0) SpawnMonstre();
    }

    void SpawnMonstre()
    {
        Vector3 Position = Utilities.RandomPointAround(joueur.transform.position, 75.0f);
        Instantiate(Monstre, Position, Quaternion.identity);
        TimeUntilNextMonsterSpawn = 200 + Random.Range(0, 60);
    }

    

}
