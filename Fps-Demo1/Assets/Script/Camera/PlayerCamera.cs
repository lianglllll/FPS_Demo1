using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 摄像机的旋转（上下左右）
/// 主角视角的摄像机
/// </summary>
/// 1.上下旋转让摄像机转就行了，因为目前主角是个胶囊体，身体上下转实在是奇怪
/// 2.左右旋转让主角转因为到时候需要转动枪口


public class PlayerCamera : MonoBehaviour
{
    //视野旋转灵敏度
    public float mouseSensitivity = 100f;
    //玩家的身体
    public Transform playerBody;
    //存放鼠标轴值的累计
    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //将光标锁定在该游戏窗口的中心。这还会隐藏硬件光标。(鼠标隐藏)
        Cursor.lockState = CursorLockMode.Locked;
    }

 
    void Update()
    {
        //获取鼠标的轴体,右/上是正值  左/下是负值   且数值不在-1~1之间
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //将上下旋转的轴值进行累计，加负号是因为要和想要的效果相反了
        xRotation -= mouseY;
        //限制轴值的累积（正负80度）
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        //改变摄像机的旋转，摄像机的旋转和玩家旋转不同的直接，摄像机旋转的数据保存放在这个类里面而已
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //玩家的横向旋转,这里直接转相当于在原来数据上面累加了，mouseX会自动指定方向
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
