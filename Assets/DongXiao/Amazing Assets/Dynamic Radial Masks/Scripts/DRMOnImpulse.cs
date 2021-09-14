using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmazingAssets
{
    namespace DynamicRadialMasks
    {
        public class DRMOnImpulse : MonoBehaviour
        {
            public DRMAnimatableObjectsPool drmObjectsPool;

            public float impulseFrequency = 1;
            float deltaTime;


            public DRMAnimatableObject drmObject;


            private void Start()
            {
                deltaTime = impulseFrequency;
            }

            void Update()
            {
                deltaTime += Time.deltaTime;
                if(deltaTime > impulseFrequency)
                {
                    deltaTime = 0;

                    drmObjectsPool.AddItem(transform.position, drmObject);
                }
            }
        }
    }
}