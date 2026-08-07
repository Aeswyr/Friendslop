
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerHudController : MonoBehaviour
{
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform[] damageTypes;
    [SerializeField] private Image staminaBar;
    [SerializeField] private RectTransform focusBar;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private GameObject actionSlotPrefab;
    [SerializeField] private Transform inventoryParent;
    [SerializeField] private TextMeshProUGUI prompt;
    [SerializeField] private GameObject cursorWheel;
    [SerializeField] private Image cursorWheelFill;
    [SerializeField] private Transform usePromptHolder;
    [SerializeField] private GameObject usePromptPrefab;
    List<InventorySlot> inventory = new();
    private float baseFocusLength;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseFocusLength = focusBar.sizeDelta.x;
        HidePrompt();

        for (int i = 0; i < 3; i++)
        {
            Instantiate(usePromptPrefab, usePromptHolder).SetActive(false);
        }
    }

    public void ResizeInventory(int inventorySize)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            inventory.Add(Instantiate(inventorySlotPrefab, inventoryParent).GetComponent<InventorySlot>());
        }
    }

    public void AddInventoryAction()
    {
        Instantiate(actionSlotPrefab, inventoryParent);
    }

    public void UpdateDisplay(float[] health, float maxHealth, float stamina)
    {
        for (int i = 0; i < health.Length; i++)
        {
            damageTypes[i].sizeDelta = new Vector2(health[i] / maxHealth * healthBar.sizeDelta.x, damageTypes[i].sizeDelta.y);
        }
        
        UpdateStamina(health, stamina);
    }

    public void UpdateStamina(float[] health, float stamina)
    {
        if (health[(int)HealthType.STAMINA] > 0)
        {
        staminaBar.fillAmount = stamina / health[(int)HealthType.STAMINA];
        } else
        {
            staminaBar.fillAmount = 0;
        }
    }

    public void UpdateFocus(float amt)
    {
        focusBar.sizeDelta = new Vector2(baseFocusLength * amt / 100, focusBar.sizeDelta.y);
    }

    public void UpdateCursorWheel(bool show, float amt)
    {
        cursorWheel.SetActive(show);
        cursorWheelFill.fillAmount = amt;
    }

    public void ShowPrompt(string text)
    {
        prompt.gameObject.SetActive(true);
        prompt.text = text;
    }

    public void HidePrompt()
    {
        prompt.gameObject.SetActive(false);
    }

    public void UpdateInventory( ItemID[] items, int selected)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            inventory[i].UpdateIcon(items[i]);
            inventory[i].SetSelected(i == selected);
        }
    }

    public void UpdateUsePrompts(string[] prompts)
    {
        for (int i = 0; i < usePromptHolder.childCount; i++)
        {
            GameObject usePrompt = usePromptHolder.GetChild(i).gameObject;

            if (string.IsNullOrEmpty(prompts[i]))
            {
                usePrompt.SetActive(false);
            } else {
                usePrompt.SetActive(true);
                usePrompt.GetComponent<TextMeshProUGUI>().text = prompts[i];
            }
        }
    }
}
