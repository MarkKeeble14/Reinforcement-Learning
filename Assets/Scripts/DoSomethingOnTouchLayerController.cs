using System;
using System.Collections.Generic;
using UnityEngine;

public class DoSomethingOnTouchLayerController : MonoBehaviour
{
    [SerializeField]
    private SerializableDictionary<LayerMask, List<DoSomethingOnTouchLayerComponent>> onTouchLayer
        = new SerializableDictionary<LayerMask, List<DoSomethingOnTouchLayerComponent>>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (LayerMask layerToTouch in onTouchLayer.Keys())
        {
            if (LayerMaskHelper.IsInLayerMask(collision.gameObject, layerToTouch))
            {
                foreach (DoSomethingOnTouchLayerComponent comp in onTouchLayer[layerToTouch])
                {
                    comp.Effect();
                }
            }
        }
    }
}
