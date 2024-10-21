using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class newLight : MonoBehaviour
{
    public Vector2 startPosition;
    public Vector2 endPosition;
    public bool followMouse = false;

    [SerializeField] public Color color = new Color(191 / 255, 36 / 255, 0);
    [SerializeField] private float colorIntensity = 4.3f;
    private float beamColorEnhance = 1;

    [SerializeField] public float thickness = 9;
    [SerializeField] private float noiseScale = 3.14f;

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject startVtf;
    [SerializeField] private GameObject endVtf;
    
    private const float Max_Length = 100;
    private const float Offset = 1f;

    [SerializeField] private string[] layerMasks;
    private LayerMask layerMask;
    private void Awake()
    {
        lineRenderer.material.color = color * colorIntensity;
        lineRenderer.material.SetFloat("_LaserThickness", thickness);
        lineRenderer.material.SetFloat("_LaserScale", noiseScale);

        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
