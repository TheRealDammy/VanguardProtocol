// Assets/_Project/Tests/InstanceFinder.cs — delete after
using UnityEngine;
using VanguardProtocol.Characters;

public class InstanceFinder : MonoBehaviour
{
    private void Start()
    {
        var all = FindObjectsOfType<CharacterBase>();

        Debug.Log($"[InstanceFinder] Found {all.Length} CharacterBase instances:");

        foreach (var c in all)
        {
            string path = GetPath(c.transform);
            Debug.Log($"[InstanceFinder] {c.gameObject.name} " +
                      $"(ID: {c.GetInstanceID()}) | Scene: {c.gameObject.scene.name} | " +
                      $"Path: {path} | Active: {c.gameObject.activeInHierarchy}");
        }
    }

    private string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}