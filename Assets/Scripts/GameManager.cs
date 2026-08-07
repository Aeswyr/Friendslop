using Mirror;
using UnityEngine;

public class GameManager : NetworkSingleton<GameManager>
{
    [SerializeField] private ItemInfoDictionary items;

    [SerializeField] private Transform levelHolder;
    [SerializeField] private GameObject storeLevel;
    [SerializeField] private GameObject[] outdoorLevels;
    [SerializeField] private GameObject[] indoorLevels;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadLevel(storeLevel);
    }

    public void SpawnItem(ItemID item, Vector3 position, Quaternion rotation, Vector3 startingVelocity = default, bool gravityEnabled = true)
    {
        SendSpawnedItem(item, position, rotation, startingVelocity, gravityEnabled);

        [Command(requiresAuthority = false)] void SendSpawnedItem(ItemID item, Vector3 position, Quaternion rotation, Vector3 startingVelocity, bool gravityEnabled)
        {
            GameObject obj = Instantiate(items.GetItemInfo(item).prefab, position, rotation);
            
            Rigidbody rbody = obj.GetComponent<Rigidbody>();
            rbody.linearVelocity = startingVelocity;
            rbody.useGravity = gravityEnabled;

            NetworkServer.Spawn(obj);
        }
    }

    public void LoadLevel(GameObject prefab)
    {

        foreach (Transform child in levelHolder)
            Destroy(child.gameObject);
        
        Instantiate(prefab, levelHolder);

        foreach (var spawn in FindObjectsByType<NetworkSpawnable>(FindObjectsSortMode.None))
        {

            if (isServer)
                NetworkServer.Spawn(spawn.GetSpawnable());
            else 
                Destroy(spawn.gameObject);
        }
    }
}
