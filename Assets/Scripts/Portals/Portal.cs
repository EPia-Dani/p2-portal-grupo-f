using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.EventBus;
using Player;

namespace Portals
{
    public enum PortalColor
    {
        Blue,
        Orange
    }

    public class Portal : MonoBehaviour
    {
        [SerializeField]
        private PortalColor portalColor;
        [SerializeField]
        private MeshRenderer portalRenderer;
        [SerializeField]
        private MeshRenderer frameRenderer;
        [SerializeField]
        public GameObject linkedPortal;
        
        [SerializeField]
        private Vector3 m_targetScale;
        [SerializeField]
        private float m_portalGrowthDuration = 0.15f;
        
        private Camera mainCamera;
        
        public bool IsPlaced { get; private set; }
        
        public MeshRenderer Renderer => portalRenderer;

        public GameObject attachedSurface { get; private set; }
        public Transform portalCheckerParent;
        private List<Transform> portalCheckers = new List<Transform>();

        private void Awake()
        {
            // Populate portalCheckers list from portalCheckerParent children
            foreach (Transform child in portalCheckerParent)
            {
                portalCheckers.Add(child);
            }
            
            // Set initial target scale to current scale of portal renderer
            m_targetScale = portalRenderer.transform.localScale;
            
            // Set camera reference
            mainCamera = Camera.main;
            
            // Set attachedSurface with a Raycast
            if (Physics.Raycast(transform.position + transform.forward * 0.1f, -transform.forward, out var hit, 1f))
            {
                attachedSurface = hit.collider.gameObject;
            }
        }

        private void OnEnable()
        {
            // Subscribe to the appropriate event based on portal color
            if (portalColor == PortalColor.Blue)
            {
                EventBus<PortalEventBlue>.Subscribe(OnPortalEventBlue);
            }
            else if (portalColor == PortalColor.Orange)
            {
                EventBus<PortalEventOrange>.Subscribe(OnPortalEventOrange);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (portalColor == PortalColor.Blue)
            {
                EventBus<PortalEventBlue>.Unsubscribe(OnPortalEventBlue);
            }
            else if (portalColor == PortalColor.Orange)
            {
                EventBus<PortalEventOrange>.Unsubscribe(OnPortalEventOrange);
            }
        }

        private void OnPortalEventBlue(PortalEventBlue eventData)
        {
            SetPortalTransform(eventData.destPosition, eventData.destRotation, eventData.destScale, eventData.destObject);
        }

        private void OnPortalEventOrange(PortalEventOrange eventData)
        {
            SetPortalTransform(eventData.destPosition, eventData.destRotation, eventData.destScale, eventData.destObject);
        }

        private void SetPortalTransform(Vector3 position, Quaternion rotation, Vector3 scale, GameObject colliderGameObject)
        {
            // Set portal scale
            var originalScale = transform.localScale;
            transform.localScale = scale;
            
            // Move portal checkers to the desired position and rotation
            portalCheckerParent.position = position;
            portalCheckerParent.rotation = rotation;
            
            // Check for valid placement using portal checkers
            foreach (var checker in portalCheckers)
            {
                Vector3 directionToChecker = checker.position - mainCamera.transform.position;
                float distanceToChecker = directionToChecker.magnitude +.5f;
                
                // Raycast from camera position towards checker position
                if (Physics.Raycast(mainCamera.transform.position, directionToChecker.normalized, out var hit, distanceToChecker))
                {
                    if (colliderGameObject != hit.collider.gameObject)
                    {
                        // If surface differs, placement is invalid unless said surface is the portal itself and there's valid placement behind it
                        if (hit.collider.transform.parent != transform || !Physics.Raycast(checker.position,
                                -checker.forward, .5f, LayerMask.GetMask("PortalAble")))
                        {
                            portalCheckerParent.position = transform.position;
                            portalCheckerParent.rotation = transform.rotation;
                            transform.localScale = originalScale;
                            return;
                        }
                    }
                } else { 
                    // If raycast does not hit anything, placement is invalid
                    portalCheckerParent.position = transform.position;
                    portalCheckerParent.rotation = transform.rotation;
                    transform.localScale = originalScale;
                    return;
                }
            }
            
            transform.position = position;
            transform.rotation = rotation;
            IsPlaced = true;
            attachedSurface = colliderGameObject;
            
            // Return portal checkers to original position
            portalCheckerParent.position = transform.position;
            portalCheckerParent.rotation = transform.rotation;
            
            // Enable the portal renderer if it was disabled
            if (portalRenderer)
            {
                portalRenderer.enabled = true;
            }
            
            // Start the portal growth effect
            StartCoroutine(PortalGrowthEffect(m_portalGrowthDuration));
        }
        
        public IEnumerator PortalGrowthEffect(float duration)
        {
            float elapsedTime = 0f;
            Vector3 initialScale = Vector3.zero;
            Vector3 targetScale = m_targetScale;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                Vector3 nextScale = Vector3.Lerp(initialScale, targetScale, t);
                portalRenderer.transform.localScale = nextScale;
                frameRenderer.transform.localScale = nextScale;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            portalRenderer.transform.localScale = targetScale;
            frameRenderer.transform.localScale = targetScale;
        }
        
        public void ClearPortal()
        {
            IsPlaced = false;
            if (portalRenderer)
            {
                portalRenderer.enabled = false;
            }
        }
    }
}
