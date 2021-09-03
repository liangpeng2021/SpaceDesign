namespace OXRTK.ARHandTracking
{
    public enum FingerTipJointID
    {
        Thumb = 20, Index = 16, Middle = 12, Ring = 8, Pinky = 4
    }

    public enum HandJointID
    {
        Wrist = 0, IndexNode1 = 13, MiddleNode1 = 9, ThumbNode2 = 18
    }

    public enum TransformEventType
    {
        OnTranslateStart,
        OnTranslateEnd,
        OnRotateStart,
        OnRotateEnd,
        OnScaleStart,
        OnScaleEnd
    }

    public enum PointerEventType
    {
        OnPointerStart,
        OnPointerEnd,
        OnPinchDown,
        OnPinchUp,
        OnDragging,
        None
    }

    public enum HandInteractionType
    {
        None = 0,
        RayInteraction = 1,
        PhysicalInteraction = 2,
        UiIneraction = 3,
        LaserInteraction = 4,
    }

    public enum InteractionPriority
    {
        RayInteraction = 1,
        PhysicalInteraction = 0,
        UiIneraction = 0,
        LaserInteraction = 100,
        None = 200,
    }
}
