using Mirror;
using UnityEngine;

public class QuestBoard : NetworkBehaviour
{
    [SerializeField] private float width, height;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!isServer)
            return;
        GameManager.Instance.SpawnItem(ItemID.QUEST_POSTING, 
        transform.position + new Vector3(Random.Range(-width / 2, width / 2), Random.Range(-height / 2, height / 2), 0.1f),
        transform.rotation,
        gravityEnabled: false);
    }

}
