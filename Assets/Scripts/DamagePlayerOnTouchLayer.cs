public class DamagePlayerOnTouchLayer : DoSomethingOnTouchLayerComponent
{
    public override void Effect()
    {
        environmentManager.DamagePlayer();
    }
}
