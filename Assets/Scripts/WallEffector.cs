using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class WallEffector : MonoBehaviour
{
    [SerializeField] WallType wallType;
    public WallType WallType
    {
        get => wallType;
        set => wallType = value;
    }

    [SerializeField] bool isRunnable;
    public bool IsRunnable
    {
        get => isRunnable;
        set => isRunnable = value;
    }

    [SerializeField] bool isJumpable;
    public bool IsJumpable
    {
        get => isJumpable;
        set => isJumpable = value;
    }

    [SerializeField] float wallSlideAmount;
    public float WallSlideAmount
    {
        get => wallSlideAmount;
        set => wallSlideAmount = value;
    }
}
