using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTeleport : MonoBehaviour
{
    [SerializeField] TeleportTarget teleportTarget;

    public void OnTeleport(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            teleportTarget.TeleportTargetActive = true;
        }
        else if (context.canceled)
        {
            teleportTarget.TeleportTargetActive = false;
            MoveToPosition();
        }
    }

    void MoveToPosition()
    {
        if (teleportTarget.CanTeleport)
        {
            transform.position = teleportTarget.transform.position;
        }
        else
        {
            Debug.Log("Can't teleport to that location!");
        }
    }
}
