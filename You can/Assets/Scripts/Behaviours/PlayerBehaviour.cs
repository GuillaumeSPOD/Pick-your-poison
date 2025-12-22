using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private const float BaseStamina = 20.0f;
    private const int BaseHungerObjective = 1000;
    private float HungerObjective = BaseHungerObjective;

    private static int HungerBar = 200;
    private int hungerRate = 1;

    private const int BaseFatigue = 20 * 60;
    private int MaxFatigue = BaseFatigue;
    private int Fatigue;



    private float MaxStamina = BaseStamina;

    public float walkSpeed = 5f;
    public float crouchSpeed = 2f;
    public float sprintSpeed = 5f;
    public float maxSprintSpeed = 10f;
    public float sprintAcceleration = 1.0f;
    public float stamina = BaseStamina;
    public float sprintStaminaLoss = 3f;
    public float staminaRecovery = 1.5f;

    private float currentSpeed = 6f;

    private float globalSpeedModifier = 1f;

    private bool sprintLimitedByHunger = false;

    private bool CanSprint = true;

    public float mouseSensitivity = 1000f;

    private float verticalRotation = 0f;

    public AudioSource audioSource;
    public AudioClip[] WalkingOnGrassSound;
    public AudioClip[] RunningOnGrassSound;

    public float timeBetweenWalkingSounds = 0.5f;
    public float timeBetweenRunningSounds = 0.3f;

    private float timerBetweenSounds = 0f;

    public Transform playerCamera;

    private bool isMoving = false;
    private bool isWalking = false;

    private bool limpCoroutineRunning;

    private CharacterController controller;

    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    

    private float amplificator = 1f;

    public static bool HasEatenEnough = false;


    //Limp variables

    private const float MovementDelayWhenLimp = 0.25f;
    private float MovementDelayWhenLimpTimer = MovementDelayWhenLimp;
    private const float MoveTimeWhenLimp = 0.25f;
    private float MoveTimeWhenLimpTimer = MovementDelayWhenLimp;

    private Vector3 finalMove;

    private float LimpSpeedMult = 0.5f;

    public static bool IsLimp = false;
    private float LimpMultiplier = 1.5f;


    //Wheelchair variables

    private bool IsInWheelchair = false;
    private Vector3 slopeDir = Vector3.zero;
    private Vector3 inputDir = Vector3.zero;




    void ApplySlopeSliding(Vector3 inputDir)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f))
        {
            float slideAccel = 12f;   
            float damping = 1.5f;      
            float maxSlideSpeed = 15f;

            
            Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;

            
            slopeDir += slideDir * slideAccel * Time.deltaTime;

            
            float oppose = Vector3.Dot(inputDir, slopeDir.normalized);
            if (oppose < 0f)
            {
                
                if (isWalking && isMoving) slopeDir *= 0.98f;
                //slopeDir += inputDir * 5f * Time.deltaTime; 
            }

            
            slopeDir *= Mathf.Clamp01(1f - damping * Time.deltaTime);

            
            if (slopeDir.magnitude > maxSlideSpeed)
                slopeDir = slopeDir.normalized * maxSlideSpeed;

           
            controller.Move(slopeDir * Time.deltaTime);

            float distanceToGround = hit.distance;
            float stickForce = 20f; 
            if (distanceToGround > 0.1f)
            {
                controller.Move(Vector3.down * stickForce * Time.deltaTime);
            }
        }
        else
        {
            
            slopeDir *= Mathf.Clamp01(1f - 8f * Time.deltaTime);
        }
    }





    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        slopeDir = new Vector3();
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        timerBetweenSounds = 0f;

        Fatigue = MaxFatigue;

        StartCoroutine(DecreaseOverTime());

        StartCoroutine(FatigueOverTime());

        Screen.fullScreenMode = FullScreenMode.FullScreenWindow; // ou FullScreenMode.ExclusiveFullScreen
        Screen.fullScreen = true;

    }

    void Update()
    {
        
        LookAround();
        MovePlayer();
    }

    void FixedUpdate()
    {
        if (HungerBar >= HungerObjective)
        {
            HasEatenEnough = true;
            PlayerInteraction.HasEatenEnough = true;
        }
            
    }

    void MovePlayer()
    {
        hungerRate = 1;

        float moveHorizontal = 0f;
        float moveVertical = 0f;
        currentSpeed = walkSpeed * globalSpeedModifier;
        isMoving = false;

        if (Input.GetKey(KeyCode.W))
        {
            moveVertical = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVertical = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveHorizontal = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveHorizontal = 1f;
        }

        isMoving = (moveHorizontal != 0f || moveVertical != 0f);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = crouchSpeed;
            isWalking = true;
            
        }
        else if (Input.GetKey(KeyCode.LeftControl) && stamina > 0 && CanSprint)
        {
            currentSpeed = sprintSpeed;
            isWalking = false;

            if (sprintSpeed < maxSprintSpeed)
                sprintSpeed += sprintAcceleration * Time.deltaTime;
            else
                sprintSpeed = maxSprintSpeed;

            if (!sprintLimitedByHunger)
                stamina -= sprintStaminaLoss * Time.deltaTime;

            else
                hungerRate = 3;
        }
        else
        {
            sprintSpeed = 6f;
            currentSpeed = walkSpeed;
            isWalking = true;

            if (stamina < MaxStamina)
                stamina += staminaRecovery * Time.deltaTime;
        }

        // Appliquer la gravité
        if (controller.isGrounded)
        {
            verticalVelocity = -2f; // pour bien coller au sol
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 horizontalMove = transform.right * moveHorizontal + transform.forward * moveVertical;
        horizontalMove = horizontalMove.normalized * currentSpeed * globalSpeedModifier;

        if (IsInWheelchair)
        {
            inputDir = horizontalMove.normalized; 
            ApplySlopeSliding(inputDir);
        }
        

        finalMove = horizontalMove;
        finalMove.y = verticalVelocity;

        if (!IsLimp || amplificator == 0f)
            controller.Move(finalMove * Time.deltaTime);

        else
        {
            if (!limpCoroutineRunning)
                StartCoroutine(LimpMovementCoroutine());

            
            controller.Move(finalMove * Time.deltaTime * LimpMultiplier * LimpSpeedMult);
        }

        // Sons de pas
        if (timerBetweenSounds > 0f)
            timerBetweenSounds -= Time.deltaTime;

        if (isMoving && timerBetweenSounds <= 0f)
        {
            if (isWalking)
                PlayWalkingSound();
            else
                PlayRunningSound();
        }


        
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void PlayWalkingSound()
    {
        if (WalkingOnGrassSound.Length == 0) return;

        int index = Random.Range(0, WalkingOnGrassSound.Length);
        audioSource.clip = WalkingOnGrassSound[index];
        audioSource.Play();
        timerBetweenSounds = timeBetweenWalkingSounds;
    }

    void PlayRunningSound()
    {
        if (RunningOnGrassSound.Length == 0) return;

        int index = Random.Range(0, RunningOnGrassSound.Length);
        audioSource.clip = RunningOnGrassSound[index];
        audioSource.Play();
        timerBetweenSounds = timeBetweenRunningSounds;
    }

    // === FOOD MANAGING ===
    private System.Collections.IEnumerator DecreaseOverTime()
    {
        while (!HasEatenEnough)
        {
            yield return new WaitForSeconds(1);
            HungerBar -= hungerRate;
            if (HungerBar == 0) Die(1);

        }
    }



    private void Die(int IdOfDeathReason)
    {

    }

    // === FATIGUE MANAGER ===
    private System.Collections.IEnumerator FatigueOverTime()
    {
        while (Fatigue > -1000)
        {
            yield return new WaitForSeconds(1);
            Fatigue--;
            if (Fatigue <= 0)
            {
                globalSpeedModifier = 0.9f * ((BaseFatigue + Fatigue)/BaseFatigue);
            }

        }
    }


    // === LIMP MOVEMENT MANAGER ===
    private System.Collections.IEnumerator LimpMovementDelayCoroutine()
    {
        while (MovementDelayWhenLimpTimer > 0)
        {
            if (isWalking && isMoving)
                yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.05f);

            MovementDelayWhenLimpTimer -= 0.1f;
        }

        MovementDelayWhenLimpTimer = MovementDelayWhenLimp * amplificator;
        MoveTimeWhenLimpTimer = MoveTimeWhenLimp * amplificator;
        LimpSpeedMult = 1f;

        limpCoroutineRunning = false;
    }

    private System.Collections.IEnumerator LimpMovementCoroutine()
    {
        limpCoroutineRunning = true;
        while (MoveTimeWhenLimpTimer > 0)
        {
            if (isWalking && isMoving)
                yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.05f);
            MoveTimeWhenLimpTimer -= 0.1f;
            

        }

        LimpSpeedMult = 0.5f;


        StartCoroutine(LimpMovementDelayCoroutine());
    }




    public void IncreaseSprintSpeed() => maxSprintSpeed *= Mathf.Pow(1.15f, amplificator);
    public void MuchFasterSprint() => maxSprintSpeed = (maxSprintSpeed / 1.15f) * Mathf.Pow(1.35f, amplificator);
    public void ExtremeSprint() => maxSprintSpeed = (maxSprintSpeed / 1.35f) * Mathf.Pow(1.5f, amplificator);

    public void InstantMaxSpeed() => sprintAcceleration = 100f;

    public void MoreEndurance() => MaxStamina = BaseStamina * Mathf.Pow(1.15f, amplificator);
    public void MuchMoreEndurance() => MaxStamina = BaseStamina * Mathf.Pow(1.35f, amplificator);
    public void ExtremeEndurance() => MaxStamina = BaseStamina * Mathf.Pow(1.6f, amplificator);

    public void LessHunger() => HungerObjective = BaseHungerObjective * Mathf.Pow(0.95f, amplificator);
    public void MuchLessHunger() => HungerObjective = BaseHungerObjective * Mathf.Pow(0.90f, amplificator);
    public void HugeLessHunger() => HungerObjective = BaseHungerObjective * Mathf.Pow(0.8f, amplificator);

    //public void HearRabbitsFarther() => rabbitHearingRange = 50f;
    //public void HearRabbitsMuchFarther() => rabbitHearingRange = 70f;

    public void Taller() => transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 1.4f, transform.localScale.z);

    public void SprintUntilFull() => sprintLimitedByHunger = true;

    public void SlowerFatigue() => MaxFatigue = (int)(BaseFatigue * Mathf.Pow(1.2f, amplificator));
    public void NoFatLoss() => hungerRate = 0;

    public void MoreHunger() => HungerObjective = BaseHungerObjective * Mathf.Pow(1.1f, amplificator);
    public void MuchMoreHunger() => HungerObjective = BaseHungerObjective * Mathf.Pow(1.25f, amplificator);
    public void HugeMoreHunger() => HungerObjective = BaseHungerObjective * Mathf.Pow(1.4f, amplificator);

    public void BecomeFat()
    {
        globalSpeedModifier = Mathf.Pow(0.9f, amplificator);
        MaxStamina = BaseStamina * Mathf.Pow(0.9f, amplificator);
    }

    //public void CannotHearFootsteps() => canHearFootsteps = false;
    //public void CannotHearWell() => hearingMultiplier = 0.5f;
    //public void Hallucinations() => hallucinationLevel = 1;
    //public void ManyHallucinations() => hallucinationLevel = 2;
    public void Limp() => IsLimp = true;
    public void Wheels() => IsInWheelchair = true;
    //public void WheelsCreak() => wheelsCreak = true;
    //public void ScareRabbits() => scareRabbits = true;

    public void Smaller() => transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 0.7f, transform.localScale.z);
    //public void NeedToDrink() => needsToDrink = true;

    public void SlowerEnergyRecovery() => staminaRecovery *= Mathf.Pow(0.6f, amplificator);
    public void CannotSprint() => CanSprint = false;
    //public void CanGetHurtWhileRunning() => canGetHurtWhileRunning = true;

    public void FasterFatigue() => MaxFatigue = (int)(BaseFatigue * Mathf.Pow(0.7f, amplificator));
    //public void Blink() => canBlink = true;
    //public void BlinkAndHallucinate() => blinkCausesHallucination = true;
    //public void SeeingMonsterGivesHunger() => seeingMonsterGivesHunger = true;
    //public void HeartAttack() => canHaveHeartAttack = true;

    public static PlayerMovement Instance { get; private set; }
}