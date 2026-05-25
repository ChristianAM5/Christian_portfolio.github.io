using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningLightController : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine("DestroyWarningLight");
    }

    public IEnumerator DestroyWarningLight()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
