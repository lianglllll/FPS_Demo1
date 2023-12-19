using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���������ת���������ң�
/// �����ӽǵ������
/// </summary>
/// 1.������ת�������ת�����ˣ���ΪĿǰ�����Ǹ������壬��������תʵ�������
/// 2.������ת������ת��Ϊ��ʱ����Ҫת��ǹ��


public class PlayerCamera : MonoBehaviour
{
    //��Ұ��ת������
    public float mouseSensitivity = 100f;
    //��ҵ�����
    public Transform playerBody;
    //��������ֵ���ۼ�
    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //����������ڸ���Ϸ���ڵ����ġ��⻹������Ӳ����ꡣ(�������)
        Cursor.lockState = CursorLockMode.Locked;
    }

 
    void Update()
    {
        //��ȡ��������,��/������ֵ  ��/���Ǹ�ֵ   ����ֵ����-1~1֮��
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //��������ת����ֵ�����ۼƣ��Ӹ�������ΪҪ����Ҫ��Ч���෴��
        xRotation -= mouseY;
        //������ֵ���ۻ�������80�ȣ�
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        //�ı����������ת�����������ת�������ת��ͬ��ֱ�ӣ��������ת�����ݱ������������������
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //��ҵĺ�����ת,����ֱ��ת�൱����ԭ�����������ۼ��ˣ�mouseX���Զ�ָ������
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
