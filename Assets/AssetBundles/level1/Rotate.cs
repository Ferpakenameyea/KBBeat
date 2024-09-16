using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Rotate : MonoBehaviour
{
    [SerializeField] private Vector3 axis;
    [SerializeField] private float speed;


    private void Update() 
    {
        this.transform.Rotate(axis, speed * Time.deltaTime);    
    }
}
