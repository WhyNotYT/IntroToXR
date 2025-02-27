using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public Transform teleportPoint;
    public float distanceThreshold = 3;

    void Update()
    {
        float sqrDistance = Vector3.SqrMagnitude(PlayerController.mainPlayer.transform.position - transform.position);
        // Debug.Log($"Distance squared: {sqrDistance}");

        if (sqrDistance < distanceThreshold * distanceThreshold)
        {
            // Debug.Log($"Teleporting to {teleportPoint.position}");

            CharacterController characterController = PlayerController.mainPlayer.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
                PlayerController.mainPlayer.transform.position = teleportPoint.position;
                characterController.enabled = true;
            }
            else
            {
                PlayerController.mainPlayer.transform.position = teleportPoint.position;
            }

            GameManager.instance.gameStarted = true;
        }
    }
}
