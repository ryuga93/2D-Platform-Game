using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerAnimationController : MonoBehaviour
{
    PlayerController _playerController;
    CharacterController2D _characterController;
    Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _characterController = GetComponent<CharacterController2D>();
        _animator = GetComponentInChildren<Animator>();

        _playerController.OnDoubleJump += PlayDoubleJump;
        _playerController.OnPowerJump += PlayPowerJump;
        _playerController.OnStomp += PlayStomp;
        _playerController.OnStartDash += PlayDash;
    }

    // Update is called once per frame
    void Update()
    {
        _animator.SetFloat("horizontalMovement", Mathf.Abs(_playerController.MoveDirection.x));
        _animator.SetFloat("verticalMovement", _playerController.MoveDirection.y);

        _animator.SetBool("isGrounded", _characterController.IsBelowExist);
        _animator.SetBool("isGliding", _playerController.IsGliding);
        _animator.SetBool("isCrouching", _playerController.IsDucking);
        _animator.SetBool("isInWater", _characterController.IsSubmerged);

        if ((_characterController.IsLeftExist || _characterController.IsRightExist) && !_characterController.IsBelowExist)
        {
            _animator.SetBool("isOnWall", true);
        }
        else
        {
            _animator.SetBool("isOnWall", false);
        }
    }

    void PlayDoubleJump(object sender, EventArgs e)
    {
        // _animator.Play("doubleJump");
        _animator.SetTrigger("doubleJump");
    }

    void PlayPowerJump(object sender, EventArgs e)
    {
        _animator.Play("powerJump");
    }

    void PlayStomp(object sender, EventArgs e)
    {
        _animator.Play("stomp");
    }

    void PlayDash(object sender, EventArgs e)
    {
        _animator.Play("slide");
    }
}
