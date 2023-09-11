using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToNewCellOnTouchLayer : DoSomethingOnTouchLayerComponent
{
    public override void Effect()
    {
        transform.position = environmentManager.GetRandomUnobstructedSpotOnGrid().transform.position;
    }
}
