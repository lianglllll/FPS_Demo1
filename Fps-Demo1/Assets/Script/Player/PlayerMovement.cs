using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1.控制角色移动
/// 2.在上面基础之上添加奔跑功能(需要获取键位输入)
/// 3.实现跳跃功能
/// 4.解决人物在斜坡上有顿挫感
/// 5.[SerializeField]加入这个可以在unity界面看到这些私有的属性并且修改
/// </summary>

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController; //角色控制器，用了这个组件就不用碰撞器了

    [SerializeField]
    private float walkSpeed = 10f; //行走的速度
    [SerializeField]
    private float runSpeed = 15f;  //奔跑的速度
    public float speed; //单纯一个速度容器，用于装不同的速度,应对不同的移动方式
    private Vector3 moveDirction; //一个三维向量,保存移动的向量

    [SerializeField]
    private float jumpForce = 3f;//向上跳跃施加的力度，原理是给人物一个向上的力度
    [SerializeField]
    public float gravity = -23f;//重力
    [SerializeField]
    private Vector3 jumpVector3 = new Vector3(0f,0f,0f);//设置玩家跳跃的向量值（垂直向上添加y轴的值即可）
    private Transform groundCheckPoint;//地面检测点
    private float groundDistance = 0.1f;//检测点检测与地面的距离
    [SerializeField]
    private LayerMask groundMask;//地面所属于的层次


    public bool isRun;//判断角色是否在奔跑
    private bool isJump;//判断是否按下了跳跃键
    public bool isWalk;//判断是否在行走
    private bool isGround;//判断是否在地面

    public float slopeForce = 6.0f;//走斜坡时施加的力度
    public float slopeForceRayLength = 2.0f;//斜坡的射线长度（自定义量）


    [Header("声音设置")]
    [SerializeField] private AudioSource audioSource;//声音资源
    public AudioClip walkingSource;//角色行走的声音片段
    public AudioClip runingSource;//角色奔跑的声音片段

    /*--------------------------------设置一些特殊事件的按键，顺便用一些好用的标签----------------------------------------------------------*/

    [Header("按键设置")]//可以在面板上搞一个大的标题

    //1奔跑按键
    [SerializeField]//序列化，可以让private的属性显示到面板上
    [Tooltip("奔跑按键")]//这个当鼠标在面板上悬停的时候就会显示出来 
     private KeyCode runIputName;

    //2.跳跃键位
    [SerializeField] [Tooltip("跳跃按键")] private string jumpIputName;







    void Start()
    {
        //1.开始先给角色控制器赋值
        characterController = GetComponent<CharacterController>();
        //2.设置奔跑按键
        runIputName = KeyCode.LeftShift;
        //3.设置跳跃按键
        jumpIputName = "Jump";
        //4.获取地面检测点
        groundCheckPoint = GameObject.Find("Player/groundCheckPoint").GetComponent<Transform>();
    }

    
    void Update()
    {
        CheckGround();
        Move();
    }

    
    /*
     --------------------------------------------------------1.角色移动-------------------------------------------------------
     */
    public void Move()
    {
        //1.获取水平轴, 获取垂直轴
        float h = Input.GetAxis("Horizontal");//水平
        float v = Input.GetAxis("Vertical");//垂直

        //2.判断角色是否在奔跑/行走，根据是否奔跑分别给速度赋值
        isRun = Input.GetKey(runIputName);
        isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;
        if (isRun)
        {
            speed = runSpeed;
        }
        else
        {
            speed = walkSpeed;
        }

        //3.设置方向向量，并且给角色控制器的move函数赋值
        //获取到了角色移动的方向向量   transform.right是水平的方向x   transform.forward是垂直的的方向z
        //normalized的作用是归一化，保持向量的方向不变，但长度变为1.0    在这里normalized归一化会导致移动会滑行
        moveDirction = (transform.right * h + transform.forward * v);

        //实现移动这里使用CharacterController自带的move函数，需要传入一个三维的方向向量
        //单位方向向量 * 速度大小*完成上一帧所用的时间           表示每一帧移动的距离=速度*每一帧的时间
        characterController.Move(moveDirction * speed * Time.deltaTime);

        //4.当不在地面的时候，我们需要累加向下的重力
        if(isGround == false)
        {

            jumpVector3.y += gravity * Time.deltaTime;//（jumpVector3每次-20直到物体在地面）
        }
        //5.角色控制器进行移动操作（根据向上的向量（0，120开方，0）实现向上的移动）
        //（如果是在地面的时候jumpVector3是（0，-2，0）此时进行移动是向下移动，然后两个物体在地面上碰撞,所以啥事没用）
        characterController.Move(jumpVector3 * Time.deltaTime);//重力影响下垂直方向下的移动

        //6.给jumpVector3赋值
        Jump();

        //7.如果在斜坡上移动的话，就需要给人物再添加一个向下的力度，放在它再斜坡上蹦蹦跳跳
        if (onSlope())
        {
            characterController.Move(Vector3.down * characterController.height / 2 * slopeForce *Time.deltaTime);
        }

        //8.播放相对应的行走奔跑音效
        PlayFootAudioSource();

    }

    //-------------------------------------------------2.跳跃---------------------------------------------
    public void Jump()
    {
        //判断用户是否输入了跳跃键
        isJump = Input.GetButtonDown(jumpIputName);
        //当按下跳跃键同时在地面的话，给给主角施加跳跃力(其实就是给这个向上的向量赋值大小)
        if (isJump && isGround)
        {        
             jumpVector3.y = Mathf.Sqrt(jumpForce * -2f * gravity);//3×-2×-20 =120 （y值为120的向量（0，120开方，0））
        }
      
    }

    //--------------------------------------------3.地面检测-----------------------------------------------------
    public void CheckGround()
    {
        //在给定的点名接触点中生成一个你自定义的球体，检测球体内是否碰撞到其他物体了，如果碰撞到了就返回true
        isGround= Physics.CheckSphere(groundCheckPoint.position, groundDistance, groundMask);
        //Debug.Log(isGround);
        //如果检测在地面就给角色施加垂直向下的重力（其实就是给向量赋值）
        if (isGround && jumpVector3.y<=0)
        {
            jumpVector3.y = -2f;
        }
    } 

    //------------------------------------------4.判断是否在斜坡----------------------------------------------
    public bool onSlope()
    {
        //跳跃状态，不能划分为在斜坡上
        if (isJump)
        {
            return false;
        }
        //判断是否在斜坡上，原理是向地面打出一条射线，生成的一条法线，如果这条法线不是向上的话就说明此时主角在斜坡上
        RaycastHit hit;//记录射线的信息
        /*
         Physics.Raycast的参数 1.主角的位置（要发出射线的物体） 2.向什么方向发出射线  3.存放射线的数据（法线的方向）  4.射线的长度
         */
        //如果射线与任何碰撞体相交，返回 true，否则返回 false。
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength))
        {
            //如果返回的法线方向不等于（0，1，0）就说明在斜坡上
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }


    /*-------------------------------------------------------5.播放行走和奔跑的音效------------------------------------------------*/
    public void PlayFootAudioSource()
    {
        //1.首先判断它是否在地面上(过滤跳跃时)，并且不是静止的状态（过滤静止时）
        //moveDirction.sqrMagnitude返回该向量的平方长度,让他和0.9f这个值毕竟大于它就是走起来了（毕竟单位长度是1）
        if (isGround && moveDirction.sqrMagnitude > 0.9f)//行走和奔跑的状态下
        {//a.设置行走和奔跑的音效
            audioSource.clip = isRun ? runingSource : walkingSource;
            //b.如果正在播放就不管他，如果每放就播放音效
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else//在静止的状态下
        {
            //如果音效还在播放就关闭它
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }

        }
    }


}
