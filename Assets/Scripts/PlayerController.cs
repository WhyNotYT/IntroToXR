using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ArmadrilloHandController leftController;
    public ArmadrilloHandController rightController;

    public float fanForce = 10f;       // Force applied by the tool
    public float gravity = 3f;         // Constant low gravity when not using the tool

    private CharacterController characterController;
    private Vector3 velocity; // Stores gravity effect
    public float health = 3;
    public static PlayerController mainPlayer;
    public Transform gameOverPoint;

    public SpriteRenderer vignette;


    public AudioSource takeDamageAudio;
    public AudioSource dieAudio;

    void Awake()
    {
        mainPlayer = this;
    }



    public void TakeDamage()
    {
        health--;

        takeDamageAudio.Play();
        vignette.color = new Color(1, 1, 1, (1 - ((float)health) / (float)3));

        if (health < 1)
        {
            health = 3;

            characterController.enabled = false;
            this.transform.position = gameOverPoint.position;

            characterController.enabled = true;

            vignette.color = new Color(1, 1, 1, 0);
            dieAudio.Play();
            GameManager.instance.PlayerDied();
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        bool isUsingTool = leftController.IsUsingTool || rightController.IsUsingTool;

        // Apply movement when using tools
        if (!leftController.IsDrillMode && leftController.IsUsingTool)
        {
            characterController.Move(-leftController.transform.up * fanForce * Time.deltaTime);
        }
        if (!rightController.IsDrillMode && rightController.IsUsingTool)
        {
            characterController.Move(-rightController.transform.up * fanForce * Time.deltaTime);
        }

        // Apply gravity only if the tool is not in use
        if (isUsingTool)
        {
            velocity.y = gravity * 0.5f * Time.deltaTime; // Disable gravity when using the tool
        }
        else if (characterController.isGrounded)
        {
            velocity.y = -0.5f; // Small downward force to keep grounded
        }
        else
        {
            velocity.y += Physics.gravity.y * gravity * Time.deltaTime; // Apply constant low gravity
        }

        characterController.Move(velocity * Time.deltaTime);
    }
}
