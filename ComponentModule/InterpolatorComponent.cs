using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InterpolatorComponent<DataType> : Interpolator
{
    [SerializeField]
    protected DataType AValue, BValue;
}
