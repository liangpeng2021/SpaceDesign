/*
 * Copyright(C) OPPO Limited - All Rights Reserved
 * Proprietary and confidential
 */
namespace XR.Samples
{
    public class XRWindowDoubleScreen_Fullscreen : XRWindowDoubleScreen
    {

        public override void OnWindowVisible(bool visible)
        {
            base.OnWindowVisible(visible);
            if (visible)
            {
                WindowFullscreen = true;
            }
        }

        public override void Create()
        {
            base.Create();
            WindowFullscreen = true;
        }
    }
}