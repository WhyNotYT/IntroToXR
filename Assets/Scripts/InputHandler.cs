using UnityEngine;
using UnityEngine.InputSystem;


public class Quit : MonoBehaviour
{
    public InputActionReference quitAction;
    public InputActionReference lightAction;
    public InputActionReference teleportAction;
    public LightManager lightManager;

    public Transform playerTeleportTarget;
    public GameObject player;
    private bool playerInRoom = true;
    private Vector3 initialPlayerPosition;
    void Start()
    {
        initialPlayerPosition = player.transform.position;
        
        quitAction.action.Enable();

        quitAction.action.performed += (ctx) =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

        };


        lightAction.action.Enable();

        lightAction.action.performed += (ctx) =>
        {
            lightManager.UpdateLight();

        };

        teleportAction.action.Enable();

        teleportAction.action.performed += (ctx) =>
        {
            if(playerInRoom) {
                // initialPlayerPosition = player.transform.position;
                player.transform.position = playerTeleportTarget.position;
                playerInRoom = false;
                return;
            }

            player.transform.position = initialPlayerPosition;
            playerInRoom = true;

        };
    }
}
