using UnityEngine;

public class MagnifyingGlass : MonoBehaviour
{
    public Camera lensCam;
    public Renderer lensRenderer; // Assign the lens mesh renderer in the inspector
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        Vector3 toMainCamera = mainCam.transform.position - transform.position;
        Vector3 forward = -toMainCamera.normalized;

        // Ensure lens camera faces the correct direction
        lensCam.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        // Calculate angle difference between the lens's "up" vector and the world's "up"
        Vector3 lensUp = transform.up;
        float angle = Mathf.Atan2(lensUp.x, lensUp.y); // Get rotation in radians

        // Apply the rotation to the material (flip sign if needed)
        lensRenderer.material.SetFloat("_Rotation", -angle);
    }
}
