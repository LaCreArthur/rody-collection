using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardComponent : MonoBehaviour
{
    //public Transform target;
    private Transform _cam;
    //public Vector3 posOffset;

    private void Start()
    {
        _cam = Camera.main.transform;
    }

    void Update()
    {
        //transform.position = cam.WorldToScreenPoint(target.position + posOffset);
        transform.LookAt(transform.position + _cam.forward);
    }
}