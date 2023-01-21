using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : GroundEffector
{
    [SerializeField] float jumpPadAmount = 15f;
    public float JumpPadAmount
    {
        get => jumpPadAmount;
        set => jumpPadAmount = value;
    }

    [SerializeField] float jumpPadUpperLimit = 30f;
    public float JumpPadUpperLimit
    {
        get => jumpPadUpperLimit;
        set => jumpPadUpperLimit = value;
    }
}
