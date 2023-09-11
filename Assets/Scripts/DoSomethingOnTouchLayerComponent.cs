using System;
using UnityEngine;

[RequireComponent(typeof(DoSomethingOnTouchLayerController))]
public abstract class DoSomethingOnTouchLayerComponent : RequiresEnvironmentManager
{
    public abstract void Effect();
}
