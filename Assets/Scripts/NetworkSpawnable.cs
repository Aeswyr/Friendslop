using System.Collections.Generic;
using UnityEngine;

public class NetworkSpawnable : MonoBehaviour
{
    [SerializeField] private List<GameObject> randomizedPrefabs;

    public GameObject GetSpawnable()
    {
        if (randomizedPrefabs != null)
        {
            int index = Random.Range(-1, randomizedPrefabs.Count);
            if (index != -1)
            {
                var newObj = Instantiate(randomizedPrefabs[index], transform.position, transform.rotation, transform.parent);
                Destroy(gameObject);
                return newObj;
            }
        }
        return gameObject;
    }
}
