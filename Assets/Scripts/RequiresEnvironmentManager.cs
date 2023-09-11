using UnityEngine;

public abstract class RequiresEnvironmentManager : MonoBehaviour
{
    protected EnvironmentManager environmentManager;
    public void SetEnvironmentManager(EnvironmentManager environmentManager)
    {
        this.environmentManager = environmentManager;
    }
}
