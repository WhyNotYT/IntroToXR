using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ArmadrilloHandController : MonoBehaviour
{
    public InputActionReference triggerAction;
    public InputActionReference grabAction;
    
    // Public read-only properties to expose state
    public bool IsDrillMode => isDrillMode;
    public bool IsUsingTool => isUsingTool;
    public bool IsSwitchingMode => isSwitchingMode;
    public string CurrentAnimationState => currentAnimation;
    
    private Animator animator;
    private bool isDrillMode = true;
    private bool isUsingTool = false;
    private bool isSwitchingMode = false;
    private string currentAnimation = "";
    
    // Animation state names
    private readonly string equipDrillParam = "EquipDrill";
    private readonly string equipFanParam = "EquipFan";
    private readonly string useDrillParam = "UseDrill";
    private readonly string useFanParam = "UseFan";
    private readonly string idleDrillParam = "IdleDrill";
    private readonly string idleFanParam = "IdleFan";

    // Unity Event that other scripts can subscribe to for state changes
    public event System.Action<bool> OnModeChanged;
    public event System.Action<bool> OnToolUseChanged;

    public AudioSource drillAudio;
    public AudioSource fanAudio;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found!");
            return;
        }
        
        // Check if audio sources are assigned
        if (drillAudio == null || fanAudio == null)
        {
            Debug.LogWarning("Audio sources not fully assigned to ArmadrilloHandController!");
        }
        
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPerformed;
        
        grabAction.action.Enable();
        grabAction.action.started += OnGrabStarted;
        grabAction.action.canceled += OnGrabCanceled;
        
        PlayAnimation(idleDrillParam);
    }

    void Update()
    {
        if (isUsingTool && !isSwitchingMode)
        {
            string useAnim = isDrillMode ? useDrillParam : useFanParam;
            
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (!currentState.IsName(useAnim) && currentAnimation != useAnim)
            {
                PlayAnimation(useAnim);
            }
        }
    }

    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        if (!isSwitchingMode)
        {
            StartCoroutine(SwitchMode());
        }
    }

    private IEnumerator SwitchMode()
    {
        isSwitchingMode = true;
        isUsingTool = false;
        OnToolUseChanged?.Invoke(false);
        
        // Stop any playing audio when switching tools
        StopToolSounds();
        
        if (isDrillMode)
        {
            float time = 0f;
            float duration = 0.5f;
            
            while (time < duration)
            {
                float normalizedTime = 1f - (time / duration);
                animator.Play(equipDrillParam, 0, normalizedTime);
                time += Time.deltaTime;
                yield return null;
            }
            
            animator.Play(equipFanParam, 0, 0);
            yield return new WaitForSeconds(0.5f);
            
            isDrillMode = false;
            PlayAnimation(idleFanParam);
            OnModeChanged?.Invoke(false);
        }
        else
        {
            float time = 0f;
            float duration = 0.5f;
            
            while (time < duration)
            {
                float normalizedTime = 1f - (time / duration);
                animator.Play(equipFanParam, 0, normalizedTime);
                time += Time.deltaTime;
                yield return null;
            }
            
            animator.Play(equipDrillParam, 0, 0);
            yield return new WaitForSeconds(0.5f);
            
            isDrillMode = true;
            PlayAnimation(idleDrillParam);
            OnModeChanged?.Invoke(true);
        }
        
        isSwitchingMode = false;
    }

    private void OnGrabStarted(InputAction.CallbackContext context)
    {
        isUsingTool = true;
        OnToolUseChanged?.Invoke(true);

        // Play appropriate sound based on current tool mode
        PlayToolSound();

        if (!isSwitchingMode)
        {
            string useAnim = isDrillMode ? useDrillParam : useFanParam;
            if (currentAnimation != useAnim)
            {
                PlayAnimation(useAnim);
            }
        }
    }

    private void OnGrabCanceled(InputAction.CallbackContext context)
    {
        isUsingTool = false;
        OnToolUseChanged?.Invoke(false);
        
        // Stop tool sounds when grab is released
        StopToolSounds();
        
        if (!isSwitchingMode)
        {
            string idleAnim = isDrillMode ? idleDrillParam : idleFanParam;
            PlayAnimation(idleAnim);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName)
        {
            return;
        }
        animator.Play(animationName, 0, 0);
        currentAnimation = animationName;
    }
    
    // Play the appropriate tool sound based on current mode
    private void PlayToolSound()
    {
        if (isDrillMode)
        {
            if (drillAudio != null && !drillAudio.isPlaying)
            {
                drillAudio.Play();
            }
        }
        else
        {
            if (fanAudio != null && !fanAudio.isPlaying)
            {
                fanAudio.Play();
            }
        }
    }
    
    // Stop all tool sounds
    private void StopToolSounds()
    {
        if (drillAudio != null && drillAudio.isPlaying)
        {
            drillAudio.Stop();
        }
        
        if (fanAudio != null && fanAudio.isPlaying)
        {
            fanAudio.Stop();
        }
    }

    void OnDestroy()
    {
        if (triggerAction != null && triggerAction.action != null)
        {
            triggerAction.action.performed -= OnTriggerPerformed;
        }
        if (grabAction != null && grabAction.action != null)
        {
            grabAction.action.started -= OnGrabStarted;
            grabAction.action.canceled -= OnGrabCanceled;
        }
        
        // Make sure to stop any sounds when the component is destroyed
        StopToolSounds();
    }
}