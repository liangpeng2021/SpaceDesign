using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AmazingAssets
{
    namespace DynamicRadialMasks
    {
        public class DRMOnCollision : MonoBehaviour
        {
            public DRMAnimatableObjectsPool drmObjectPool;

            public DRMAnimatableObject drmObject = new DRMAnimatableObject();            


            void OnCollisionEnter(Collision collision)
            {
                if (collision != null && collision.contacts != null && collision.contacts.Length > 0)
                    drmObjectPool.AddItem(collision.contacts[0].point, drmObject);    //Add item 
            }


            private void Reset()
            {
                drmObject.Reset();
            }
        }
    }
}