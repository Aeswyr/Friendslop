using Mirror;
using Unity.Mathematics;
using UnityEngine;

public enum HealthType
{
    STAMINA,
    WEIGHT,
    HEALTH,
    HUNGER,
    ARMOR,
    DOOM,
    DEFAULT
}

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform modelRoot;
    [Header("Data")]
    [SerializeField] private float speed;
    [SerializeField] private float sprintMod;
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravity;
    [SerializeField] private ItemInfoDictionary itemInfo;
    [SerializeField] private int inventorySize;
    private ItemID[] inventory;
    private int selectedIndex;
    //ui
    private PlayerHudController playerHUD;

    // movement + camera
    private float sensitivity;
    private float verticalLook;
    private float horizontalLook;
    private bool sprinting;
    private Vector3 velocity;
    private Camera playerCamera;
    private bool grounded;
    private float jumpLockout;

    // gameplay
    private float nextHunger;
    private float staminaRegenStart;
    private float m_stamina;
    private float stamina
    {
        get {return m_stamina;}
        set
        {
            m_stamina = value;
            if (m_stamina > health[(int)HealthType.STAMINA])
            {
                m_stamina = health[(int)HealthType.STAMINA];
            }
            playerHUD.UpdateStamina(health, m_stamina);
        }
    }
    private float[] health = new float[(int)HealthType.DEFAULT];
    private float maxHealth = 100;
    
    private float m_focus;
    private float focus
    {
        get {return m_focus;}
        set
        {
            m_focus = value;

            playerHUD.UpdateFocus(m_focus);
        }
    }
    private float startingFocus = 100;
    private GameObject hoveredInteractable;

    private float dropStart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isLocalPlayer)
        {
            CaptureCamera();

            modelRoot.GetComponent<MeshRenderer>().enabled = false;
            
            playerHUD = FindAnyObjectByType<PlayerHudController>();
        
            RemoveDamage(HealthType.STAMINA, 0);

            nextHunger = Time.time + 20f;

            playerHUD.ResizeInventory(inventorySize);
            inventory = new ItemID[inventorySize];
            playerHUD.UpdateInventory(inventory, selectedIndex);
            playerHUD.UpdateCursorWheel(false, 0);
        }
    }

    private void CaptureCamera()
    {
        playerCamera = Camera.main;
        playerCamera.transform.parent = cameraHolder;
        playerCamera.transform.localPosition = Vector3.zero;

        Cursor.lockState = CursorLockMode.Locked;

        sensitivity = 0.5f;//PlayerHUDController.Instance.GetSensitivity();
        //playerCamera.fieldOfView = PlayerHUDController.Instance.GetFOV();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;
        

        HandleInput();
        HandleMovement();
        HandleCamera();
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        grounded = IsGrounded();

        if (Time.time > nextHunger)
        {
            AddDamage(HealthType.HUNGER, 5f);
            nextHunger = Time.time + 20f;
        }
        if (Time.time > staminaRegenStart && stamina < health[(int)HealthType.STAMINA])
        {
            stamina += 0.7f;
        }

        HandleInput();

        DetectPickups();
    }

    private void DetectPickups() {
        var ray = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.rotation * Vector3.forward, out RaycastHit hitInfo, 2.5f, LayerMask.GetMask(new string[] {"Interactable"}));
        if (ray)
        {
            playerHUD.ShowPrompt("Pick up [E]");
            hoveredInteractable = hitInfo.collider.gameObject;
        } else
        {
            playerHUD.HidePrompt();
            hoveredInteractable = null;
        }

    }

    private void HandleInput()
    {
        if (stamina > 0 && InputHandler.Instance.jump.pressed && CanInput()) {
            grounded = false;
            velocity.y = jumpForce;
            jumpLockout = Time.time + 0.1f;
            stamina -= 15;
            staminaRegenStart = Time.time + 0.5f;
        }

        sprinting = stamina > 0 && InputHandler.Instance.sprint.down && InputHandler.Instance.dir != Vector2.zero && CanInput();
        if (sprinting)
        {
            stamina -= 0.8f;
            staminaRegenStart = Time.time + 0.5f;
        }

        if (InputHandler.Instance.scroll.pressed)
        {
            selectedIndex = (selectedIndex + 1 * (int)Mathf.Sign(InputHandler.Instance.scrollDir) + inventorySize) % inventorySize;
            playerHUD.UpdateInventory(inventory, selectedIndex);
            UpdateSelectedItem();
        }

        ItemID item = inventory[selectedIndex];
        if (item != ItemID.NONE) {
            if (InputHandler.Instance.action1.pressed)
            {
                itemInfo.GetItemInfo(item).action1.action?.Invoke(this, item);
            }
            else if (InputHandler.Instance.action2.pressed)
            {
                itemInfo.GetItemInfo(item).action2.action?.Invoke(this, item);
            }
            else if (InputHandler.Instance.action3.pressed)
            {
                itemInfo.GetItemInfo(item).action3.action?.Invoke(this, item);
            }
        }

        if (inventory[selectedIndex] != ItemID.NONE && InputHandler.Instance.drop.pressed)
        {
            dropStart = Time.time;
        } else if (inventory[selectedIndex] != ItemID.NONE && InputHandler.Instance.drop.down)
        {
            float time = Time.time - dropStart;
            playerHUD.UpdateCursorWheel(true, time);
        }
        if (inventory[selectedIndex] != ItemID.NONE && InputHandler.Instance.drop.released)
        {
            float launchVelocity = 1;
            if (Time.time - dropStart > 0.5f)
                launchVelocity *= Mathf.Min(24, 24 * (Time.time - dropStart));
            GameManager.Instance.SpawnItem(inventory[selectedIndex], transform.position + cameraHolder.rotation * Vector3.forward, transform.rotation, cameraHolder.rotation * (launchVelocity * Vector3.forward));
            RemoveItem(selectedIndex);

            playerHUD.UpdateCursorWheel(false, 0);
        }

        if (hoveredInteractable != null && InputHandler.Instance.interact.pressed)
        {
            hoveredInteractable.GetComponentInParent<Interactable>().OnInteract(this);
        }
    }
    private void HandleMovement()
    {
        Vector3 dir = new Vector3(InputHandler.Instance.dir.x, 
                            0,
                            InputHandler.Instance.dir.y);
        
        if (!CanInput())
            dir = Vector3.zero;

        dir = Time.deltaTime * speed * dir;
        if (sprinting) {
            dir = sprintMod * dir;
        }
        dir = Quaternion.Euler(0, horizontalLook, 0) * dir;
        velocity.x = dir.x;
        velocity.z = dir.z;

        controller.Move(velocity);

        if (!grounded) {
            velocity.y -= Time.deltaTime * gravity;
        } else
            velocity.y = 0;

    }
    private void HandleCamera()
    {
        if (!CanInput())
            return;
        horizontalLook += sensitivity * InputHandler.Instance.mouseDelta.x;
        verticalLook -= sensitivity * InputHandler.Instance.mouseDelta.y;
        verticalLook = Mathf.Clamp(verticalLook, -85, 85);
        cameraHolder.localRotation = Quaternion.Euler(verticalLook, 0, 0);
        transform.localRotation = Quaternion.Euler(0, horizontalLook, 0);
    }

    private bool CanInput()
    {
        return !PauseMenuController.Instance.IsPaused();
    }

    private bool IsGrounded() {
        return Physics.Raycast(new Ray(transform.position + Vector3.down, Vector3.down), 0.1f) && Time.time > jumpLockout;
    }

    public void AddDamage(HealthType type, float amt)
    {
        health[(int)type] += amt;

        float staminaCap = maxHealth;
        for (int i = 1; i < health.Length; i++) {
            staminaCap -= health[i];
        }
        health[(int)HealthType.STAMINA] = staminaCap;

        if (stamina > staminaCap)
            stamina = staminaCap;
        
        playerHUD.UpdateDisplay(health, maxHealth, stamina);
    }

    public void RemoveDamage(HealthType type, float amt)
    {
        health[(int)type] -= amt;
        if (health[(int)type] < 0)
            health[(int)type] = 0;

        float staminaCap = maxHealth;
        for (int i = 1; i < health.Length; i++) {
            staminaCap -= health[i];
        }
        health[(int)HealthType.STAMINA] = staminaCap;

        if (stamina > staminaCap)
            stamina = staminaCap;

        playerHUD.UpdateDisplay(health, maxHealth, stamina);
    }

    public bool AddItem(ItemID item)
    {
        int slot = GetEmptyInventorySlot();
        if (slot == -1)
            return false;

        inventory[slot] = item;
        playerHUD.UpdateInventory(inventory, selectedIndex);

        if (slot == selectedIndex)
            UpdateSelectedItem();
        return true;
    }

    public void RemoveItem(int slot)
    {
        inventory[slot] = ItemID.NONE;
        playerHUD.UpdateInventory(inventory, selectedIndex);

        if (slot == selectedIndex)
        {
            playerHUD.UpdateUsePrompts(new string[3]);
        }
    }

    public void RemoveHeldItem()
    {
        RemoveItem(selectedIndex);
    }

    private int GetEmptyInventorySlot()
    {
        if (inventory[selectedIndex] == ItemID.NONE)
            return selectedIndex;

        for (int i = 0; i < inventorySize; i++) {
            int slot = (i + selectedIndex) % inventorySize;
            if (inventory[slot] == ItemID.NONE)
                return slot;
        }
        return -1;
    }

    private void UpdateSelectedItem()
    {
        var tooltips = new string[3];
        if (inventory[selectedIndex] != ItemID.NONE)
        {
            AssignTooltip(itemInfo.GetItemInfo(inventory[selectedIndex]).action1.verb, 0);
            AssignTooltip(itemInfo.GetItemInfo(inventory[selectedIndex]).action2.verb, 1);
            AssignTooltip(itemInfo.GetItemInfo(inventory[selectedIndex]).action3.verb, 2);

            void AssignTooltip(string verb, int index)
            {
                if (!string.IsNullOrEmpty(verb))
                {
                    string keyName = "";
                    switch (index)
                    {
                        case 0:
                            keyName = "LMB";
                            break;
                        case 1:
                            keyName = "RMB";
                            break;
                        case 2:
                            keyName = "F";
                            break;
                    }
                    tooltips[index] = $"[{keyName}] to {verb}";
                }
            }
        }

        playerHUD.UpdateUsePrompts(tooltips);
    }
}


