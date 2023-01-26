using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class AirEffector : MonoBehaviour
{
    [SerializeField] AirEffectorType airEffectorType;
    public AirEffectorType AirEffectorType
    {
        get => airEffectorType;
        set => airEffectorType = value;
    }

    [SerializeField] float speed;
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    [SerializeField] Vector2 direction;
    public Vector2 Direction => direction;

    BoxCollider2D _collider;

    // Start is called before the first frame update
    void Start()
    {
        direction = transform.up;
        _collider = GetComponent<BoxCollider2D>();
    }

    public void DeactivateEffector()
    {
        StartCoroutine(DeactivateEffectorCoroutine());
    }

    IEnumerator DeactivateEffectorCoroutine()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        _collider.enabled = true;
    }
}
