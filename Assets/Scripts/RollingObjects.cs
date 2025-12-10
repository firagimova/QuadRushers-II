using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingObjects : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * 75f);
    }

}
