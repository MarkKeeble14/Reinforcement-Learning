
public class DestroyOnTouchLayer : DoSomethingOnTouchLayerComponent
{
    public override void Effect()
    {
        Destroy(gameObject);
    }
}