using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] Vector2 slopeNormal;
    [SerializeField] float slopeAngle;
    [SerializeField] float slopeAngleLimit = 45f;
    [SerializeField] float downForceAdjustment = 1.2f;
    [SerializeField] float waterForce = 250f;

    [SerializeField] bool isSubmerged;
    public bool IsSubmerged
    {
        get => isSubmerged;
        set => isSubmerged = value;
    }

    [SerializeField] bool isInWater;
    public bool IsInWater
    {
        get => isInWater;
        set => isInWater = false;
    }

    [SerializeField] float raycastDistance = 0.2f;
    public float RaycastDistance
    {
        get => raycastDistance;
        set => raycastDistance = value;
    }

    [SerializeField] LayerMask layerMask;
    public LayerMask LayerMask
    {
        get => layerMask;
        set => layerMask = value;
    }

    [SerializeField] GroundType groundType;
    public GroundType GroundType
    {
        get => groundType;
        set => groundType = value;
    }

    [SerializeField] WallType leftWallType;
    public WallType LeftWallType
    {
        get => leftWallType;
        set => leftWallType = value;
    }

    [SerializeField] WallType rightWallType;
    public WallType RightWallType
    {
        get => rightWallType;
        set => rightWallType = value;
    }

    [SerializeField] GroundType ceilingType;
    public GroundType CeilingType
    {
        get => ceilingType;
        set => ceilingType = value;
    }

    [SerializeField] float jumpPadAmount;
    public float JumpPadAmount
    {
        get => jumpPadAmount;
        set => jumpPadAmount = value;
    }

    [SerializeField] float jumpPadUpperLimit;
    public float JumpPadUpperLimit
    {
        get => jumpPadUpperLimit;
        set => jumpPadUpperLimit = value;
    }

    [SerializeField] WallEffector leftWallEffector;
    public WallEffector LeftWallEffector
    {
        get => leftWallEffector;
        set => leftWallEffector = value;
    }

    [SerializeField] bool isLeftRunnable;
    public bool IsLeftRunnable
    {
        get => isLeftRunnable;
        set => isLeftRunnable = value;
    }

    [SerializeField] bool isLeftJumpable;
    public bool IsLeftJumpable
    {
        get => isLeftJumpable;
        set => isLeftJumpable = value;
    }

    [SerializeField] float leftSlideModifier;
    public float LeftSlideModifier
    {
        get => leftSlideModifier;
        set => leftSlideModifier = value;
    }

    [SerializeField] WallEffector rightWallEffector;
    public WallEffector RightWallEffector
    {
        get => rightWallEffector;
        set => rightWallEffector = value;
    }

    [SerializeField] bool isRightRunnable;
    public bool IsRightRunnable
    {
        get => isRightRunnable;
        set => isRightRunnable = value;
    }

    [SerializeField] bool isRightJumpable;
    public bool IsRightJumpable
    {
        get => isRightJumpable;
        set => isRightJumpable = value;
    }

    [SerializeField] float rightSlideModifier;
    public float RightSlideModifier
    {
        get => rightSlideModifier;
        set => rightSlideModifier = value;
    }

    [SerializeField] bool isInAirEffector;
    public bool IsInAirEffector
    {
        get => isInAirEffector;
        set => isInAirEffector = value;
    }
    
    [SerializeField] AirEffectorType airEffectorType;
    public AirEffectorType AirEffectorType
    {
        get => airEffectorType;
        set => airEffectorType = value;
    }

    [SerializeField] float airEffectorSpeed;
    public float AirEffectorSpeed
    {
        get => airEffectorSpeed;
        set => airEffectorSpeed = value;
    }

    [SerializeField] Vector2 airEffectorDirection;
    public Vector2 AirEffectorDirection
    {
        get => airEffectorDirection;
        set => airEffectorDirection = value;
    }

    // flags
    bool _disableGroundCheck;
    bool _inAirLastFrame;
    bool _noSideCollisionLastFrame;
    bool _isBelowExist;
    bool _isLeftExist;
    bool _isRightExist;
    bool _isAboveExist;
    bool _hitGroundThisFrame;
    bool _hitWallThisFrame;
    public bool IsBelowExist => _isBelowExist;
    public bool IsLeftExist => _isLeftExist;
    public bool IsRightExist => _isRightExist;
    public bool IsAboveExist => _isAboveExist;
    public bool HitGroundThisFrame => _hitGroundThisFrame;
    public bool HitWallThisFrame => _hitWallThisFrame;

    Vector2 _moveAmount;
    Vector2 _currentPosition;
    Vector2 _lastPosition;

    Rigidbody2D _rigidbody;
    CapsuleCollider2D _capsuleCollider;

    Vector2[] _raycastPosition = new Vector2[3];
    RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    Transform _tempMovingPlatform;
    Vector2 _movingPlatformVelocity;

    AirEffector _airEffector;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _inAirLastFrame = !_isBelowExist;
        _noSideCollisionLastFrame = (!_isLeftExist && !_isRightExist);

        _lastPosition = _rigidbody.position;

        // Slope adjustment
        if (slopeAngle != 0 && _isBelowExist)
        {
            if ((_moveAmount.x > 0f && slopeAngle > 0f) || (_moveAmount.x < 0f && slopeAngle < 0f))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * _moveAmount.x);
                _moveAmount.y *= downForceAdjustment;
            }
        }

        // Moving Platform adjustment
        if (groundType == GroundType.MovingPlatform)
        {
            // Offset player movement on X with Moving Platform's velocity
            _moveAmount.x += GetMovingPlatformVelocity().x;
            // If platform is moving down
            if (GetMovingPlatformVelocity().y < 0f)
            {
                // Offset player movement on the Y
                _moveAmount.y += GetMovingPlatformVelocity().y;
                _moveAmount.y *= (downForceAdjustment + 0.5f);
            }       
        }

        if (groundType == GroundType.CollapsablePlatform)
        {
            if (GetMovingPlatformVelocity().y < 0f)
            {
                _moveAmount.y += GetMovingPlatformVelocity().y;
                _moveAmount.y *= (downForceAdjustment * 4);
            }
        }

        // Tractor Beam adjustment
        if (_airEffector && airEffectorType == AirEffectorType.TractorBeam)
        {
            Vector2 airEffectorVector = airEffectorDirection * airEffectorSpeed;
            _moveAmount = Vector2.Lerp(_moveAmount, airEffectorVector, Time.deltaTime);
        }

        if (!isInWater)
        {
            _currentPosition = _lastPosition + _moveAmount;
            _rigidbody.MovePosition(_currentPosition);
        }
        else
        {
            if (_rigidbody.velocity.magnitude < 10f)
            {
                _rigidbody.AddForce(_moveAmount * waterForce);
            }
        }

        _moveAmount = Vector2.zero;

        if (!_disableGroundCheck)
        {
            CheckGrounded();
        }

        CheckOtherCollisions();

        if (_isBelowExist && _inAirLastFrame)
        {
            _hitGroundThisFrame = true;
        }
        else
        {
            _hitGroundThisFrame = false;
        }

        if ((_isLeftExist || _isRightExist) && _noSideCollisionLastFrame)
        {
            _hitWallThisFrame = true;
        }
        else
        {
            _hitWallThisFrame = false;
        }
    }

    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, 
                            CapsuleDirection2D.Vertical, 0f, Vector2.down, raycastDistance, layerMask);
        
        if (hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);

            if (groundType == GroundType.MovingPlatform || groundType == GroundType.CollapsablePlatform)
            {
                _tempMovingPlatform = hit.collider.transform;

                if (groundType == GroundType.CollapsablePlatform)
                {
                    _tempMovingPlatform.GetComponent<CollapsablePlatform>().CollapsePlatform();
                }
            }

            slopeNormal = hit.normal;
            slopeAngle = Vector2.SignedAngle(slopeNormal, Vector2.up);

            if (slopeAngle > slopeAngleLimit || slopeAngle < -slopeAngleLimit)
            {
                _isBelowExist = false;
            }
            else
            {
                _isBelowExist = true;
            }

            if (groundType == GroundType.JumpPad)
            {
                JumpPad jumpPad = hit.collider.GetComponent<JumpPad>();
                jumpPadAmount = jumpPad.JumpPadAmount;
                jumpPadUpperLimit = jumpPad.JumpPadUpperLimit;
            }
        }
        else
        {
            groundType = GroundType.None;
            _isBelowExist = false;

            if (_tempMovingPlatform)
            {
                _tempMovingPlatform = null;
            }
        }
    }

    void CheckOtherCollisions()
    {
        //left
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.7f,
                                0f, Vector2.left, raycastDistance * 2, layerMask);

        if (leftHit.collider)
        {
            leftWallType = DetermineWallType(leftHit.collider);
            _isLeftExist = true;

            leftWallEffector = leftHit.collider.GetComponent<WallEffector>();

            if (leftWallEffector)
            {
                isLeftRunnable = leftWallEffector.IsRunnable;
                isLeftJumpable = leftWallEffector.IsJumpable;
                leftSlideModifier = leftWallEffector.WallSlideAmount;
            }
        }
        else
        {
            _isLeftExist = false;
            leftWallType = WallType.None;
        }

        //right
        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.7f,
                                0f, Vector2.right, raycastDistance * 2, layerMask);
        
        if (rightHit.collider)
        {
            rightWallType = DetermineWallType(rightHit.collider);
            _isRightExist = true;

            rightWallEffector = rightHit.collider.GetComponent<WallEffector>();

            if (rightWallEffector)
            {
                isRightRunnable = rightWallEffector.IsRunnable;
                isRightJumpable = rightWallEffector.IsJumpable;
                rightSlideModifier = rightWallEffector.WallSlideAmount;
            }
        }
        else
        {
            _isRightExist = false;
            rightWallType = WallType.None;
        }

        //above
        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size,
                                CapsuleDirection2D.Vertical, 0f, Vector2.up, raycastDistance, layerMask);

        if (aboveHit)
        {
            ceilingType = DetermineGroundType(aboveHit.collider);
            _isAboveExist = true;
        }
        else
        {
            ceilingType = GroundType.None;
            _isAboveExist = false;
        }
    }

    /*
    void CheckGrounded()
    {
        Vector2 raycastOrigin = _rigidbody.position - new Vector2(0, _capsuleCollider.size.y * 0.5f);

        _raycastPosition[0] = raycastOrigin + Vector2.left * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f;
        _raycastPosition[1] = raycastOrigin;
        _raycastPosition[2] = raycastOrigin + Vector2.right * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f;
    
        DrawDebugRays(Vector2.down * raycastDistance, Color.green);

        int numberOfGoundHits = 0;
        for (int i = 0; i < _raycastPosition.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPosition[i], Vector2.down, raycastDistance, layerMask);

            if (hit.collider)
            {
                _raycastHits[i] = hit;
                numberOfGoundHits++;
            }
        }

        if (numberOfGoundHits > 0)
        {
            if (_raycastHits[1].collider)
            {
                groundType = DetermineGroundType(_raycastHits[1].collider);
                slopeNormal = _raycastHits[1].normal;
                slopeAngle = Vector2.SignedAngle(slopeNormal, Vector2.up);
            }
            else
            {
                foreach (RaycastHit2D raycastHit in _raycastHits)
                {
                    if (raycastHit.collider)
                    {
                        groundType = DetermineGroundType(raycastHit.collider);
                        slopeNormal = raycastHit.normal;
                        slopeAngle = Vector2.SignedAngle(slopeNormal, Vector2.up);
                    }
                }
            }

            if ((slopeAngle > slopeAngleLimit) || (slopeAngle < -slopeAngleLimit))
            {
                _isBelowExist = false;
            }
            else
            {
                _isBelowExist = true;
            }
        }
        else
        {
            groundType = GroundType.None;
            _isBelowExist = false;
        }

        System.Array.Clear(_raycastHits, 0, _raycastHits.Length);
    } */

    void DrawDebugRays(Vector2 direction, Color color)
    {
        foreach (Vector2 raycastPos in _raycastPosition)
        {
            Debug.DrawRay(raycastPos, direction, color);
        }
    }

    public void DisableGroundCheck()
    {
        _isBelowExist = false;
        _disableGroundCheck = true;
        StartCoroutine(EnableGroundCheck());
    }

    IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        _disableGroundCheck = false;
    }

    GroundType DetermineGroundType(Collider2D collider)
    {
        if (collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();

            return groundEffector.GroundType;
        }
        
        if (_tempMovingPlatform)
        {
            _tempMovingPlatform = null;
        }

        return GroundType.LevelGeometry;
    }

    WallType DetermineWallType(Collider2D collider)
    {
        if (collider.GetComponent<WallEffector>())
        {
            WallEffector wallEffector = collider.GetComponent<WallEffector>();
            return wallEffector.WallType;
        }
        
        return WallType.Normal;
    }

    Vector2 GetMovingPlatformVelocity()
    {
        if (_tempMovingPlatform && groundType == GroundType.MovingPlatform)
        {
            _movingPlatformVelocity = _tempMovingPlatform.GetComponent<MovingPlatform>().Difference;
            return _movingPlatformVelocity;
        }
        else if (_tempMovingPlatform && groundType == GroundType.CollapsablePlatform)
        {
            _movingPlatformVelocity = _tempMovingPlatform.GetComponent<CollapsablePlatform>().Difference;
            return _movingPlatformVelocity;
        }
        
        return Vector2.zero;
    }

    public void ClearMovingPlatform()
    {
        if (_tempMovingPlatform)
        {
            _tempMovingPlatform = null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<BuoyancyEffector2D>())
        {
            isInWater = true;
        }

        if (other.gameObject.GetComponent<AirEffector>())
        {
            isInAirEffector = true;
            _airEffector = other.gameObject.GetComponent<AirEffector>();

            airEffectorType = _airEffector.AirEffectorType;
            airEffectorSpeed = _airEffector.Speed;
            airEffectorDirection = _airEffector.Direction;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.bounds.Contains(_capsuleCollider.bounds.min) && other.bounds.Contains(_capsuleCollider.bounds.max) && other.gameObject.GetComponent<BuoyancyEffector2D>())
        {
            isSubmerged = true;
        }
        else
        {
            isSubmerged = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<BuoyancyEffector2D>())
        {
            _rigidbody.velocity = Vector2.zero;
            isInWater = false;
        }

        if (other.gameObject.GetComponent<AirEffector>())
        {
            isInAirEffector = false;
            _airEffector.DeactivateEffector();
            _airEffector = null;
            airEffectorType = AirEffectorType.None;
            airEffectorSpeed = 0f;
            airEffectorDirection = Vector2.zero;
        }
    }

    public void DeactivateAirEffector()
    {
        if (_airEffector)
        {
            _airEffector.DeactivateEffector();
        }
    }
}
