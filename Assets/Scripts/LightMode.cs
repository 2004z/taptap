using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayInterface;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
public  enum E_color
{
    White,
    Red,
    Yellow,
    Green,
    Cyan,
    Blue,
}
public class Light : MonoBehaviour, IRay
{
    public static int LightIntense =10;//光强
    int ChangeColor = 0;//是否变色
    public float MoveSpeed = 5f;//移动速度
    private Rigidbody2D rb;//刚体
    public LayerMask collisionLayer;//碰撞层
    public float rayLength;//射线长度
    public float lineDuration = 1f;//射线持续时间
    public int maxReflections = 5;//最大反射次数
    E_color color = E_color.White;//初始颜色

    public Vector2 SplitPoint =new Vector2(50, 50);//分界点
    public Vector2 RayhitPoint;//假设的最终点
    public static int decay = 0;//衰减因子


    private Vector2 startPoint;//射线起始点
    private Vector2 direction ;//射线预计初始方向
    private Vector2 startDirection;//初始方向
    private int i = 0;//用来判断初始方向
    private Vector2 lastInputDirection;//最后按下的方向
    private LineRenderer lineRenderer;//线条

    private bool isShooting = false;//是否正在射线

    public float offset =0.01f;//偏移量
    public static Light Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));       
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    void Update()
    {
        rayLength = LightIntense * 5 -decay*decay;
        if (!isShooting)
        {
            Move();
        }
        
        if (Input.GetKeyDown(KeyCode.J)&&LightIntense>0)
        {                        
            DecidedDecay();
            startDirection = direction;
            isShooting = true;
            rb.velocity = Vector2.zero;            
            ((IRay)this).Ray();
            LightIntense -= 1;
        }

    }
    #region 移动相关以及获取初始射线方向    
    void Move()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        // 判断方向，优先级为上下>左右
        if (horizontalInput > 0 && verticalInput == 0
            || horizontalInput > 0 && verticalInput > 0
            || horizontalInput > 0 && verticalInput < 0)
        {
            i = 1;
            direction = Vector2.right;
        }
        else if (horizontalInput < 0 && verticalInput == 0
            || horizontalInput < 0 && verticalInput > 0
            || horizontalInput < 0 && verticalInput < 0)
        {
            i = 2;
           direction = Vector2.left;
        }
        else if (verticalInput > 0 && horizontalInput == 0)
        {
            i = 3;
            direction = Vector2.up;
        }
        else if (verticalInput < 0 && horizontalInput == 0)
        {
            i = 4;
            direction = Vector2.down;
        }
        else
        {
            switch (i)
            {
                case 0:
                    direction = Vector2.right;
                    break;
                case 1:
                    direction = Vector2.right;
                    break;
                case 2:
                    direction = Vector2.left;
                    break;
                case 3:
                    direction = Vector2.up;
                    break;
                case 4:
                    direction = Vector2.down;
                    break;
            }            
        }
        rb.velocity = new Vector2(horizontalInput, verticalInput).normalized * MoveSpeed;
    }
    #endregion

    void IRay.Ray()
    {
        List<Vector2> rayPoints = new List<Vector2> { rb.position };
        List<Vector2> rayPoints2 = new List<Vector2> { rb.position };
        
        Vector2 currentDirection = startDirection;
        Vector2 currentDirection2;
        float remainingLength = rayLength;
        int reflections = 0;
        
        #region 射线具体判断逻辑
        while (remainingLength > 0 && reflections < maxReflections)
        {
            RaycastHit2D hit = Physics2D.Raycast(rayPoints[rayPoints.Count-1],currentDirection, remainingLength, collisionLayer);
            if (hit.collider == null)
            {
                rayPoints.Add(rayPoints[rayPoints.Count - 1] + currentDirection * remainingLength);
                remainingLength = 0;
            }
            else if (hit.collider != null )
            {
                Vector2 hitPoint = hit.point;
                Vector2 normal = hit.normal;
                Debug.Log($"碰撞点{hit.point}");
                Debug.Log($"碰撞点法向{hit.normal}");
                Debug.Log($"当前射出方向{currentDirection}");
                switch (hit.collider.tag)
                {
                    #region 反射相关逻辑
                    case "Reflection":
                        hit.point= DecideReflectionOffset(offset, currentDirection,ref hitPoint);
                        currentDirection = Vector2.Reflect(currentDirection, normal);
                        Debug.Log($"反射后射出方向{currentDirection}");
                        break;
                    #endregion
                    #region 折射相关
                    case "Refraction1"://60度
                        rayPoints.Add(hit.point);                       
                        currentDirection = Quaternion.Euler(0, 0,-60) * currentDirection;
                        DecideRefractionOffset(currentDirection, ref hitPoint);    
                        Debug.Log($"折射目前角度{currentDirection}");
                        break;

                    case "Refraction2"://120度
                        rayPoints.Add(hit.point);
                        currentDirection = Quaternion.Euler(0, 0, -120) * currentDirection;
                        DecideRefractionOffset(currentDirection, ref hitPoint);
                        break;
                    #endregion
                    #region 滤镜相关
                    case "Filter":
                        int change = rayPoints.Count - 1;
                        color = color + 1;
                        break;
                    #endregion
                    #region 障碍相关
                    case "Obstacle":
                        remainingLength = 0;
                        break;

                    case "SpecialObstacle1"://不可以通过，但是可以折射 60度
                        if (lastInputDirection == Vector2.right)
                        {
                            currentDirection = Quaternion.Euler(0, 0, 60)* currentDirection;
                        }
                        else if (lastInputDirection == Vector2.left)
                        {
                            currentDirection = Quaternion.Euler(0, 0, 60) * currentDirection;
                        }
                        else if (lastInputDirection == Vector2.up)
                        {
                            currentDirection = Quaternion.Euler(0, 0, 60) * currentDirection;
                        }
                        else if (lastInputDirection == Vector2.left)
                        {
                            currentDirection = Quaternion.Euler(0, 0, 60) * currentDirection;
                        }
                        Debug.Log($"当前射出方向{currentDirection}");
                        break;
                    //case "SpecialObstacle2":
                    //    currentDirection = (currentDirection + new Vector2(-1, 1.7f)).normalized;
                    //    break;
                    //case "Division":
                    //    if (currentDirection.x > 0)
                    //    {
                    //        currentDirection = new Vector2(1.7f, 1).normalized;
                    //        currentDirection2 = new Vector2(1.7f, -1).normalized;
                    //    }
                    //    else
                    //    {
                    //        currentDirection = new Vector2(-1.7f, 1).normalized;
                    //        currentDirection2 = new Vector2(-1.7f, -1).normalized;
                    //    }
                    //    rayPoints2 = rayPoints;

                    //    break;                    
                    #endregion
                    default:
                        break; //结束射线
                }
                rayPoints.Add(hitPoint);
                remainingLength -= hit.distance;
                reflections++;
            }
            #endregion           
        }
        UpdateLineRenderer(rayPoints);
        StartCoroutine(ClearLineAfterDelay(lineDuration));
    }
    #region 更新线条位置的函数
    private void UpdateLineRenderer(List<Vector2> rayPoints)
    {
        if(ChangeColor == 0)
        {
            
            lineRenderer.positionCount = rayPoints.Count;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, rayPoints[i]);
                Debug.Log($"碰撞点和结束点{rayPoints[i]}");
            }
        }
        if (ChangeColor != 0)
        {
            for (int i = 0; i < ChangeColor; i++)
            {
                lineRenderer.SetPosition(i, rayPoints[i]);
            }
            lineRenderer.material.color = ConvertColor(color);
            for (int j = ChangeColor; j < rayPoints.Count; j++)
            {
                lineRenderer.SetPosition(j, rayPoints[j]);               
            }
            lineRenderer.material.color = Color.white;
            ChangeColor = 0;
        }
        
        
    }
    #endregion
    #region 清除线条迭代器
    private IEnumerator ClearLineAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lineRenderer.positionCount = 0;
        isShooting = false;
    }
    #endregion
    #region 将枚举转换成颜色的函数
    Color ConvertColor(E_color color)
    {
        switch(color)
        {
            case E_color.White:
                return Color.white;
            case E_color.Red:
                return Color.red;
            case E_color.Yellow:
                return Color.yellow;
            case E_color.Green:
                return Color.green;
            case E_color.Cyan:
                return Color.cyan;
            case E_color.Blue:
                return Color.blue;            
        }
        return Color.white;
    }
    #endregion
    #region 判断衰减系数
    void DecidedDecay()
    {
        RayhitPoint = rb.position + lastInputDirection * (LightIntense * 5 - decay * decay);
        if (RayhitPoint.x > SplitPoint.x && RayhitPoint.y> SplitPoint.y)
        {
            decay = 1;
        }
        else if (RayhitPoint.x < SplitPoint.x && RayhitPoint.y > SplitPoint.y)
        {
            decay = 2;
        }
        else if (RayhitPoint.x < SplitPoint.x && RayhitPoint.y < SplitPoint.y)
        {
            decay = 3;
        }
        else
        {
            decay = 4;
        }
    }
    #endregion
    #region 检测反射碰撞点应该往哪偏移
    Vector2 DecideReflectionOffset(float offset, Vector2 currentdirection,ref Vector2 hitpoint)
    {
        if (currentdirection.x > 0 && currentdirection.y>0)
        {
            hitpoint.x -= offset;
            hitpoint.y -= offset;
        }
        else if (currentdirection.x>0 && currentdirection.y<0)
        {
            hitpoint.x -= offset;
            hitpoint.y += offset;
        }
        else if (currentdirection.x < 0 && currentdirection.y > 0)
        {
            hitpoint.x += offset;
            hitpoint.y -= offset;
        }
        else if (currentdirection.x < 0 && currentdirection.y < 0)
        {
            hitpoint.x += offset;
            hitpoint.y += offset;
        }
        else if (currentdirection.x == 0 && currentdirection.y < 0)
        {
            hitpoint.y += offset;           
        }
        else if (currentdirection.x == 0 && currentdirection.y > 0)
        {
            hitpoint.y -= offset;
        }
        else if (currentdirection.x < 0 && currentdirection.y == 0)
        {
            hitpoint.x += offset;
        }
        else if (currentdirection.x > 0 && currentdirection.y == 0)
        {
            hitpoint.x -= offset;
        }
        return hitpoint;
    }
    #endregion
    #region 检测折射碰撞点该往哪移动
    Vector2 DecideRefractionOffset( Vector2 currentdirection, ref Vector2 hitpoint)
    {
        hitpoint += currentdirection * offset*100;
        return hitpoint;
    }
    #endregion
}


