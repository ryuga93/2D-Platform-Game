using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsablePlatform : GroundEffector
{
    [SerializeField] float fallSpeed = 10f;
    public float FallSpeed
    {
        get => fallSpeed;
        set => fallSpeed = value;
    }

    [SerializeField] float delayTime = 0.5f;
    public float DelayTime
    {
        get => delayTime;
        set => delayTime = value;
    }

    [SerializeField] Vector3 difference;
    public Vector3 Difference
    {
        get => difference;
        set => difference = value;
    }

    bool _isPlatformCollapsing = false;
    Rigidbody2D _rigidbody;
    Vector3 _lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _lastPosition = transform.position;

        if (_isPlatformCollapsing)
        {
            _rigidbody.AddForce(Vector2.down * fallSpeed);

            if (_rigidbody.velocity.y == 0f)
            {
                _isPlatformCollapsing = false;
                _rigidbody.bodyType = RigidbodyType2D.Static;
            }
        }
    }

    void LateUpdate()
    {
        difference = transform.position - _lastPosition;
    }

    public void CollapsePlatform()
    {
        StartCoroutine(CollapsePlatformCoroutine());
    }

    IEnumerator CollapsePlatformCoroutine()
    {
        yield return new WaitForSeconds(delayTime);
        _isPlatformCollapsing = true;

        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidbody.freezeRotation = true;
        _rigidbody.gravityScale = 1f;
        _rigidbody.mass = 1000f;
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }
}
