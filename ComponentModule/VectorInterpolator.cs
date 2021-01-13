using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorInterpolator : InterpolatorComponent<Vector3>
{
    public override void Invoke(float point)
    {
        Vector3.Lerp(AValue,BValue,inverselerp(point));
    }
}
