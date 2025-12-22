using UnityEngine;
using Random = System.Random;
public static class GlobalVariables
{
    public static int CurrentDay;
    public static int CurrentTime;
    public static int GlobalSeed;
    public static int PowerUpSeed;
    public static int AxeDurabilitySeed;



    public static void InitializeSeed()
    {
        GlobalSeed = UnityEngine.Random.Range(100, 99999999);
        Random rnd = new Random(GlobalSeed);
        PowerUpSeed = rnd.Next(100, 99999999);
        AxeDurabilitySeed = rnd.Next(100, 99999999);

    }
}   
