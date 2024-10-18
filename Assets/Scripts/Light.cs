using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayInterface;

public class Light : MonoBehaviour, IRay
{
    public float MoveSeppd = 5f; // 移动速度
    private Rigidbody2D rb; // 获取刚体
    public LayerMask collisionLayer; // 射线碰撞的图层
    public float rayLength = 10f; // 限制射线长度
    public float lineDuration = 1f; // 线条持续时间
    public int maxReflections = 5; // 最大反射次数

    private Vector2 startPoint; // 射线起始点
    private Vector2 endPoint; // 射线终点
    private RaycastHit2D hit; // 射线碰撞信息
    private Vector2 direction = Vector2.right; // 射线初始方向
    private Vector2 lastInputDirection; // 最后一次按下的方向
    private LineRenderer lineRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();

        // 设置 LineRenderer 的材质和宽度
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // 确认 LineRenderer 已正确初始化
        Debug.Log("LineRenderer initialized with startWidth: " + lineRenderer.startWidth + " and endWidth: " + lineRenderer.endWidth);
    }

    void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.J))
        {
            // 调用 Ray() 方法
            ((IRay)this).Ray();
        }
    }

    void Move()
    {
        // 获取输入
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // 更新最后一次按下的方向
        if (horizontalInput > 0)
        {
            lastInputDirection = Vector2.right; // 向右
        }
        else if (horizontalInput < 0)
        {
            lastInputDirection = Vector2.left; // 向左
        }
        else if (verticalInput > 0)
        {
            lastInputDirection = Vector2.up; // 向上
        }
        else if (verticalInput < 0)
        {
            lastInputDirection = Vector2.down; // 向下
        }

        // 计算移动方向
        Vector2 targetVelocity = new Vector2(horizontalInput, verticalInput).normalized * MoveSeppd;

        // 移动
        rb.velocity = targetVelocity;
    }

    void IRay.Ray()
    {
        direction = lastInputDirection;
        List<Vector3> rayPoints = new List<Vector3>();
        startPoint = rb.position;
        rayPoints.Add(startPoint);
        Vector2 currentDirection = direction;
        float remainingLength = rayLength;
        int reflections = 0;

        while (remainingLength > 0 && reflections < maxReflections)
        {
            hit = Physics2D.Raycast(startPoint, currentDirection, remainingLength, collisionLayer);
            if (hit.collider != null && hit.collider.tag == "Reflection")
            {
                Vector2 hitPoint = hit.point;
                Vector2 normal = hit.normal;
                Vector2 reflection = Vector2.Reflect(currentDirection, normal);
                startPoint = hitPoint;
                currentDirection = reflection;
                rayPoints.Add(hitPoint);
                remainingLength -= hit.distance;
                reflections++;
            }
            else
            {
                endPoint = startPoint + currentDirection * remainingLength;
                rayPoints.Add(endPoint);
                remainingLength = 0;
            }
        }

        // Ensure the length is set properly
        if (remainingLength <= 0)
        {
            rayPoints.Add(startPoint + currentDirection * rayLength);
        }

        lineRenderer.positionCount = rayPoints.Count;
        for (int i = 0; i < rayPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, rayPoints[i]);
        }
        StartCoroutine(ClearLineAfterDelay(lineDuration));
    }


    private IEnumerator ClearLineAfterDelay(float delay)
    {
        // 等待指定的时间
        yield return new WaitForSeconds(delay);

        // 清除 LineRenderer 的点数
        lineRenderer.positionCount = 0;
    }
}

