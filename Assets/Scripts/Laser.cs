using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : GridCellData
{
    [SerializeField] private LineRenderer lineRenderer;

    public bool IsInUse => isFiring || isPrepping;
    private bool isFiring;
    private bool isPrepping;

    [SerializeField] private LayerMask laserLayer;
    [SerializeField] private float widthIncreaseRate;
    [SerializeField] private float maxWidthMultiplier;
    [SerializeField] private float widthDecreaseRate;
    [SerializeField] private float followSpeed = 1;

    [SerializeField] private Projectile laserProjectilePrefab;

    private EnvironmentManager environmentManager;

    private Vector3 targetPos;

    public void SetEnvironmentManager(EnvironmentManager environmentManager)
    {
        this.environmentManager = environmentManager;
    }

    public void Reset()
    {
        StopAllCoroutines();
        ResetState();
    }

    private void ResetState()
    {
        isPrepping = false;
        isFiring = false;
        lineRenderer.enabled = false;
        lineRenderer.widthMultiplier = 0;
    }

    public IEnumerator Prep(Transform target, float delay, Action onEnd)
    {
        isPrepping = true;

        Vector2 direction = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(target.position, direction, Mathf.Infinity, laserLayer);
        targetPos = hit.transform.position;
        lineRenderer.enabled = true;
        lineRenderer.widthMultiplier = 0;

        float t = 0;
        while (t < delay)
        {
            // Debug.Log(this + ": Prep");
            direction = target.position - transform.position;
            hit = Physics2D.Raycast(target.position, direction, Mathf.Infinity, laserLayer);
            targetPos = Vector3.MoveTowards(targetPos, hit.transform.position, Time.deltaTime * followSpeed);

            lineRenderer.widthMultiplier = Mathf.Lerp(lineRenderer.widthMultiplier, maxWidthMultiplier, Time.deltaTime * widthIncreaseRate);
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, targetPos);
            t += Time.deltaTime;

            yield return null;
        }

        isPrepping = false;

        onEnd?.Invoke();
    }

    public IEnumerator Fire(float delay, Action onEnd)
    {
        isFiring = true;

        // Wait
        yield return new WaitForSeconds(delay);

        yield return StartCoroutine(Cooldown());

        lineRenderer.enabled = false;

        // Debug.Log(this + ": Fire");
        // Fire Projectile
        Projectile spawned = Instantiate(laserProjectilePrefab, transform.position, Quaternion.identity);
        spawned.StartCoroutine(spawned.FlyToTarget(targetPos));

        foreach (RequiresEnvironmentManager comp in spawned.GetComponents<RequiresEnvironmentManager>())
        {
            comp.SetEnvironmentManager(environmentManager);
        }

        isFiring = false;
        onEnd?.Invoke();
    }

    private IEnumerator Cooldown()
    {
        while (lineRenderer.widthMultiplier > 0)
        {
            lineRenderer.widthMultiplier = Mathf.MoveTowards(lineRenderer.widthMultiplier, 0, Time.deltaTime * widthDecreaseRate);

            yield return null;
        }
    }
}
