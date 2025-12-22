using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TvBehaviour : MonoBehaviour
{
    public Image tvRenderer; // Référence au SpriteRenderer de la TV

    // Listes de sprites pour chaque langue
    public List<Sprite> spritesFR;
    public List<Sprite> spritesEN;

    private List<Sprite> currentSpriteList;

    public Image BlackScreen;
    public Image Crosshair;

    private int currentIndex = 0;

    // Simule la langue choisie (à remplacer par ton vrai système de langue)
    public enum Language { French, English }
    public Language selectedLanguage = Language.French;

    bool IsProgramEnded = false;

    void Start()
    {
        // Sélectionne la liste de sprite en fonction de la langue
        switch (selectedLanguage)
        {
            case Language.French:
                currentSpriteList = spritesFR;
                break;
            case Language.English:
                currentSpriteList = spritesEN;
                break;
        }

        if (currentSpriteList != null && currentSpriteList.Count > 0)
        {
            tvRenderer.sprite = currentSpriteList[0];
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsProgramEnded)
        {
            SwitchToNextSprite();
        }
    }
    private void SwitchToNextSprite()
    {
        if (currentSpriteList == null || currentSpriteList.Count == 0) return;
        currentIndex = currentIndex + 1;
        if (currentIndex >= currentSpriteList.Count)
        {
            Crosshair.gameObject.SetActive(true);
            tvRenderer.enabled = false;
            BlackScreen.enabled = false;
            IsProgramEnded = true;
            
        }
            
        else 
            tvRenderer.sprite = currentSpriteList[currentIndex];
    }

}
