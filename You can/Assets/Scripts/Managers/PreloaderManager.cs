using UnityEngine;

public class PreloaderManager : MonoBehaviour
{
    void Awake()
    {
        // Force l'initialisation dès le chargement de la scène
        PowerUpManager.InitializeLists();
    }
}
