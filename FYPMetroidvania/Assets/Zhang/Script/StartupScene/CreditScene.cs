using System.Collections;
using UnityEngine;

public class CreditScene : MonoBehaviour
{
    public RectTransform[] panel;
    public float stayTime = 1.5f;
    public float duration = 1.5f;
    public float time = 3f;

    void Start()
    {
        StartCoroutine(Credit());
    }

    void Update()
    {
        
    }

    IEnumerator Credit()
    {
        yield return new WaitForSeconds(1.5f);

        //foreach (RectTransform element in element)
        //{
        //    yield return Scroll(element);
        //}

        foreach (RectTransform element in panel)
        {
            StartCoroutine(Scroll(element));
            yield return new WaitForSeconds(time);
        }
    }

    IEnumerator Scroll(RectTransform element)
    {
        if (element == null) yield break;

        Vector2 startPos = new Vector2(0, -Screen.height / 2 - element.rect.height / 2);
        Vector2 middlePos = Vector2.zero;
        Vector2 topPos = new Vector2(0, Screen.height / 2 + element.rect.height / 2);

        element.anchoredPosition = startPos;

        float e = 0f;

        while (e < duration)
        {
            e += Time.deltaTime;
            float t = e / duration;
            element.anchoredPosition = Vector2.Lerp(startPos, middlePos, t);
            yield return null;
        }
        element.anchoredPosition = middlePos;

        yield return new WaitForSeconds(stayTime);

        e = 0f;
        while (e < duration)
        {
            e += Time.deltaTime;
            float t = e / duration;
            element.anchoredPosition = Vector2.Lerp(middlePos, topPos, t);
            yield return null;
        }
        element.anchoredPosition = topPos;
    }

}
