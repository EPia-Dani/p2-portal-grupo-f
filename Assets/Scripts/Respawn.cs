using Core.EventBus;
using Player;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EventBus<RespawnSetEvent>.Invoke(new RespawnSetEvent() { position = other.transform.position });
    }
}
