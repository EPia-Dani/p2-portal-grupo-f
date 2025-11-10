using System;
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
        
        public bool IsPlaced { get; private set; }
        
        public MeshRenderer Renderer => portalRenderer;

        public Transform portalCheckerParent;
        public List<Transform> portalCheckers = new List<Transform>();

        private void Awake()
        {
            // Populate portalCheckers list from portalCheckerParent children
            foreach (Transform child in portalCheckerParent)
            {
                portalCheckers.Add(child);
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
            SetPortalTransform(eventData.destPosition, eventData.destRotation);
        }

        private void OnPortalEventOrange(PortalEventOrange eventData)
        {
            SetPortalTransform(eventData.destPosition, eventData.destRotation);
        }

        private void SetPortalTransform(Vector3 position, Quaternion rotation)
        {
            // Move portal checkers to the desired position and rotation
            portalCheckerParent.position = position;
            portalCheckerParent.rotation = rotation;
            
            // Check for valid placement using portal checkers
            foreach (var checker in portalCheckers)
            {
                // Raycast from checker position backwards to see if it hits anything
                if (!Physics.Raycast(checker.position, -checker.forward, out RaycastHit hit, 0.1f, LayerMask.GetMask("PortalAble")))
                {
                    // If checker does not hit, placement is invalid, return portal checkers to original position
                    portalCheckerParent.position = transform.position;
                    portalCheckerParent.rotation = transform.rotation;
                    return;
                }
            }
            
            transform.position = position;
            transform.rotation = rotation;
            IsPlaced = true;
            
            // Return portal checkers to original position
            portalCheckerParent.position = transform.position;
            portalCheckerParent.rotation = transform.rotation;
            
            // Enable the portal renderer if it was disabled
            if (portalRenderer)
            {
                portalRenderer.enabled = true;
            }
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

