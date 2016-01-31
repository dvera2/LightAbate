using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour {

    public float Timer = 5.0f;

    void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0)
            Destroy(gameObject);
    }
}
