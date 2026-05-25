using System.Collections;
using UnityEngine;

public class BounceEffect : MonoBehaviour
{
    public float bounceHeight = 0.3f;
    public float bounceDuration = 0.4f;
    public int bounceCount = 2;

    public void StartBounce()
    {
        // Llamamos a una corrutina
        StartCoroutine(BounceHandler());
    }    
    private IEnumerator BounceHandler()
    {
        Vector3 startPosition = transform.position;
        float localHeight = bounceHeight;
        float localDuration = bounceDuration;

        for (int i = 0; i < bounceCount; i++)
        {
            // Llamamos a otro corrutina para hacer el efecto rebote
            yield return Bounce(startPosition, localHeight, localDuration/2);
            localHeight *= 0.5f;
            localDuration *= 0.8f;
        }
        transform.position = startPosition;
    }
    private IEnumerator Bounce( Vector3 start, float height, float duration)
    {
        Vector3 peak = start + Vector3.up * height;
        float elapsed = 0;
        // Nos movemos hacia arriva
        while(elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, peak, elapsed / duration);
            elapsed+=Time.deltaTime;
            yield return null;
        }
        elapsed = 0;
        // Nos movemos hacia abajo
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp( peak, start, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }    
}
