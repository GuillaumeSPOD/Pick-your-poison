using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PowerUpInitializer : MonoBehaviour
{
    public GameObject ChoosingUI;
    public Button buttonPrefab;
    public ScrollRect scrollRect;
    public RectTransform content;

    private List<List<PowerUpManager.PowerUp>> GeneratedPowers;

    public float topMargin = 50f;
    public float spacing = 25f;
    public float buttonHeight = 75f;
    public float buttonWidth = 700f;
    public float visibleHeight = 405f;

    public PlayerMovement PlayerScript;
    public PlayerInteraction InteractionScript;

    public List<GameObject> ChoosingSeringeGameObjects;

    public void InstantiateXPowerUps(int X)
    {
        Utilities.DisableScript(PlayerScript);
        Utilities.DisableScript(InteractionScript);

        ChoosingUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PowerUpManager.InitializeLists();

        GeneratedPowers = PowerUpManager.GenerateXPowerUpPairs(X);

        // Clear anciens boutons
        

        float requiredHeight = topMargin + X * buttonHeight + (X - 1) * spacing;
        bool needScroll = requiredHeight > visibleHeight;
        scrollRect.vertical = needScroll;

        float effectiveSpacing = needScroll ? spacing : (visibleHeight - topMargin - X * buttonHeight) / Mathf.Max(X - 1, 1);
        content.sizeDelta = new Vector2(content.sizeDelta.x, needScroll ? requiredHeight : visibleHeight);

        float currentY = -topMargin;

        for (int i = 0; i < X; i++)
        {
            int index = i; // capture pour lambda

            Button button = Instantiate(buttonPrefab, content, false);
            RectTransform rt = button.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            rt.anchoredPosition = new Vector2(0f, currentY - buttonHeight);

            currentY -= buttonHeight + effectiveSpacing;

            PowerUpButtonTexts texts = button.GetComponent<PowerUpButtonTexts>();
            if (texts != null)
            {
                texts.bonusTitle.text = GeneratedPowers[index][0].description;
                texts.bonusDesc.text = GeneratedPowers[index][0].quote;
                texts.malusTitle.text = GeneratedPowers[index][1].description;
                texts.malusDesc.text = GeneratedPowers[index][1].quote;
            }

            button.onClick.AddListener(() => TakingPowerUp(index));
        }
    }

    private void TakingPowerUp(int PowerUpId)
    {
        GeneratedPowers[PowerUpId][0].function?.Invoke();
        GeneratedPowers[PowerUpId][1].function?.Invoke();

        PowerUpManager.TakePowerUpPair(
            GeneratedPowers[PowerUpId][0],
            GeneratedPowers[PowerUpId][1]
        );

        foreach (Transform child in content)
            Destroy(child.gameObject);

        ChoosingUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Utilities.EnableScript(PlayerScript);
        Utilities.EnableScript(InteractionScript);

        
    }
}