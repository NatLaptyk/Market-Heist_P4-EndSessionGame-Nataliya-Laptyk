using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Singleton managing save/load via JsonUtility + Application.persistentDataPath.

// SAVE DATA STRUCTURE:
// SaveData is a [Serializable] C# class containing player position (as SerializableVector3
// since JsonUtility can't serialize Unity's Vector3 directly in all versions), player
// health, item count, two world-state booleans (chefCatPersuaded, fishStolen), and a list
// of collected pickup IDs to persist which coins have been gathered.

// FILE LOCATION:
// Saved to Application.persistentDataPath + "/savegame.json". This path is OS-appropriate
// (AppData on Windows, ~/Library on macOS, /data/data on Android) and persists across
// game sessions and Unity restarts.

// PLAYERPREFS USE:
// PlayerPrefs is reserved for SETTINGS only (volume, resolution). Game state ALWAYS uses
// JsonUtility because (1) PlayerPrefs has no built-in serialization for complex types,
// (2) PlayerPrefs is stored in registry/plist on most platforms which is the wrong place
// for game state, (3) PlayerPrefs has size limits.

// ERROR HANDLING:
// Missing file = log message, do nothing (player just stays as is).
// Corrupt JSON = catch exception, log error, do nothing (don't crash, don't corrupt state).
// Save errors = catch + log + display.

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInputHandler m_inputHandler;
    [SerializeField] private Transform m_playerTransform;
    [SerializeField] private Health m_playerHealth;

    private const string SaveFileName = "savegame.json";
    private const string VolumePrefsKey = "MasterVolume";

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    // Track which pickups have been collected this session
    private HashSet<string> m_collectedPickups = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (m_inputHandler == null) Debug.LogError($"{nameof(SaveManager)}: PlayerInputHandler not assigned.");
        if (m_playerTransform == null) Debug.LogError($"{nameof(SaveManager)}: Player Transform not assigned.");
        if (m_playerHealth == null) Debug.LogError($"{nameof(SaveManager)}: Player Health not assigned.");
    }

    private void Start()
    {
        if (m_inputHandler != null)
        {
            m_inputHandler.OnSavePressed += SaveGame;
            m_inputHandler.OnLoadPressed += LoadGame;
        }

        Debug.Log($"{nameof(SaveManager)}: Save path is {SavePath}");
    }

    private void OnDisable()
    {
        if (m_inputHandler != null)
        {
            m_inputHandler.OnSavePressed -= SaveGame;
            m_inputHandler.OnLoadPressed -= LoadGame;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void NotifyPickupCollected(string pickupId)
    {
        if (string.IsNullOrEmpty(pickupId)) return;
        m_collectedPickups.Add(pickupId);
    }

    public bool IsPickupCollected(string pickupId)
    {
        return m_collectedPickups.Contains(pickupId);
    }

   
    // GAME STATE — handled by JsonUtility + file I/O (NOT PlayerPrefs)
    

    [ContextMenu("Test: Save Game")]
    public void SaveGame()
    {
        try
        {
            SaveData data = new SaveData();

            // Gather state from each subsystem
            if (m_playerTransform != null)
            {
                data.playerPosition = new SerializableVector3(m_playerTransform.position);
            }

            if (m_playerHealth != null)
            {
                data.playerHealth = m_playerHealth.CurrentHealth;
            }

            if (GameManager.Instance != null)
            {
                data.itemCount = GameManager.Instance.ItemCount;
                data.chefCatPersuaded = GameManager.Instance.ChefCatPersuaded;
                data.fishStolen = GameManager.Instance.FishStolen;
            }

            data.pickupsCollected = new List<string>(m_collectedPickups);

            // Serialize
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);

            Debug.Log($"{nameof(SaveManager)}: Game saved to {SavePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{nameof(SaveManager)}: Save failed — {ex.Message}");
        }
    }

    [ContextMenu("Test: Load Game")]
    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"{nameof(SaveManager)}: No save file found at {SavePath}. Starting fresh.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError($"{nameof(SaveManager)}: Save file deserialized to null. File may be corrupt.");
                return;
            }

            // Restore state
            if (m_playerTransform != null)
            {
                // Use CharacterController.enabled toggle to avoid CC fighting transform sets
                if (m_playerTransform.TryGetComponent(out CharacterController cc))
                {
                    cc.enabled = false;
                    m_playerTransform.position = data.playerPosition.ToVector3();
                    cc.enabled = true;
                }
                else
                {
                    m_playerTransform.position = data.playerPosition.ToVector3();
                }
            }

            if (m_playerHealth != null)
            {
                m_playerHealth.SetHealth(data.playerHealth);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestoreWorldState(data.itemCount, data.chefCatPersuaded, data.fishStolen);
            }

            m_collectedPickups = new HashSet<string>(data.pickupsCollected);
            ApplyCollectedPickupsToScene();

            Debug.Log($"{nameof(SaveManager)}: Game loaded from {SavePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{nameof(SaveManager)}: Load failed — {ex.Message}. Save file may be corrupt.");
        }
    }

    private void ApplyCollectedPickupsToScene()
    {
        // Find all pickups in the scene and destroy any that match a collected ID
        Pickup[] allPickups = FindObjectsOfType<Pickup>();
        foreach (Pickup pickup in allPickups)
        {
            if (m_collectedPickups.Contains(pickup.PickupId))
            {
                Destroy(pickup.gameObject);
            }
        }
    }

    [ContextMenu("Test: Delete Save File")]
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log($"{nameof(SaveManager)}: Save file deleted.");
        }
        else
        {
            Debug.Log($"{nameof(SaveManager)}: No save file to delete.");
        }
    }

   
    // SETTINGS — handled by PlayerPrefs (NOT JsonUtility)
    
    // PlayerPrefs is appropriate here because:
    //   - Settings are simple primitives (a single float for volume)
    //   - Settings should persist across game installs / save resets
    //   - Settings live OUTSIDE game state — they're a user preference, not a game world fact
    
    // If we expand to multiple settings (resolution, key bindings, etc.), each gets its own
    // PlayerPrefs key. We never put game state here because PlayerPrefs can't serialize
    // complex types and is stored in the registry/plist on most platforms.
    

    public void SaveVolumeSetting(float volume)
    {
        PlayerPrefs.SetFloat(VolumePrefsKey, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public float LoadVolumeSetting()
    {
        // Default to full volume if no setting has ever been saved
        return PlayerPrefs.GetFloat(VolumePrefsKey, 1.0f);
    }

    [ContextMenu("Test: Save Volume 0.5")]
    private void TestSaveVolume()
    {
        SaveVolumeSetting(0.5f);
        Debug.Log($"{nameof(SaveManager)}: Volume saved as 0.5 to PlayerPrefs (key: '{VolumePrefsKey}')");
    }

    [ContextMenu("Test: Load Volume")]
    private void TestLoadVolume()
    {
        float vol = LoadVolumeSetting();
        Debug.Log($"{nameof(SaveManager)}: Volume loaded from PlayerPrefs: {vol}");
    }

    [ContextMenu("Test: Clear Volume Setting")]
    private void TestClearVolume()
    {
        PlayerPrefs.DeleteKey(VolumePrefsKey);
        PlayerPrefs.Save();
        Debug.Log($"{nameof(SaveManager)}: Volume PlayerPrefs key cleared.");
    }
}