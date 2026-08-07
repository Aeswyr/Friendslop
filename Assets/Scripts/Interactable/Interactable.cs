using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent<PlayerController> action;


    public void OnInteract(PlayerController interactor)
    {
        action.Invoke(interactor);
    }
}
