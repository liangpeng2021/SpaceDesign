using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class NumberRoll : MonoBehaviour
{
    public int targetNumber = 26;
    public Text numberText;
    public float rate = 0.02f;
    private int currentNumber;

    void OnEnable()
    {
        if (numberText == null)
            numberText = GetComponent<Text>();
        numberText.text = "";
        currentNumber = 0;
        StartCoroutine(ShowNumber());
    }

    IEnumerator ShowNumber()
    {
        while(currentNumber < targetNumber)
        {
            currentNumber += 1;
            numberText.text = currentNumber.ToString();
            yield return new WaitForSeconds(rate);
        }
    }
}
