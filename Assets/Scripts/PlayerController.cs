using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GlobalTypes;
using System;

public class PlayerController : MonoBehaviour
{
    //player properties
    [Header("Player Properties")]
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float creepSpeed = 5f;
    [SerializeField] float gravity = 20f;
    [SerializeField] float jumpSpeed = 15f;
    [SerializeField] float doubleJumpSpeed = 10f;
    [SerializeField] float xWallJumpSpeed = 15f;
    [SerializeField] float yWallJumpSpeed = 15f;
    [SerializeField] float wallRunAmount = 8f;
    [SerializeField] float wallSlideAmount = 0.1f;
    [SerializeField] float glideTime = 2f;
    [SerializeField] float glideDecendAmount = 2f;
    [SerializeField] float powerJumpSpeed = 40f;
    [SerializeField] float powerJumpWaitTime = 1.5f;
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float dashTime = 0.2f;
    [SerializeField] float dashCooldownTime = 1f;
    [SerializeField] float groundSlamSpeed = 60f;
    [SerializeField] float deadzoneValue = 0.15f;
    [SerializeField] float swimSpeed = 150f;
    [SerializeField] float wallJumpDelay = 0.4f;

    [Header("Player States")]
    [SerializeField] bool isJumping;
    [SerializeField] bool isDoubleJumping;
    [SerializeField] bool isTripleJumping;
    [SerializeField] bool isWallJumping;
    [SerializeField] bool isWallRunning;
    [SerializeField] bool isWallSliding;
    [SerializeField] bool isDucking;
    [SerializeField] bool isCreeping;
    [SerializeField] bool isGliding;
    [SerializeField] bool isPowerJumping;
    [SerializeField] bool isDashing;
    [SerializeField] bool isGroundSlamming;
    [SerializeField] bool isSwimming;

    [Header("Player Abilities")]
    //abilities toggle flags
    [SerializeField] bool canDoubleJump;
    [SerializeField] bool canTripleJump;
    [SerializeField] bool canWallJump;
    [SerializeField] bool canJumpAfterWallJump;
    [SerializeField] bool canWallRun;
    [SerializeField] bool canMultipleWallRun;
    [SerializeField] bool canWallSlide;
    [SerializeField] bool canGlide;
    [SerializeField] bool canGlideAfterWallContact;
    [SerializeField] bool canPowerJump;
    [SerializeField] bool canGroundDash;
    [SerializeField] bool canAirDash;
    [SerializeField] bool canGroundSlam;
    [SerializeField] bool canSwim;

    //input flags
    bool _startJump;
    bool _releaseJump;
    bool _isHoldJump;

    Vector2 _input;
    Vector2 _moveDirection;
    CharacterController2D _characterController;
    bool _ableToWallRun = true;

    CapsuleCollider2D _capsuleCollider;
    Vector2 _originalColliderSize;
    // Remove when not needed
    SpriteRenderer _spriteRenderer;

    float _currentGlideTime;
    bool _startGlide;

    float _powerJumpTimer;
    float _dashTimer;

    float _jumpPadAmount = 15f;
    float _jumpPadAdjustment = 0f;
    Vector2 _tempVelocity;

    bool _inAirControl = true;

    public Vector2 MoveDirection => _moveDirection;
    public bool IsGliding => isGliding;
    public bool IsDucking => isDucking;

