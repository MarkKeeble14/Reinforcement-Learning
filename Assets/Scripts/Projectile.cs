using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float flySpeed;

    public void SetSpeed(float speed)
    {
        flySpeed = speed;
    }

    public IEnumerator FlyToTarget(Vector3 targetPos)
    {
        while (transform.position != targetPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * flySpeed);
            yield return null;
        }

        Destroy(gameObject);
    }
}
