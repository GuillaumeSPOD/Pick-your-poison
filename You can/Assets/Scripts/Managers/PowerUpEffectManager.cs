using UnityEngine;

public static class PowerUpEffectManager
{
    private static PlayerMovement movement;
    private static PlayerInteraction interaction;
    private static FogManager fog;

    private static GameObject LeftEyeMissing;
    private static GameObject RightEyeMissing;
    private static GameObject BlackScreen;

    static PowerUpEffectManager()
    {
        fog = Object.FindAnyObjectByType<FogManager>();
        movement = PlayerMovement.Instance;
        interaction = Object.FindAnyObjectByType<PlayerInteraction>();

        LeftEyeMissing = GameObject.Find("UI/#Main Canvas/InGameUI/ViewObstruction/LeftEyeMissing");
        RightEyeMissing = GameObject.Find("UI/#Main Canvas/InGameUI/ViewObstruction/RightEyeMissing");
        BlackScreen = GameObject.Find("UI/#Main Canvas/InGameUI/ViewObstruction/BlackScreen");

    }

    // BONUS
    public static void IncreaseSprintSpeed() => movement.IncreaseSprintSpeed();
    public static void MuchFasterSprint() => movement.MuchFasterSprint();
    public static void ExtremeSprint() => movement.ExtremeSprint();

    public static void FasterTools() => interaction.FasterTools();
    public static void CanAttackMonster() => interaction.CanAttackMonster();
    public static void MoreDamageToMonsters() => interaction.MoreDamageToMonsters();
    public static void MuchMoreDamageToMonsters() => interaction.MuchMoreDamageToMonsters();
    public static void InstantMaxSpeed() => movement.InstantMaxSpeed();
    public static void BetterDIY() => interaction.BetterDIY();
    public static void AllActionsFaster() => interaction.AllActionsFaster();
    public static void FasterAxe() => interaction.FasterAxe();

    public static void SeeFartherInFog() 
    {
        fog.ChangeSize(45, 35);
        RenderSettings.fogDensity *= 0.80f;
    }

    public static void MoreEndurance() => movement.MoreEndurance();
    public static void MuchMoreEndurance() => movement.MuchMoreEndurance();
    public static void ExtremeEndurance() => movement.ExtremeEndurance();

    public static void LessHunger() => movement.LessHunger();
    public static void MuchLessHunger() => movement.MuchLessHunger();
    public static void HugeLessHunger() => movement.HugeLessHunger();

    public static void HearRabbitsFarther() { Debug.Log("Tu entends les lapins de plus loin (50m) !"); }
    public static void HearRabbitsMuchFarther() { Debug.Log("Tu entends les lapins de beaucoup plus loin (70m) !"); }

    public static void CarryLiveRabbits() => interaction.CarryLiveRabbits();
    public static void Taller() => movement.Taller();
    public static void SprintUntilFull() => movement.SprintUntilFull();

    public static void NoSideEffectsThisNight() { Debug.Log("Tu n'as pas d'effets secondaires pour cette nuit !"); }
    public static void LeftEyeCovers() { LeftEyeMissing.SetActive(false); }
    public static void RightEyeCovers() { RightEyeMissing.SetActive(false); }

    public static void SlowerFatigue() => movement.SlowerFatigue();
    public static void NoFatLoss() => movement.NoFatLoss();
    public static void ExtraSyringePerNight() => interaction.ExtraSyringePerNight();

    public static void MoreLuck() => interaction.MoreLuck();
    public static void MuchMoreLuck() => interaction.MuchMoreLuck();
    public static void EatRocks() => interaction.EatRocks();

    // MALUS
    public static void LoseRightEye() 
    {
        RightEyeMissing.SetActive(true);
    }
    public static void LoseLeftEye() 
    {
        LeftEyeMissing.SetActive(true);
    }
    public static void FoggyCampFireVision() { Debug.Log("Tu ne vois pas toujours la fumée du feu de camp !"); }
    public static void MoreHunger() => movement.MoreHunger();
    public static void MuchMoreHunger() => movement.MuchMoreHunger();
    public static void HugeMoreHunger() => movement.HugeMoreHunger();
    public static void BecomeFat() => movement.BecomeFat();
    public static void SlowerAcceleration() { Debug.Log("Tu accélères beaucoup plus lentement (50%) !"); }
    public static void CannotHearFootsteps() { Debug.Log("Tu n'entends plus les bruits de pas !"); }
    public static void CannotHearWell() { Debug.Log("Tu n'entends vraiment pas bien !"); }
    public static void Hallucinations() { Debug.Log("Tu vois des hallucinations !"); }
    public static void ManyHallucinations() { Debug.Log("Tu vois beaucoup plus d'hallucinations !"); }
    public static void AllActionsSlower() => interaction.AllActionsSlower();
    public static void Limp() { movement.Limp() ; }
    public static void Wheels() { movement.Wheels(); }
    public static void WheelsCreak() { Debug.Log("Tes roues grincent !"); }
    public static void ScareRabbits() { Debug.Log("Tu effrayes les lapins !"); }
    public static void Smaller() => movement.Smaller();
    public static void NeedToDrink() { Debug.Log("Tu as besoin de boire !"); }
    public static void CanGetSick() { Debug.Log("Tu peux tomber malade !"); }
    public static void SlowerEnergyRecovery() => movement.SlowerEnergyRecovery();
    public static void CannotSprint() => movement.CannotSprint();
    public static void CanGetHurtWhileRunning() { Debug.Log("Tu peux te blesser en courant !"); }
    public static void NoChoiceNextSyringe() => interaction.NoChoiceNextSyringe();
    public static void FasterFatigue() => movement.FasterFatigue();
    public static void Blink() { Debug.Log("Tu clignes des yeux !"); }
    public static void BlinkAndHallucinate() { Debug.Log("Cligner des yeux peut provoquer des hallucinations !"); }
    public static void SeeingMonsterGivesHunger() { Debug.Log("Voir le monstre te donne faim !"); }
    public static void ActionsGiveHunger() => interaction.ActionsGiveHunger();
    public static void AmplifySideEffects() { Debug.Log("Tes effets secondaires sont amplifiés pour cette nuit !"); }
    public static void HeartAttack() { Debug.Log("Tu peux faire une crise cardiaque !"); }
}