using System.Collections.Generic;
using UnityEngine;

// Plain C# data class, not a MonoBehaviour. Marked [Serializable] so JsonUtility can
// serialize it. Vector3 doesn't serialize cleanly via JsonUtility in some Unity versions,
// so we use a SerializableVector3 wrapper.

// Each system contributes its state to this class before write, and reads its state back after load.

[System.Serializable]
public class SaveData
{
    public SerializableVector3 playerPosition;
    public int playerHealth;
    public int itemCount;
    public bool chefCatPersuaded;
    public bool fishStolen;
    public List<string> pickupsCollected = new List<string>();
}

[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3() { return new Vector3(x, y, z); }
}