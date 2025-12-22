using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class PlayerInteraction : MonoBehaviour
{
    // === ENUMS ===
    private enum ObjectType
    {
        None = 0,
        Hache = 1,
        Planche = 2,
        Pierre = 3,
        Baton = 4,
        Arbre = 5,
        Barricade = 6,
        LapinVivant = 7,
        LapinMort = 8,
        LapinCuit=9,
        Lit = 100,
        Feu = 101,
        Anything = 200
    }

    // === STRUCTURE DE CRAFT ===
    private class CraftRecipe
    {
        public ObjectType Result;
        public ObjectType InHand;
        public int Quantity;
        public ObjectType Target;
        public string Message;

        public CraftRecipe(ObjectType result, ObjectType inHand, int quantity, ObjectType target, string message)
        {
            Result = result;
            InHand = inHand;
            Quantity = quantity;
            Target = target;
            Message = message;
        }
    }

    // === STRUCTURE D'INTERACTIONS CUSTOM ===
    private class CustomInteraction
    {
        public Action ResultingFunction;
        public ObjectType InHand;
        public int Quantity;
        public ObjectType Target;
        public string Message;

        public CustomInteraction(Action resultingfunction, ObjectType inHand, int quantity, ObjectType target, string message)
        {
            ResultingFunction = resultingfunction;
            InHand = inHand;
            Quantity = quantity;
            Target = target;
            Message = message;
        }
    }

    // === SERIALIZED FIELDS ===
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private TMP_Text CrosshairText;

    [Header("Prefabs")]
    [SerializeField] private GameObject Hache;
    [SerializeField] private GameObject Planche;
    [SerializeField] private GameObject Pierre;
    [SerializeField] private GameObject Baton;
    [SerializeField] private GameObject LapinMort;
    [SerializeField] private GameObject Barricade;
    [SerializeField] private GameObject BarricadeBlueprint;


    [SerializeField] private GameObject HacheInHand;
    [SerializeField] private GameObject PlancheInHand;
    [SerializeField] private GameObject PierreInHand;
    [SerializeField] private GameObject BatonInHand;
    [SerializeField] private GameObject LapinMortInHand;

    [SerializeField] private TreeManager TreeScript;

    [SerializeField] private PowerUpInitializer powerUpInitializer;

    // === INTERNE ===
    private Dictionary<string, ObjectType> tagToType;
    private List<CraftRecipe> recipes;
    private List<CustomInteraction> Interactions;

    private List<ObjectType> UnPickables;

    private ObjectType heldItem = ObjectType.None;
    private int heldQuantity = 0;
    private int heldDurability = 0;

    private GameObject hoverObject = null;
    private ObjectType hoverType = ObjectType.None;
    private int hoverDurability = 0;

    private GameObject blueprint;
    private bool blueprintSpawned = false;

    

    private Action CurrentInteraction;

    private GameObject HeldGameObjectModel;


    private int toolDurabilityMultiplier = 1;

    private float toolUseSpeed = 1f;
    private float actionSpeedMultiplier = 1f;
    private float axeSpeedMultiplier = 1f;

    private bool InDelay = false;

    private float monsterDamageMultiplier = 1f;

    private float rareDropMultiplier = 1f;

    private int MaxSyringueCount = 3;
    private int SyringeCount;

    private bool canGetSick = false;

    private bool actionsGiveHunger = false;

    private float amplificator = 1f;

    public static bool HasEatenEnough = false;



    // === INIT ===
    void Start()
    {
        Application.targetFrameRate = 100;
        tooltipText.text = "";

        SyringeCount = MaxSyringueCount;

        tagToType = new Dictionary<string, ObjectType>
        {
            { "Hache", ObjectType.Hache },
            { "Planche", ObjectType.Planche },
            { "Pierre", ObjectType.Pierre },
            { "Baton", ObjectType.Baton },
            { "Arbre", ObjectType.Arbre },
            { "Barricade", ObjectType.Barricade },
            { "Lit", ObjectType.Lit },
            { "LapinVivant", ObjectType.LapinVivant },
            { "LapinMort", ObjectType.LapinMort },
            { "Feu", ObjectType.Feu }
        };

        UnPickables = new List<ObjectType>
        {
            ObjectType.Arbre,
            ObjectType.Barricade,
            ObjectType.Lit,
            ObjectType.LapinVivant,
            ObjectType.Feu
        };


        recipes = new List<CraftRecipe>
        {
            new CraftRecipe(ObjectType.Hache, ObjectType.Pierre, 1, ObjectType.Baton, "Faire une hache"),
            new CraftRecipe(ObjectType.Barricade, ObjectType.Planche, 3, ObjectType.Planche, "Faire une barricade"),
            
            
        };

        //(CalledFunction, heldItem, numberOfHeldItems, lookingAtObject, Message)
        Interactions = new List<CustomInteraction>
        {
            new CustomInteraction(CutPlank, ObjectType.Hache, 1, ObjectType.Planche, "Couper la planche en bâtons"),
            new CustomInteraction(Sleep, ObjectType.Anything, 1, ObjectType.Lit, "Dormir"),
            new CustomInteraction(KillRabbit, ObjectType.Hache, 1, ObjectType.LapinVivant, "Tuer le lapin"),
            new CustomInteraction(CutTree, ObjectType.Hache, 1, ObjectType.Arbre, "Couper l'arbre"),
            new CustomInteraction(CookRabbit, ObjectType.LapinMort, 1, ObjectType.Feu, "Faire cuire le lapin"),
            new CustomInteraction(EatRabbit, ObjectType.LapinCuit, 1, ObjectType.Anything, "Manger le lapin"),
        };

        
    }

    // === UPDATE ===
    void Update()
    {
        DetectHoverObject();
        UpdateTooltipMessage();

        if (Input.GetMouseButtonUp(0) && !InDelay)
        {
            TryInteract();
        }

        if (Input.GetMouseButtonUp(1) && !InDelay)
        {
            TryDropItem();
        }

        if ((int)heldItem == 6)
        {
            if (blueprintSpawned)
                UpdateBlueprint();
        }

    }

    

    // === RAYCAST DETECTION ===
    void DetectHoverObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, 3.5f))
        {
            hoverObject = hit.collider.gameObject;

            if (hoverObject != null)
            {
                tagToType.TryGetValue(hoverObject.tag, out hoverType);

                if (hoverObject.TryGetComponent(out Durability d))
                    hoverDurability = d.durability;
            }
            else
            {
                hoverType = ObjectType.None;
                tooltipText.text = "";
            }
        }
        else
        {
            hoverObject = null;
            hoverType = ObjectType.None;
            tooltipText.text = "";
        }
    }

    // === AFFICHAGE MESSAGES ===
    void UpdateTooltipMessage()
    {
        foreach (var recipe in recipes)
        {
            if (heldItem == recipe.InHand &&
                heldQuantity == recipe.Quantity &&
                hoverType == recipe.Target)
            {
                tooltipText.text = recipe.Message;
                return;
            }
        }


        foreach (var action in Interactions)
        {
            if (
                hoverType == action.Target &&
                    (
                        (heldItem == action.InHand && heldQuantity >= action.Quantity) ||
                        (action.InHand == ObjectType.Anything)
                    )
                )
            {
                tooltipText.text = action.Message;
                return;
            }
        }

        //Si le joueur n'a pas d'objet dans les mains, ou que l'objet qu'il regarde est du même type que celui qu'il a dans les mains et est prenable, alors il peut le prendre
        if ((hoverType != ObjectType.None && (heldItem == ObjectType.None || (heldItem == hoverType && heldQuantity < 3))) && !UnPickables.Contains(hoverType))     
        {
            tooltipText.text = $"Prendre {hoverType}";
        }
        else
        {
            tooltipText.text = "";
        }
    }

    // === INTERACTIONS ===
    void TryInteract()
    {
        if (hoverType == ObjectType.None)
            return;

        // Essayons un craft
        foreach (var recipe in recipes)
        {
            if (heldItem == recipe.InHand &&
                heldQuantity >= recipe.Quantity &&
                hoverType == recipe.Target)
            {
                ExecuteCraft(recipe);
                return;
            }
        }
        

        // Sinon : ramasser l'objet
        if ((heldItem == ObjectType.None || (heldItem == hoverType && heldQuantity < 3)) && !UnPickables.Contains(hoverType))
        {
            PickUp();
            return;
        }

        // Essayons une interaction
        foreach (var action in Interactions)
        {
            if (
                hoverType == action.Target &&
                    (
                        (heldItem == action.InHand && heldQuantity >= action.Quantity) ||
                        (action.InHand == ObjectType.Anything)
                    )
                )
            {
                if (heldItem == ObjectType.Hache)
                {
                    heldDurability--;
                    if (heldDurability <= 0)
                    {
                        heldItem = ObjectType.None;
                        heldQuantity = 0;
                        UpdateItemInHand();
                    }
                    
                }
                

                CurrentInteraction = action.ResultingFunction;
                CurrentInteraction?.Invoke();
                return;
            }
        }
        
    }

    private System.Collections.IEnumerator ResetCrosshairText()
    {
        int seconds = 4;
        while (--seconds != 0)
        {
            yield return new WaitForSeconds(1);

        }

        CrosshairText.text = "";
    }

    void Sleep()
    {
        HasEatenEnough = true;
        if (HasEatenEnough) 
        {
            CrosshairText.text = "Bonne nuit";
            powerUpInitializer.InstantiateXPowerUps(SyringeCount);
            SyringeCount = MaxSyringueCount;
        }
        else CrosshairText.text = "Tu n'as pas assez mangé";

        StartCoroutine(ResetCrosshairText());
    }

    void KillRabbit()
    {
        if (hoverType == ObjectType.LapinVivant) hoverObject.GetComponent<RabbitBehaviour>().Die();

        InDelay = true;

        StartCoroutine(DelayManager((int)(20 * toolUseSpeed * actionSpeedMultiplier * axeSpeedMultiplier)));

    }

    void CutTree()
    {
        TreeScript.HitTree(hoverObject);
        InDelay = true;

        StartCoroutine(DelayManager((int)(40 * toolUseSpeed * actionSpeedMultiplier * axeSpeedMultiplier)));
    }

    void CutPlank()
    {
        Vector3 pos = GetFlatPosition();
        Destroy(hoverObject);

        Instantiate(Baton, pos, GetRot());
        Instantiate(Baton, pos + Vector3.forward, GetRot());
        heldDurability--;

        InDelay = true;

        StartCoroutine(DelayManager((int)(20 * toolUseSpeed * actionSpeedMultiplier * axeSpeedMultiplier)));
    }

    void CookRabbit()
    {
        heldItem = ObjectType.LapinCuit;
        UpdateItemInHand();

        InDelay = true;

        StartCoroutine(DelayManager((int)(20 *  actionSpeedMultiplier)));
    }

    void EatRabbit()
    {
        heldItem = ObjectType.None;
        UpdateItemInHand();

        InDelay = true;

        StartCoroutine(DelayManager((int)(50 * actionSpeedMultiplier)));

        HasEatenEnough = true;
        PlayerMovement.HasEatenEnough = true;
    }

    void ExecuteCraft(CraftRecipe recipe)
    {

        if (recipe.Result == ObjectType.Barricade)
        {
            SpawnBlueprint();
            UpdateItemInHand();
        }

        else if (recipe.Result == ObjectType.Hache)
        {
            heldDurability = UnityEngine.Random.Range(3, 6) * toolDurabilityMultiplier;
        }

        InDelay = true;

        StartCoroutine(DelayManager((int)(20 * actionSpeedMultiplier)));

        heldItem = recipe.Result;

        Destroy(hoverObject);
        heldQuantity = 1;
        UpdateItemInHand();

        tooltipText.text = "";

    }

    void PickUp()
    {
        heldItem = hoverType;
        heldQuantity++;

        if (hoverType == ObjectType.Hache)
            heldDurability = hoverDurability;

        if (hoverType == ObjectType.Barricade)
            SpawnBlueprint();

        if (heldQuantity == 1)
        {
            UpdateItemInHand();
        }

        InDelay = true;

        StartCoroutine(DelayManager((int)(20 * actionSpeedMultiplier)));

        Destroy(hoverObject);
        tooltipText.text = "";

    }

    void TryDropItem()
    {
        if (heldItem == ObjectType.None || heldQuantity <= 0) return;

        Vector3 pos = GetFlatPosition();

        switch (heldItem)
        {
            case ObjectType.Hache:
                var h = Instantiate(Hache, pos, Quaternion.Euler(0, player.eulerAngles.y, 90));
                h.GetComponent<Durability>().durability = heldDurability;
                break;
            case ObjectType.Planche:
                Instantiate(Planche, pos + Vector3.up * 0.2f, GetRot());
                break;
            case ObjectType.Pierre:
                Instantiate(Pierre, pos, Quaternion.identity);
                break;
            case ObjectType.Baton:
                Instantiate(Baton, pos, GetRot());
                break;
            case ObjectType.Barricade:

                if (Barricade == null)
                {
                    return;
                }
                Instantiate(Barricade, pos + Vector3.up * 1.4f, GetRot(0));
                blueprintSpawned = false;
                Destroy(blueprint);
                break;
            case ObjectType.LapinMort:
                Instantiate(LapinMort, pos, GetRot());
                break;
        }

        InDelay = true;

        StartCoroutine(DelayManager((int)(20 * actionSpeedMultiplier)));

        heldQuantity--;
        if (heldQuantity <= 0) heldItem = ObjectType.None;
        UpdateItemInHand();
    }


    //Blueprint = hologramme de la barricade
    // === BLUEPRINT ===
    void SpawnBlueprint()
    {
        Vector3 pos = GetFlatPosition() + Vector3.up * 1.4f;
        blueprint = Instantiate(BarricadeBlueprint, pos, GetRot(0));
        blueprintSpawned = true;
    }

    void UpdateBlueprint()
    {
        Vector3 pos = GetFlatPosition(1.4f);
        blueprint.transform.position = pos;
        blueprint.transform.rotation = GetRot(0);
    }

    // === HELPERS ===
    Vector3 GetFlatPosition(float y = 0f) => new Vector3(transform.position.x, y, transform.position.z);
    Quaternion GetRot(float offset = 90f) => Quaternion.Euler(90, player.eulerAngles.y + offset, 0);

    private GameObject GetHeldDisplayPrefab(ObjectType type)
    {
        return type switch
        {
            ObjectType.Hache => HacheInHand,
            ObjectType.Planche => PlancheInHand,
            ObjectType.Baton => BatonInHand,
            ObjectType.Pierre => PierreInHand,
            ObjectType.LapinMort => LapinMortInHand,
            _ => null
        };
    }

    private void UpdateItemInHand()
    {
        if (HeldGameObjectModel != null)
            Destroy(HeldGameObjectModel);

        GameObject prefab = GetHeldDisplayPrefab(heldItem);
        if (prefab != null)
        {
            HeldGameObjectModel = Instantiate(prefab, player);
            HeldGameObjectModel.transform.localPosition = new Vector3(1, 0, 1.5f);
            HeldGameObjectModel.transform.localRotation = Quaternion.identity;
        }
    }

    private System.Collections.IEnumerator DelayManager(int deciseconds)
    {
        InDelay = true;
        while (deciseconds > 0)
        {
            yield return new WaitForSeconds(0.1f);
            deciseconds--;
        }
        InDelay = false;
    }

    // BONUS
    public void FasterTools() => toolUseSpeed = Mathf.Pow(0.65f, amplificator);
    public void CanAttackMonster()
    {
        
    }

    public void MoreDamageToMonsters() => monsterDamageMultiplier = Mathf.Pow(1.5f, amplificator);
    public void MuchMoreDamageToMonsters() => monsterDamageMultiplier = Mathf.Pow(2.5f, amplificator);

    public void BetterDIY() => toolDurabilityMultiplier = (int)Mathf.Pow(2, amplificator);
    public void AllActionsFaster() => actionSpeedMultiplier = Mathf.Pow(0.8f, amplificator);
    public void FasterAxe() => axeSpeedMultiplier = Mathf.Pow(0.5f, amplificator);

    public void CarryLiveRabbits()
    {
        
    }

    public void MoreLuck() => rareDropMultiplier = Mathf.Pow(1.4f, amplificator);
    public void MuchMoreLuck() => rareDropMultiplier = Mathf.Pow(2f, amplificator);

    public void ExtraSyringePerNight()
    {
        MaxSyringueCount++;
        SyringeCount = 4;
    }

    public void EatRocks()
    {
        
    }

    // MALUS
    public void AllActionsSlower() => actionSpeedMultiplier = Mathf.Pow(1.3f, amplificator);

    public void CanGetSick() => canGetSick = true;

    public void NoChoiceNextSyringe() => SyringeCount = 1;

    public void ActionsGiveHunger() => actionsGiveHunger = true;


}