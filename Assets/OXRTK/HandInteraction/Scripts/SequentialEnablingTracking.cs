    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialEnablingTracking : MonoBehaviour
{
    [SerializeField] private IKNodeTracker leftT;
    [SerializeField] private IKNodeTracker rightT;
    private int loopCount = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartSequence());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator StartSequence()
    {
        rightT.enabled = true;

        yield return new WaitForSeconds(.4f);

        leftT.enabled = true;

        yield return null;
    }
}
