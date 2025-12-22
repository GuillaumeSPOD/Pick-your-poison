using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Random = System.Random;

public static class PowerUpManager
{
    public class PowerUp
    {
        public string description;
        public string quote;
        public Action function;
        public int weight;

        public List<PowerUp> requirements;
        public List<PowerUp> incompatibilities;

        public PowerUp(string description_, string quote_, Action func, int weight_, List<PowerUp> PowerRequired = null, List<PowerUp> PowerIncompatible = null)
        {
            description = description_;
            quote = quote_;
            function = func;
            weight = weight_;
            requirements = PowerRequired ?? new List<PowerUp>();
            incompatibilities = PowerIncompatible ?? new List<PowerUp>();
        }
    }

    private static List<PowerUp> Bonus;
    private static List<PowerUp> Malus;

    private static List<PowerUp> CompatibleBonus = new List<PowerUp> { };
    private static List<PowerUp> CompatibleMalus = new List<PowerUp> { };

    private static List<PowerUp> TakenPowerUps = new List<PowerUp>();

    private static int seed = GlobalVariables.PowerUpSeed;
    private static Random rnd = new Random(seed);

    private static bool initialized = false;

    public static void InitializeLists()
    {
        if (initialized) return;

        initialized = true;

        PowerUpSO[] allPowerUpSOs = Resources.LoadAll<PowerUpSO>("PowerUps");

        Bonus = new List<PowerUp>();
        Malus = new List<PowerUp>();

        foreach (PowerUpSO so in allPowerUpSOs)
        {
            PowerUp power = so.ToPowerUp();
            if (so.IsBonus) Bonus.Add(power);
            else Malus.Add(power);
        }

        foreach (PowerUpSO so in allPowerUpSOs)
        {
            PowerUp p = (so.IsBonus ? Bonus : Malus)
                .Find(x => string.Equals(x.description, so.description, StringComparison.OrdinalIgnoreCase));

            p.requirements = so.requirements.Select(reqSO =>
                (reqSO.IsBonus ? Bonus : Malus)
                .Find(x => string.Equals(x.description, reqSO.description, StringComparison.OrdinalIgnoreCase))
            ).Where(x => x != null).ToList();

            p.incompatibilities = so.incompatibilities.Select(incSO =>
                (incSO.IsBonus ? Bonus : Malus)
                .Find(x => string.Equals(x.description, incSO.description, StringComparison.OrdinalIgnoreCase))
            ).Where(x => x != null).ToList();
        }

        CompatibleBonus = Bonus.Where(p => p.requirements.Count == 0).ToList();
        CompatibleMalus = Malus.Where(p => p.requirements.Count == 0).ToList();

    }



    //Les autres méthodes restent inchangées
    public static void TakePowerUpPair(PowerUp Upgrade, PowerUp Downgrade)
    {
        foreach (PowerUp incompatibility in Upgrade.incompatibilities)
        {
            CompatibleBonus.Remove(incompatibility);
            CompatibleMalus.Remove(incompatibility);
            Debug.Log("Truc retiré 1");
        }

        foreach (PowerUp incompatibility in Downgrade.incompatibilities)
        {
            CompatibleBonus.Remove(incompatibility);
            CompatibleMalus.Remove(incompatibility);
            Debug.Log("Truc retiré 2");
        }

        TakenPowerUps.Add(Upgrade);
        TakenPowerUps.Add(Downgrade);

        foreach (PowerUp OnePowerUp in Bonus)
        {
            if (!TakenPowerUps.Contains(OnePowerUp) &&
                OnePowerUp.requirements.All(req => TakenPowerUps.Contains(req)) &&
                !OnePowerUp.incompatibilities.Any(inc => TakenPowerUps.Contains(inc)) &&
                !CompatibleBonus.Contains(OnePowerUp))
            {
                CompatibleBonus.Add(OnePowerUp);
            }
        }

        foreach (PowerUp OnePowerUp in Malus)
        {
            if (!TakenPowerUps.Contains(OnePowerUp) &&
                OnePowerUp.requirements.All(req => TakenPowerUps.Contains(req)) &&
                !OnePowerUp.incompatibilities.Any(inc => TakenPowerUps.Contains(inc)) &&
                !CompatibleMalus.Contains(OnePowerUp))
            {
                CompatibleMalus.Add(OnePowerUp);
            }
        }


        CompatibleBonus.Remove(Upgrade);
        CompatibleMalus.Remove(Downgrade);

        
    }

    public static List<PowerUp> GeneratePowerUpPair(List<PowerUp> LocalCompatibleBonus, List<PowerUp> LocalCompatibleMalus)
    {
        int TotalCompatibleBonusWeight = 0;
        int TotalCompatibleMalusWeight = 0;

        List<int> BonusWeightToPowerUpIndex = new List<int>();
        List<int> MalusWeightToPowerUpIndex = new List<int>();

        foreach (PowerUp PowerUp in LocalCompatibleBonus)
        {
            TotalCompatibleBonusWeight += PowerUp.weight;
            BonusWeightToPowerUpIndex.Add(TotalCompatibleBonusWeight);
        }

        int index = rnd.Next(0, TotalCompatibleBonusWeight + 1);
        PowerUp GeneratedBonus = LocalCompatibleBonus[GetIndexFromWeight(BonusWeightToPowerUpIndex, index)];

        foreach (PowerUp incompatibility in GeneratedBonus.incompatibilities)
        {
            LocalCompatibleMalus.Remove(incompatibility);
        }

        foreach (PowerUp PowerUp in LocalCompatibleMalus)
        {
            TotalCompatibleMalusWeight += PowerUp.weight;
            MalusWeightToPowerUpIndex.Add(TotalCompatibleMalusWeight);
        }

        index = rnd.Next(0, TotalCompatibleMalusWeight + 1);
        PowerUp GeneratedMalus = LocalCompatibleMalus[GetIndexFromWeight(MalusWeightToPowerUpIndex, index)];

        return new List<PowerUp> { GeneratedBonus, GeneratedMalus };
    }

    private static int GetIndexFromWeight(List<int> WeightToIndex, int randomValue)
    {
        for (int i = 0; i < WeightToIndex.Count; i++)
        {
            if (randomValue <= WeightToIndex[i])
                return i;
        }
        return WeightToIndex.Count - 1;
    }

    public static List<List<PowerUp>> GenerateXPowerUpPairs(int X)
    {


        if (X <= 0) return new List<List<PowerUp>>();

        List<List<PowerUp>> ResultingList = new List<List<PowerUp>>();

        List<PowerUp> LocalCompatibleBonus = CompatibleBonus.ToList();
        List<PowerUp> LocalCompatibleMalus = CompatibleMalus.ToList();


        if (LocalCompatibleBonus.Count == 0 || LocalCompatibleMalus.Count == 0)
        {
            return new List<List<PowerUp>>();
        }

        for (int i = 0; i < X; i++)
        {
            ResultingList.Add(GeneratePowerUpPair(LocalCompatibleBonus, LocalCompatibleMalus));

            LocalCompatibleBonus.Remove(ResultingList[i][0]);
            LocalCompatibleMalus.Remove(ResultingList[i][1]);
        }

        return ResultingList;
    }
}
