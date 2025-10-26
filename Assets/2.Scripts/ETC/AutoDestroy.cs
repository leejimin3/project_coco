using System;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay;

    private void Start()
    {
        Invoke("des", delay);
    }

    public void des()
    {
        Destroy(this.gameObject);
    }
}
