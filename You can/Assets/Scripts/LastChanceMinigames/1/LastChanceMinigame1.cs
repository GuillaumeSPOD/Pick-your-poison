using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LastChanceMinigame1 : MonoBehaviour
{

    public List<GameObject> ToKeepDuringMinigame;

    public Image GameOverText;
    public Image AnotherTryImage;

    public Image Emoji1;
    public Image Emoji2;

    public Sprite ConfetiSprite;
    public Sprite SadFaceSprite;

    public TextMeshProUGUI ScoreText;
    public GameObject ScoreObject;

    public GameObject GameResultObject;
    public TextMeshProUGUI GameResultText;

    public GameObject Player;
    public Rigidbody2D playerRigidBody;

    public Camera mainCamera;
    public Camera secondaryCamera;

    private bool GameStarted = false;
    private bool GameFinished = false;
    private bool HasFirstClickBeenDone = false;


    public Sprite PrimaryGameOverSprite;
    public Sprite SecondaryGameOverSprite;

    private float Cooldown = 3f;
    private float SpawnCooldown = 1.2f;
    private float MaxSpawnCooldown = 1.5f;

    public Vector3 targetScale = new Vector3(1, 1, 1);

    public GameObject thisMinigame;

    public GameObject Pillar;
    public Canvas canvas;

    public AudioSource audioSource;
    public AudioClip Flap;
    public AudioClip HittingPipe;
    public AudioClip Falling;
    public AudioClip Scoring;

    private int Score = 0;

    private bool Win = false;

    private List<GameObject> MainGameScripts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Utilities.SetMainCamera(secondaryCamera);
        playerRigidBody.simulated = false;
        List<GameObject> ThisGameScript = new List<GameObject> { thisMinigame };
        Utilities.DisableAllScriptsExcept(ThisGameScript);
        Utilities.DisableAllExcept(ToKeepDuringMinigame);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !GameStarted)
        {
            Utilities.ScaleImageOverTime(GameOverText, new Vector3(1, 1, 1), 2.5f);
            Utilities.ScaleImageOverTime(AnotherTryImage, new Vector3(0, 0, 0), 1f);

            GameOverText.sprite = SecondaryGameOverSprite;
            GameStarted = true;

            SpawnCooldown = 1f;
        }
        else if (Input.GetMouseButtonDown(0) && Cooldown == 0 && !GameFinished)
        {
            if (!playerRigidBody.simulated)
            {
                playerRigidBody.simulated = true;
                HasFirstClickBeenDone = true;
                Utilities.ScaleObjectOverTime(ScoreObject, new Vector3(0.6f, 0.6f, 0.6f), 1f);
                Utilities.RotateObject360OverTime(ScoreObject, 0.7f);
            }

            playerRigidBody.linearVelocity = Vector2.up * 70f;
            audioSource.PlayOneShot(Flap);
        }


        if (Cooldown != 0)
        {
            Cooldown -= Time.deltaTime;
            if (Cooldown < 0)
            {
                Cooldown = 0;
            }
        }

        if (SpawnCooldown != 0 && !GameFinished && HasFirstClickBeenDone)
        {
            SpawnCooldown -= Time.deltaTime;
            if (SpawnCooldown < 0)
            {
                SpawnCooldown = MaxSpawnCooldown;
                SpawnPillars();
            }
        }

        if (Input.GetMouseButtonDown(0) && GameFinished)
        {
            if (Win)
            {
                List<GameObject> ThisGameScript = new List<GameObject> { thisMinigame };
                Utilities.EnableAll();
                Utilities.EnableAllScriptsExcept(ThisGameScript);
                Utilities.SetMainCamera(mainCamera);
                
                Destroy(gameObject);
            }
            else
            {
                SceneManager.LoadScene("Main Menu");
            }
        }

    }

    void SpawnPillars()
    {
        int randomValue = Random.Range(0, 51);

        GameObject newPillar = Instantiate(Pillar, canvas.transform);
        newPillar.GetComponent<RectTransform>().anchoredPosition = new Vector2(110, randomValue);
    }

    public void AddScore()
    {
        Score++;
        MaxSpawnCooldown -= 0.05f;
        audioSource.PlayOneShot(Scoring);
        ScoreText.text = $"Score: {Score}/15";
    }

    public void HasDied()
    {
        GameFinished = true;
        DisableAllPillars();
        audioSource.PlayOneShot(HittingPipe);
        Utilities.RotateObject360OverTime(Player, 0.5f);
    }


    public void GameOver()
    {
        Utilities.ScaleImageOverTime(GameOverText, new Vector3(12, 12, 12), 2f);
        StartCoroutine(MoveToCenter(GameOverText.rectTransform, Vector2.zero, 1f));

        playerRigidBody.simulated = false;

        GameOverText.sprite = PrimaryGameOverSprite;

        audioSource.PlayOneShot(Falling);

        DeleteAllPillars();

        if (Score >= 15)
        {
            Win = true;
            GameResultText.text = "You win";
            Emoji1.sprite = ConfetiSprite;
            Emoji2.sprite = ConfetiSprite;
        }
        else
        {
            GameResultText.text = "You lost";
            Emoji1.sprite = SadFaceSprite;
            Emoji2.sprite = SadFaceSprite;
        }

        Utilities.ScaleObjectOverTime(GameResultObject, new Vector3(1, 1, 1), 2f);
        Utilities.RotateObject360OverTime(GameResultObject, 0.5f);
    }

    IEnumerator MoveToCenter(RectTransform rectTransform, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }

    public void DeleteAllPillars()
    {
        GameObject[] pillars = GameObject.FindGameObjectsWithTag("PillarMiddle");

        foreach (GameObject pillar in pillars)
        {
            Destroy(pillar);
        }
    }

    public void DisableAllPillars()
    {
        GameObject[] pillars = GameObject.FindGameObjectsWithTag("PillarMiddle");

        foreach (GameObject pillar in pillars)
        {
            MonoBehaviour[] scripts = pillar.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }
        }
    }
}