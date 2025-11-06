#nullable enable
using TMPro;
using UnityEngine;

/// <summary>
/// This script is attached to the Main Camera which is a child of the Player object.
/// 
/// When a player looks at an interactable object, aka and Item, within a certain 
/// range, this script is responsible for detecting if the player is looking at an 
/// interactable object, displaying the help text, and invoking the Item's
/// interaction menu.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{

#region Interaction Interface Settings
    public float interactRange = 3f;
    public UnityEngine.UI.Image? crosshairImage;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.green;
    public TextMeshProUGUI? helpTextUI;
#endregion

    public GameObject? FocusedObject = null;    
    private IInteractable? _currentFocus;

    private CharacterEntity? _playerEntity = null;

    private void Start()
    {
        helpTextUI?.gameObject.SetActive(false);
    }

    private void checkInteractions()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, interactRange, ~0))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                if (interactable != _currentFocus)
                {
                    ClearFocus();
                    _currentFocus = interactable;
                    _currentFocus!.OnFocus();
                    
                    if (crosshairImage != null)
                    {
                        crosshairImage.color = defaultColor;
                    }

                    if (helpTextUI != null)
                    {
                        helpTextUI.gameObject.SetActive(true);
                        helpTextUI.text = interactable.HoverText;
                    }

                    FocusedObject = hit.collider.gameObject;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    _currentFocus!.Interact();
                }
            }
            else
            {
                ClearFocus();
            }
        }
        else
        {
            ClearFocus();
        }
    }

    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            throw new System.Exception("PlayerInteractor: Player GameObject not found. Make sure the Player has the 'Player' tag.");
        }

        var playerEntity = player.GetComponent<CharacterEntity>();
        if (playerEntity == null)
        {
            throw new System.Exception("PlayerInteractor: CharacterEntity script not found on Player object.");
        }

        _playerEntity = playerEntity;

        InputManager.Instance.RegisterInputHandler(InputState.Gameplay, this.ProcessInput);
    }

    void ClearFocus()
    {
        if (_currentFocus != null)
        {
            _currentFocus!.OnDefocus();
            _currentFocus = null;
        }

        if (helpTextUI != null)
        {
            helpTextUI.text = "";
            helpTextUI.gameObject.SetActive(false);
        }

        if (crosshairImage != null)
        {
            crosshairImage.color = defaultColor;
        }
        FocusedObject = null;
    }

    private void Update()
    {
        checkInteractions();
    }

    void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_playerEntity!.EquippedItem == null)
            {
                if (FocusedObject == null) return;

                var itemEntity = FocusedObject!.GetComponent<ItemEntity>();
                if (itemEntity == null) return;

                _playerEntity!.SetEquippedItem(itemEntity!);
                BottomTypewriter.Instance.Enqueue("Picked up item '" + itemEntity.ItemData!.ItemName + "'");
            }
            else
            {
                var droppedItem = _playerEntity!.DropEquippedItem();
                if (droppedItem != null)
                {
                    BottomTypewriter.Instance.Enqueue("Dropped equipped item '" + droppedItem!.ItemData!.ItemName + "'");
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            if (FocusedObject == null) return;

            var itemEntity = FocusedObject.GetComponent<ItemEntity>();
            if (itemEntity == null) return;

            _playerEntity!.AddItemToInventory(itemEntity);
            BottomTypewriter.Instance.Enqueue("Added item '" + itemEntity.ItemData!.ItemName + "' to inventory.");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (FocusedObject != null)
            {
                var itemEntity = FocusedObject!.GetComponent<ItemEntity>();
                if (itemEntity == null)
                {
                    throw new System.Exception("PlayerInteractor: FocusedObject does not have an ItemEntity component.");
                }

                _playerEntity!.AddItemToInventory(itemEntity);
                BottomTypewriter.Instance.Enqueue("Added item '" + itemEntity.ItemData!.ItemName + "' to inventory.");
            }
            else if (_playerEntity!.EquippedItem != null)
            {
                var itemEntity = _playerEntity!.EquippedItem;
                _playerEntity!.AddItemToInventory(_playerEntity!.EquippedItem!);
                BottomTypewriter.Instance.Enqueue("Added item '" + itemEntity.ItemData!.ItemName + "' to inventory.");
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            _playerEntity!.Attack();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            _playerEntity!.ThrowEquippedItem();
        }
    }
}
