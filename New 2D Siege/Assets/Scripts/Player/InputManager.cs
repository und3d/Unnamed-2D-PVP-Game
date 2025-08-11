using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager PlayerKeybinds { get; private set; }

    [Header("Assign your .inputactions asset here")]
    [SerializeField] private InputActionAsset actions;

    public InputActionAsset Actions => actions;

    private void Awake()
    {
        if (PlayerKeybinds != null) { Destroy(gameObject); return; }
        PlayerKeybinds = this;
        DontDestroyOnLoad(gameObject);

        if (actions == null)
        {
            Debug.LogError("InputManager: assign an InputActionAsset in the Inspector.");
            return;
        }

        // Enable all maps (or pick specific ones if you prefer)
        foreach (var map in actions.actionMaps)
            map.Enable();
    }

    /// Get an action by "Map/Action" or just "Action" name.
    public InputAction Get(string actionPathOrName)
    {
        var a = actions?.FindAction(actionPathOrName, false);
        if (a == null) Debug.LogError($"InputManager: action '{actionPathOrName}' not found.");
        return a;
    }

    public void EnableMap(string mapName)  => actions.FindActionMap(mapName, true).Enable();
    public void DisableMap(string mapName) => actions.FindActionMap(mapName, true).Disable();
}