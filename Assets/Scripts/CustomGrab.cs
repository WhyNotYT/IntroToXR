using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CustomGrabHandler : XRGrabInteractable
{
    [SerializeField]
    private float maxGrabDistance = 0.5f; // Maximum distance from the midpoint of the controllers
    
    private Dictionary<IXRSelectInteractor, GrabInfo> activeGrabs = new Dictionary<IXRSelectInteractor, GrabInfo>();
    private bool doubleRotationEnabled = false;
    private Camera xrCamera;

    // Store grab data for each interactor
    private class GrabInfo
    {
        public Vector3 initialPosition;
        public Quaternion initialRotation;
        public Vector3 previousPosition;
        public Quaternion previousRotation;

        public GrabInfo(Vector3 pos, Quaternion rot)
        {
            initialPosition = pos;
            initialRotation = rot;
            previousPosition = pos;
            previousRotation = rot;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        // Critical settings for multi-grab
        selectMode = InteractableSelectMode.Multiple;
        movementType = MovementType.Kinematic;
        
        // Disable the default tracking
        trackPosition = false;
        trackRotation = false;
        
        // Allow movement after being grabbed
        throwOnDetach = false;
        
        // Prevent the object from snapping to the hand
        attachTransform = null;

        // Find XR Camera
        xrCamera = Camera.main;
    }

    public void ToggleDoubleRotation()
    {
        doubleRotationEnabled = !doubleRotationEnabled;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is IXRSelectInteractor interactor)
        {
            Vector3 interactorPosition = interactor.transform.position;
            Quaternion interactorRotation = interactor.transform.rotation;
            
            activeGrabs[interactor] = new GrabInfo(interactorPosition, interactorRotation);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactorObject is IXRSelectInteractor interactor)
        {
            activeGrabs.Remove(interactor);
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                UpdateTransform();
            }
        }
    }

    private Vector3 GetControllersCenter()
    {
        if (activeGrabs.Count == 0) return transform.position;
        
        Vector3 sum = Vector3.zero;
        foreach (var grab in activeGrabs)
        {
            sum += grab.Key.transform.position;
        }
        return sum / activeGrabs.Count;
    }

    private Vector3 ClampPosition(Vector3 proposedPosition)
    {
        // Get the center point between controllers
        Vector3 controllersCenter = GetControllersCenter();
        
        // Calculate the vector from controllers center to the proposed position
        Vector3 toProposed = proposedPosition - controllersCenter;
        
        // If the distance is greater than maxGrabDistance, clamp it
        if (toProposed.magnitude > maxGrabDistance)
        {
            return controllersCenter + (toProposed.normalized * maxGrabDistance);
        }
        
        return proposedPosition;
    }

    private void UpdateTransform()
    {
        Vector3 totalTranslation = Vector3.zero;
        Quaternion totalRotation = Quaternion.identity;

        foreach (var grab in activeGrabs)
        {
            var interactor = grab.Key;
            var grabInfo = grab.Value;

            // Calculate position delta
            Vector3 currentPosition = interactor.transform.position;
            Vector3 positionDelta = currentPosition - grabInfo.previousPosition;
            totalTranslation += positionDelta;

            // Calculate rotation delta
            Quaternion currentRotation = interactor.transform.rotation;
            Quaternion rotationDelta = currentRotation * Quaternion.Inverse(grabInfo.previousRotation);

            // Apply double rotation if enabled
            if (doubleRotationEnabled)
            {
                float angle;
                Vector3 axis;
                rotationDelta.ToAngleAxis(out angle, out axis);
                rotationDelta = Quaternion.AngleAxis(angle * 2f, axis);
            }

            totalRotation = rotationDelta * totalRotation;

            // Calculate orbital rotation
            Vector3 toObject = transform.position - currentPosition;
            Vector3 rotatedPosition = rotationDelta * toObject;
            totalTranslation += rotatedPosition - toObject;

            // Update previous transform
            grabInfo.previousPosition = currentPosition;
            grabInfo.previousRotation = currentRotation;
        }

        // Calculate proposed new position
        Vector3 proposedPosition = transform.position + totalTranslation;
        
        // Clamp the position
        Vector3 clampedPosition = ClampPosition(proposedPosition);
        
        // Apply clamped position and rotation
        transform.position = clampedPosition;
        transform.rotation = totalRotation * transform.rotation;
    }
}