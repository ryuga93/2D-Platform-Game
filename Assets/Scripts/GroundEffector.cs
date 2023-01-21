using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class GroundEffector : MonoBehaviour
{
    [SerializeField] GroundType groundType;
    public GroundType GroundType
    {
        get { return groundType; }
        set { groundType = value; }
    }
}
