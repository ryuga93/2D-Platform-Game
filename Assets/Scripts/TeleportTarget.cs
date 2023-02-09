using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportTarget : MonoBehaviour
{
    [SerializeField] bool teleportTargetActive;
    [SerializeField] bool canTeleport;
    [SerializeField] bool followMouse;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite teleportIndicatorGreen;
    [SerializeField] Sprite teleportIndicatorRed;

    GameObject _player;
    CapsuleCollider2D _capsuleCollider;
    CharacterController2D _characterController;
    LayerMask _layerMask;
    Transform _teleportIndicator;

    public bool CanTeleport => canTeleport;
    public bool TeleportTargetActive
    {
        get => teleportTargetActive;
        set => teleportTargetActive = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _capsuleCollider = _player.GetComponent<CapsuleCollider2D>();
        _characterController = _player.GetComponent<CharacterController2D>();
        _layerMask = _characterController.LayerMask;
        _teleportIndicator = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (teleportTargetActive)
        {
            spriteRenderer.enabled = true;

            if (followMouse)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
                transform.position = mouseWorldPosition;
            }

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, _layerMask);

            if (hit.collider)
            {
                _teleportIndicator.transform.position = new Vector2(transform.position.x, hit.point.y);

                Vector3 _playerOffset = new Vector3(0f, _capsuleCollider.size.y / 2 + 0.2f, 0f);
                Collider2D capsuleOverlap = Physics2D.OverlapCapsule(_teleportIndicator.transform.position + _playerOffset, _capsuleCollider.size, CapsuleDirection2D.Vertical, 0f, _layerMask);

                if (!capsuleOverlap)
                {
                    spriteRenderer.sprite = teleportIndicatorGreen;
                    canTeleport = true;
                }
                else
                {
                    spriteRenderer.sprite = teleportIndicatorRed;
                    canTeleport = false;
                }
                
            }
            else
            {
                spriteRenderer.sprite =teleportIndicatorRed;
                canTeleport = false;
            }
        }
        else
        {
            spriteRenderer.enabled = false;
        }
    }
}
