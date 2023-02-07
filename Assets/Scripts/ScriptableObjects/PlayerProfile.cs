using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProfile", menuName = "CharacterController2D/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    [Header("Player Properties")]
    public float walkSpeed = 10f;
    public float accelerationAmount = 1f;
    public float decelerationAmount = 1f;
    public float creepSpeed = 5f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;
    public float wallRunAmount = 8f;
    public float wallSlideAmount = 0.1f;
    public float glideTime = 2f;
    public float glideDecendAmount = 2f;
    public float powerJumpSpeed = 40f;
    public float powerJumpWaitTime = 1.5f;
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    public float dashCooldownTime = 1f;
    public float groundSlamSpeed = 60f;
    public float deadzoneValue = 0.15f;
    public float swimSpeed = 150f;
    public float wallJumpDelay = 0.4f;

    [Header("Player Abilities")]
    //abilities toggle flags
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun;
    public bool canMultipleWallRun;
    public bool canWallSlide;
    public bool canGlide;
    public bool canGlideAfterWallContact;
    public bool canPowerJump;
    public bool canGroundDash;
    public bool canAirDash;
    public bool canGroundSlam;
    public bool canSwim;
}
