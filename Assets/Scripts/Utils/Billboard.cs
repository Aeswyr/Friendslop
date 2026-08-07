using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 newRotation = Camera.main.transform.eulerAngles;

        transform.eulerAngles = newRotation;
    }
}
