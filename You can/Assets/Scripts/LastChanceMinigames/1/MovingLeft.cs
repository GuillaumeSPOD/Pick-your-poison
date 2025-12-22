using UnityEngine;

public class MovingLeft : MonoBehaviour
{

    public float MoveSpeed = 15f;


    void Update()
    {
        transform.position += Vector3.left * MoveSpeed * Time.deltaTime;
        if (transform.position.x < -110) Destroy(gameObject);
    }

    
}
