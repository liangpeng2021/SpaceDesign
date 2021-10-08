using OXRTK.ARHandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsColorController : MonoBehaviour
{
    public BoundingBox boundingBox;
    public Color highlightColor = new Color(0.45f, 0.84f, 1f, 1);
    public Color pressedColor = Color.white;
    Color m_NormalColor;

    private void Awake()
    {
        if(boundingBox != null)
        {
            boundingBox.onBoundsInitFinished += HandlerColorInit;
        }
    }

    void HandlerColorInit()
    {
        if (boundingBox.cornerObjects != null)
        {
            for (int i = 0; i < boundingBox.cornerObjects.Length; i++)
            {
                BoundsHandlerColorController handlerColorController =
                    boundingBox.cornerObjects[i].gameObject.AddComponent<BoundsHandlerColorController>();
                handlerColorController.Init(highlightColor, pressedColor);
            }
        }

        if (boundingBox.edgeObjects != null)
        {
            for (int i = 0; i < boundingBox.edgeObjects.Length; i++)
            {
                BoundsHandlerColorController handlerColorController =
                    boundingBox.edgeObjects[i].gameObject.AddComponent<BoundsHandlerColorController>();
                handlerColorController.Init(highlightColor, pressedColor);
            }
        }
    }
}
