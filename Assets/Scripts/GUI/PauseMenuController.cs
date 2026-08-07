using UnityEngine;
using Mirror;
using Steamworks;

public class PauseMenuController : Singleton<PauseMenuController>
{
    [SerializeField] private GameObject menuParent;
    private bool paused;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        paused = false;
        menuParent.SetActive(paused);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (InputHandler.Instance.menu.pressed)
        {
            paused = !paused;
            menuParent.SetActive(paused);
            if (paused)
            {
                Cursor.lockState = CursorLockMode.None;
            } else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void OnResume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        paused = false;
        menuParent.SetActive(paused);
    }

    public void OnSettings()
    {
        
    }

    public void OnMenu()
    {
        SteamMatchmaking.LeaveLobby(SteamHandler.Instance.LobbyID);
		if (NetworkServer.activeHost)
		{
			NetworkManager.singleton.StopHost();
		}
		else
		{
			NetworkManager.singleton.StopClient();
		}
    }

    public bool IsPaused()
    {
        return paused;
    }
}