    // Events
    public event EventHandler OnDoubleJump;
    public event EventHandler OnPowerJump;
    public event EventHandler OnStomp;
    public event EventHandler OnStartDash;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _originalColliderSize = _capsuleCollider.size;
    }

    // Update is called once per frame
    void Update()
    {
        if (_dashTimer > 0)
        {
            _dashTimer -= Time.deltaTime;
        }

        ApplyDeadzones();
        
        ProcessHorizontalMovement();

        if (_characterController.IsBelowExist)
        {
            OnGround();
        }
        else if (_characterController.IsInAirEffector)
        {
            InAirEffector();
        }
        else if (_characterController.IsInWater)
        {
            InWater();
        }
        else // In the air
        {
            InAir();
        }

        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    private void ApplyDeadzones()
    {
        if (_input.x > -deadzoneValue && _input.x < deadzoneValue)
        {
            _input.x = 0f;
        }

        if (_input.y > -deadzoneValue && _input.y < deadzoneValue)
        {
            _input.y = 0f;
        }
    }

    private void ProcessHorizontalMovement()
    {
        if (!_inAirControl || (isWallJumping && _input.x == 0f))
        {
            return;
        }
        else
        {
            _moveDirection.x = _input.x;

            if (_moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if (_moveDirection.x > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            if (isDashing)
            {
                if (transform.rotation.y == 0)
                {
                    _moveDirection.x = dashSpeed;
                }
                else
                {
                    _moveDirection.x = -dashSpeed;
                }

                _moveDirection.y = 0f;
            }
            else if (isCreeping)
            {
                _moveDirection.x *= creepSpeed;
            }
            else 
            {
                _moveDirection.x *= walkSpeed;
            }
        }
    }

    void OnGround()
    {
        if (_characterController.AirEffectorType == AirEffectorType.Ladder)
        {
            InAirEffector();
            return;
        }

        if (_characterController.HitGroundThisFrame)
        {
            _tempVelocity = _moveDirection;
        }

        // Clear Jump flags on ground
        ClearAirAbilitiesFlags();
        
        // Clear any downward motion when on ground
        _moveDirection.y = 0f;
        
        // Jumping
        Jump();

        // Ducking and creeping
        DuckingAndCreeping();

        JumpPad();
    }

    void JumpPad()
    {
        if (_characterController.GroundType == GroundType.JumpPad)
        {
            _jumpPadAmount = _characterController.JumpPadAmount;

            // if inverted downwards velocity is greater than jump pad amount
            if (-_tempVelocity.y > _jumpPadAmount)
            {
                _moveDirection.y = -_tempVelocity.y * 0.92f;
            }
            else
            {
                _moveDirection.y = _jumpPadAmount;
            }

            // if holding jump button, add a little jump amount each time we bounce
            if (_isHoldJump)
            {
                _jumpPadAdjustment += _moveDirection.y * 0.1f;
                _moveDirection.y += _jumpPadAdjustment;
            }
            else
            {
                _jumpPadAdjustment = 0f;
            }

            // Impose and upper limit to stop exponential jump height
            if (_moveDirection.y > _characterController.JumpPadUpperLimit)
            {
                _moveDirection.y = _characterController.JumpPadUpperLimit;
            }
        }
    }

    private void DuckingAndCreeping()
    {
        if (_input.y < 0f)
        {
            if (!isDucking && !isCreeping)
            {
                _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, _capsuleCollider.size.y / 2);
                _capsuleCollider.offset = new Vector2(0f, -_capsuleCollider.size.y / 2);
                //transform.position = new Vector2(transform.position.x, transform.position.y - _originalColliderSize.y / 4);
                _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");
                isDucking = true;
            }

            _powerJumpTimer += Time.deltaTime;
        }
        else
        {
            if (isDucking || isCreeping)
            {
                RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale,
                                        CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2,
                                        _characterController.LayerMask);

                if (!hitCeiling.collider)
                {
                    _capsuleCollider.size = _originalColliderSize;
                    _capsuleCollider.offset = new Vector2(0f, 0f);
                    //transform.position = new Vector2(transform.position.x, transform.position.y + _originalColliderSize.y / 4);
                    _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
                    isDucking = false;
                    isCreeping = false;
                }
            }

            _powerJumpTimer = 0f;
        }

        if (isDucking && _moveDirection.x != 0)
        {
            isCreeping = true;
            _powerJumpTimer = 0f;
        }
        else
        {
            isCreeping = false;
        }
    }

    private void Jump()
    {
        if (_startJump) // On the ground
        {
            _startJump = false;

            if (canPowerJump && isDucking && _characterController.GroundType != GroundType.OneWayPlatform && _powerJumpTimer > powerJumpWaitTime)
            {
                OnPowerJump?.Invoke(this, EventArgs.Empty);
                _moveDirection.y = powerJumpSpeed;
                StartCoroutine(PowerJumpWaiter());
            }
            // Check One Way Platform
            else if (isDucking && _characterController.GroundType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(true));
            }
            else
            {
                _moveDirection.y = jumpSpeed;
            }

            isJumping = true;
            _ableToWallRun = true;

            _characterController.DisableGroundCheck();
            _characterController.ClearMovingPlatform();
        }
    }

    void InAirEffector()
    {
        if (_startJump)
        {
            _characterController.DeactivateAirEffector();
            Jump();
        }

        // Process movement when on ladder
        if (_characterController.AirEffectorType == AirEffectorType.Ladder)
        {
            if (_input.y > 0f)
            {
                _moveDirection.y = _characterController.AirEffectorSpeed;
            }
            else if (_input.y < 0f)
            {
                _moveDirection.y = -_characterController.AirEffectorSpeed;
            }
            else
            {
                _moveDirection.y = 0f;
            }
        }

        // Process movement when in tractor beam
        if (_characterController.AirEffectorType == AirEffectorType.TractorBeam)
        {
            if (_moveDirection.y != 0f)
            {
                _moveDirection.y = Mathf.Lerp(_moveDirection.y, 0f, Time.deltaTime * 4f);
            }
        }

        // Process movement when in an updraft
        if (_characterController.AirEffectorType == AirEffectorType.Updraft)
        {
            if (_input.y <= 0f)
            {
                isGliding = false;
            }
            
            if (isGliding)
            {
                _moveDirection.y = _characterController.AirEffectorSpeed;
            }
            else
            {
                InAir();
            }
        }
    }

    void InWater()
    {
        ClearGroundAbilityFlags();
        AirJump();

        if (_input.y != 0f && canSwim && !_isHoldJump)
        {
            if (_input.y > 0 && !_characterController.IsSubmerged)
            {
                _moveDirection.y = 0f;
            }
            else
            {
                _moveDirection.y = (_input.y * swimSpeed) * Time.deltaTime;
            }
        }
        else if (_moveDirection.y < 0f && _input.y == 0f)
        {
            _moveDirection.y += 2f;
        }

        if (_characterController.IsSubmerged && canSwim)
        {
            isSwimming = true;
        }
        else
        {
            isSwimming = false;
        }
    }

    void ClearAirAbilitiesFlags()
    {
        isJumping = false;
        isDoubleJumping = false;
        isTripleJumping = false;
        isWallJumping = false;
        isWallSliding = false;
        isGroundSlamming = false;
        _startGlide = true;
        _currentGlideTime = glideTime;
        isGliding = false;
    }

    void InAir()
    {
        ClearGroundAbilityFlags();

        AirJump();

        WallRun();

        CalculateGravity();

        if (isGliding && _input.y <= 0f)
        {
            isGliding = false;
        }
    }

    private void WallRun()
    {
        if (_characterController.HitWallThisFrame)
        {
            ClearAirAbilitiesFlags();
        }
        // Wall Run
        if (canWallRun && (_characterController.IsLeftExist || _characterController.IsRightExist))
        {
            // isGliding = false;
            if (_characterController.IsLeftExist && _characterController.LeftWallEffector && !_characterController.IsLeftRunnable)
            {
                return;
            }
            else if (_characterController.IsRightExist && _characterController.RightWallEffector && !_characterController.IsRightRunnable)
            {
                return;
            }

            if (_input.y > 0 && _ableToWallRun)
            {
                _moveDirection.y = wallRunAmount;

                if (_characterController.IsLeftExist && !isWallJumping)
                {
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else if (_characterController.IsRightExist && !isWallJumping)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                StartCoroutine(WallRunWaiter());
            }
        }
        else
        {
            if (canMultipleWallRun)
            {
                StopCoroutine(WallRunWaiter());
                _ableToWallRun = true;
                isWallRunning = false;
            }
        }

        if ((_characterController.IsLeftExist || _characterController.IsRightExist) && canWallRun)
        {
            if (canGlideAfterWallContact)
            {
                _currentGlideTime = glideTime;
            }
            else
            {
                _currentGlideTime = 0f;
            }
        }
    }

    private void AirJump()
    {
        if (_releaseJump)
        {
            _releaseJump = false;
            _moveDirection.y *= 0.5f;
        }

        // Extra Jumping
        if (_startJump)
        {
            // Triple Jump
            if (canTripleJump && !_characterController.IsLeftExist && !_characterController.IsRightExist)
            {
                if (isDoubleJumping && !isTripleJumping)
                {
                    OnDoubleJump?.Invoke(this, EventArgs.Empty);
                    _moveDirection.y = doubleJumpSpeed;
                    isTripleJumping = true;
                }
            }

            // Double Jump
            if (canDoubleJump && !_characterController.IsLeftExist && !_characterController.IsRightExist)
            {
                if (!isDoubleJumping)
                {
                    OnDoubleJump?.Invoke(this, EventArgs.Empty);
                    _moveDirection.y = doubleJumpSpeed;
                    isDoubleJumping = true;
                }
            }

            // Jump in water
            if (_characterController.IsInWater)
            {
                isDoubleJumping = false;
                isTripleJumping = false;
                _moveDirection.y = jumpSpeed;
            }

            // Wall Jump
            if (canWallJump && (_characterController.IsLeftExist || _characterController.IsRightExist))
            {
                if (_characterController.IsLeftExist && _characterController.LeftWallEffector && !_characterController.IsLeftJumpable)
                {
                    return;
                }
                else if (_characterController.IsRightExist && _characterController.RightWallEffector && !_characterController.IsRightJumpable)
                {
                    return;
                }

                if (_moveDirection.x <= 0 && _characterController.IsLeftExist)
                {
                    _moveDirection.x = xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;

                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else if (_moveDirection.x >= 0 && _characterController.IsRightExist)
                {
                    _moveDirection.x = -xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;

                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }

                StartCoroutine(WallJumpWaiter());

                if (canJumpAfterWallJump)
                {
                    isDoubleJumping = false;
                    isTripleJumping = false;
                }
            }

            _startJump = false;
        }
    }

    private void ClearGroundAbilityFlags()
    {
        _powerJumpTimer = 0f;

        if (_moveDirection.y > 0 && (isDucking || isCreeping))
        {
            StartCoroutine(ClearDuckingState());
        }
    }

    void CalculateGravity()
    {
        // Detects something above player
        if (_moveDirection.y > 0 && _characterController.IsAboveExist)
        {
            if (_characterController.CeilingType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(false));
            }
            else
            {
                _moveDirection.y = 0f;
            }   
        }

        // Apply wall slide adjustments
        if (canWallSlide && (_characterController.IsLeftExist || _characterController.IsRightExist))
        {   
            if (_characterController.HitWallThisFrame)
            {
                _moveDirection.y = 0;
            }
            
            if (_moveDirection.y <= 0)
            {
                if (_characterController.IsLeftExist && _characterController.LeftWallEffector)
                {
                    _moveDirection.y -= (gravity * _characterController.LeftSlideModifier) * Time.deltaTime;
                }
                else if (_characterController.IsRightExist && _characterController.RightWallEffector)
                {
                    _moveDirection.y -= (gravity * _characterController.RightSlideModifier) * Time.deltaTime;
                }
                else
                {
                    _moveDirection.y -= (gravity * wallSlideAmount) * Time.deltaTime;
                }

                isWallSliding = true;
            }
            else
            {
                _moveDirection.y -= gravity * Time.deltaTime;
                isWallSliding = false;
            } 
        }
        else if (canGlide && _input.y > 0f && _moveDirection.y < 0.2f) // Glide adjustment
        {
            if (_currentGlideTime > 0f)
            {
                isGliding = true;

                if (_startGlide)
                {
                    _moveDirection.y = 0f;
                    _startGlide = false;
                }

                _moveDirection.y -= glideDecendAmount * Time.deltaTime;
                _currentGlideTime -= Time.deltaTime;
            }
            else
            {
                isGliding = false;
                _moveDirection.y -= gravity * Time.deltaTime;
            }
        }
        //else if (canGroundSlam && !isPowerJumping && _input.y < 0f && _moveDirection.y < 0f) // Ground slam
        else if (isGroundSlamming && !isPowerJumping && _moveDirection.y < 0f)
        {
            OnStomp?.Invoke(this, EventArgs.Empty);
            _moveDirection.y = -groundSlamSpeed;
        }
        else if (!isDashing) // Regular gravity
        {
            _moveDirection.y -= gravity * Time.deltaTime; 
            isWallSliding = false;
        } 
    }

    //#region Input Events
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _startJump = true;
            _releaseJump = false;
            _isHoldJump = true;
        }
        else if (context.canceled)
        {
            _startJump = false;
            _releaseJump = true;
            _isHoldJump = false;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && _dashTimer <= 0)
        {
            if ((canAirDash && !_characterController.IsBelowExist) || (canGroundDash && _characterController.IsBelowExist))
            {
                StartCoroutine(Dash());
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && _input.y < 0f)
        {
            if (canGroundSlam)
            {
                isGroundSlamming = true;
            }
        }
    }
    //#endregion

    //#region Coroutines
    IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        _inAirControl = false;
        yield return new WaitForSeconds(wallJumpDelay);
        _inAirControl = true;
        // isWallJumping = false;
    }

    IEnumerator WallRunWaiter()
    {
        isWallRunning = true;
        yield return new WaitForSeconds(0.5f);
        isWallRunning = false;

        if (!isWallJumping)
        {
            _ableToWallRun = false;
        }
    }

    IEnumerator ClearDuckingState()
    {
        yield return new WaitForSeconds(0.05f);

        RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale,
                                CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2,
                                _characterController.LayerMask);

        if (!hitCeiling.collider)
        {
            _capsuleCollider.size = _originalColliderSize;
            _capsuleCollider.offset = new Vector2(0f, 0f);
            // transform.position = new Vector2(transform.position.x, transform.position.y + _originalColliderSize.y / 4);
            _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
            isDucking = false;
            isCreeping = false;
        }                      
    }

    IEnumerator PowerJumpWaiter()
    {
        isPowerJumping = true;
        yield return new WaitForSeconds(0.8f);

        isPowerJumping = false;
    }

    IEnumerator Dash()
    {
        OnStartDash?.Invoke(this, EventArgs.Empty);
        isDashing = true;
        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        _dashTimer = dashCooldownTime;
    }

    IEnumerator DisableOneWayPlatform(bool checkBelow)
    {
        bool originalCanGroundSlam = canGroundSlam;
        GameObject tempOneWayPlatform = null;

        if (checkBelow)
        {
            Vector2 raycastBelow = transform.position - new Vector3(0, _capsuleCollider.size.y);
            RaycastHit2D hit = Physics2D.Raycast(raycastBelow, Vector2.down,
                            _characterController.RaycastDistance, _characterController.LayerMask);

            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }
        else
        {
            Vector2 raycastAbove = transform.position + new Vector3(0, _capsuleCollider.size.y * 0.5f);
            RaycastHit2D hit = Physics2D.Raycast(raycastAbove, Vector2.up,
                            _characterController.RaycastDistance, _characterController.LayerMask);

            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }

        if (tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = false;
            canGroundSlam = false;
        }

        yield return new WaitForSeconds(0.5f);

        if (tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = true;
            canGroundSlam = originalCanGroundSlam;
        }
    }
    //#endregion
}
