using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flash : MonoBehaviour
{
	public float cycleTime = 0.2f;
	public Vector2 size = new Vector2(2, 20);
	public Color oriColor = new Color(1, 1, 1, 1);
	public Color newColor = new Color(1, 1, 1, 0);

	// Use this for initialization
	void OnEnable()
	{
		StartCoroutine("Shine");
	}

	private void OnDisable()
	{
		StopCoroutine("Shine");
	}

	private void ShineAction(bool isColor)
	{
		gameObject.GetComponent<RectTransform>().sizeDelta = size;
		gameObject.GetComponent<Image>().color = isColor ? oriColor : newColor;
	}
	IEnumerator Shine()
	{
		while (true)
		{
			ShineAction(false);
			yield return new WaitForSeconds(cycleTime);
			ShineAction(true);
			yield return new WaitForSeconds(cycleTime);
		}
	}
}
