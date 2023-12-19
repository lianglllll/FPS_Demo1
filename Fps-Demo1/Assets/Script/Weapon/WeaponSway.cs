using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当角色行走的时候，武器应该有小幅度的摆动。
/// 就是说当鼠标向左移动时，手臂那也应该向左偏一点
/// </summary>
public class WeaponSway : MonoBehaviour
{
    public float swayRange = 0.03f;//摇摆幅度
    public float maxSwayRange = 0.06f;//最大的摇摆幅度
    public float smoothSway = 6f;//摇摆平滑值 ,因为从一个位置变化到另一个位置是很突兀的
    [SerializeField] private Vector3 originPosition;//手臂初始位置



    // Start is called before the first frame update
    void Start()
    {
        //获取手臂一开始的位置
        //position是根据世界原点为中心 2. localPosition是根据父节点为中心,如果没有父节点,localpositon和position是没有区别的
        originPosition = transform.localPosition;
        
    }

    // Update is called once per frame
    void Update()
    {
        //获取鼠标的轴值,因为手臂只需要小幅度的移动，所以可以成一个摇摆的幅度
        //取反的目的就是好看一点，鼠标右移手臂左移一点点
        float movementX = -Input.GetAxis("Mouse X") * swayRange;
        float movementY = -Input.GetAxis("Mouse Y") * swayRange;

        //限制不能超过最大的摇摆
        Mathf.Clamp(movementX, -maxSwayRange, maxSwayRange);
        Mathf.Clamp(movementY, -maxSwayRange, maxSwayRange);



        //设置手臂变化的三维向量
        Vector3 finnallyPosition = new Vector3(movementX, movementY, 0);
        //设置当前手臂的位置
        //Vector3.Lerp   插值，等于 a + (b - a) * t。每次返回的值都是从a到b的某段距离
        //使用插值 t 在点 a 和 b 之间进行插值。参数 t 限制在范围 [0, 1] 内。
        //这最常用于查找占两个终端之间距离特定百分比的点（例如，以便在这些点之间逐步移动对象）
        //参数一：当前手臂的位置  参数二：计数出来的位置和最初游戏开始的原始位置相加（所以鼠标不动的时候能回到原处）
        //参数三：在0-1之间  如：t = 0.1 也就是返回其a到b距离中的10分之1  (让位置的转移更加的平滑一点，简单的说就是每一帧经过的时间，改变一点，)
        transform.localPosition = Vector3.Lerp(transform.localPosition, finnallyPosition +originPosition, Time.deltaTime * smoothSway);


    }
}
    