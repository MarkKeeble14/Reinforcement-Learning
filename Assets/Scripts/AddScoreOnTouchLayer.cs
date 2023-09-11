using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddScoreOnTouchLayer : DoSomethingOnTouchLayerComponent
{
    public override void Effect()
    {
        environmentManager.IncreaseScore();
    }
}
