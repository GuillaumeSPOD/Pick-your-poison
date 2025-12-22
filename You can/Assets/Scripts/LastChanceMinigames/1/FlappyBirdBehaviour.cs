using UnityEngine;

public class FlappyBirdBehaviour : MonoBehaviour
{
    public LastChanceMinigame1 MainMiniGameScript;

    private bool Died = false;
    private float DeathTimer = 0.8f;

    private bool GameOverCodeExecuted = false;
    private bool HasDiedCodeExecuted = false;

    void Start()
    {
        MainMiniGameScript = GameObject.FindGameObjectWithTag("Minigame").GetComponent<LastChanceMinigame1>();
    }

    void Update()
    { 
        if (Died && DeathTimer != 0 )
        {
            if (!HasDiedCodeExecuted) MainMiniGameScript.HasDied();

            DeathTimer -= Time.deltaTime;
            if (DeathTimer < 0f)
            {
                DeathTimer = 0f;
            }

            HasDiedCodeExecuted = true;
        }

        if (Died && DeathTimer ==0 && !GameOverCodeExecuted)
        {
            MainMiniGameScript.GameOver();
            GameOverCodeExecuted = true;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PillarExtremes"))
        {
            Died = true;
            
        }
            

        else
            MainMiniGameScript.AddScore();
        
    }
}
