using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayInterface;

public class ReflectionMode : MonoBehaviour,IRay
{
    private Rigidbody2D rb;
    public void Ray()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
