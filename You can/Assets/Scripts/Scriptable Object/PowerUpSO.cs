using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUps/PowerUp")]
public class PowerUpSO : ScriptableObject
{
    public string description;
    [TextArea] public string quote;

    [Header("Effet associé (nom de la méthode dans PowerUpEffects)")]
    public string effectMethodName;

    [Header("Liens logiques")]
    public List<PowerUpSO> requirements;
    public List<PowerUpSO> incompatibilities;

    public int weight = 100;

    public bool IsBonus = true; 

    // Conversion en PowerUp (classe d'origine)
    public PowerUpManager.PowerUp ToPowerUp()
    {
        Action action = null;

        if (!string.IsNullOrEmpty(effectMethodName))
        {
            // Reflection : cherche la méthode statique du même nom
            var method = typeof(PowerUpEffectManager).GetMethod(effectMethodName,
                BindingFlags.Public | BindingFlags.Static);
            if (method != null)
                action = (Action)Delegate.CreateDelegate(typeof(Action), method);
        }

        return new PowerUpManager.PowerUp(description, quote, action, weight);
    }
}