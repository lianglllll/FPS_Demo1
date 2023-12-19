using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1.���ƽ�ɫ�ƶ�
/// 2.���������֮����ӱ��ܹ���(��Ҫ��ȡ��λ����)
/// 3.ʵ����Ծ����
/// 4.���������б�����жٴ��
/// 5.[SerializeField]�������������unity���濴����Щ˽�е����Բ����޸�
/// </summary>

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController; //��ɫ�������������������Ͳ�����ײ����

    [SerializeField]
    private float walkSpeed = 10f; //���ߵ��ٶ�
    [SerializeField]
    private float runSpeed = 15f;  //���ܵ��ٶ�
    public float speed; //����һ���ٶ�����������װ��ͬ���ٶ�,Ӧ�Բ�ͬ���ƶ���ʽ
    private Vector3 moveDirction; //һ����ά����,�����ƶ�������

    [SerializeField]
    private float jumpForce = 3f;//������Ծʩ�ӵ����ȣ�ԭ���Ǹ�����һ�����ϵ�����
    [SerializeField]
    public float gravity = -23f;//����
    [SerializeField]
    private Vector3 jumpVector3 = new Vector3(0f,0f,0f);//���������Ծ������ֵ����ֱ�������y���ֵ���ɣ�
    private Transform groundCheckPoint;//�������
    private float groundDistance = 0.1f;//�����������ľ���
    [SerializeField]
    private LayerMask groundMask;//���������ڵĲ��


    public bool isRun;//�жϽ�ɫ�Ƿ��ڱ���
    private bool isJump;//�ж��Ƿ�������Ծ��
    public bool isWalk;//�ж��Ƿ�������
    private bool isGround;//�ж��Ƿ��ڵ���

    public float slopeForce = 6.0f;//��б��ʱʩ�ӵ�����
    public float slopeForceRayLength = 2.0f;//б�µ����߳��ȣ��Զ�������


    [Header("��������")]
    [SerializeField] private AudioSource audioSource;//������Դ
    public AudioClip walkingSource;//��ɫ���ߵ�����Ƭ��
    public AudioClip runingSource;//��ɫ���ܵ�����Ƭ��

    /*--------------------------------����һЩ�����¼��İ�����˳����һЩ���õı�ǩ----------------------------------------------------------*/

    [Header("��������")]//����������ϸ�һ����ı���

    //1���ܰ���
    [SerializeField]//���л���������private��������ʾ�������
    [Tooltip("���ܰ���")]//�����������������ͣ��ʱ��ͻ���ʾ���� 
     private KeyCode runIputName;

    //2.��Ծ��λ
    [SerializeField] [Tooltip("��Ծ����")] private string jumpIputName;







    void Start()
    {
        //1.��ʼ�ȸ���ɫ��������ֵ
        characterController = GetComponent<CharacterController>();
        //2.���ñ��ܰ���
        runIputName = KeyCode.LeftShift;
        //3.������Ծ����
        jumpIputName = "Jump";
        //4.��ȡ�������
        groundCheckPoint = GameObject.Find("Player/groundCheckPoint").GetComponent<Transform>();
    }

    
    void Update()
    {
        CheckGround();
        Move();
    }

    
    /*
     --------------------------------------------------------1.��ɫ�ƶ�-------------------------------------------------------
     */
    public void Move()
    {
        //1.��ȡˮƽ��, ��ȡ��ֱ��
        float h = Input.GetAxis("Horizontal");//ˮƽ
        float v = Input.GetAxis("Vertical");//��ֱ

        //2.�жϽ�ɫ�Ƿ��ڱ���/���ߣ������Ƿ��ֱܷ���ٶȸ�ֵ
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

        //3.���÷������������Ҹ���ɫ��������move������ֵ
        //��ȡ���˽�ɫ�ƶ��ķ�������   transform.right��ˮƽ�ķ���x   transform.forward�Ǵ�ֱ�ĵķ���z
        //normalized�������ǹ�һ�������������ķ��򲻱䣬�����ȱ�Ϊ1.0    ������normalized��һ���ᵼ���ƶ��Ử��
        moveDirction = (transform.right * h + transform.forward * v);

        //ʵ���ƶ�����ʹ��CharacterController�Դ���move��������Ҫ����һ����ά�ķ�������
        //��λ�������� * �ٶȴ�С*�����һ֡���õ�ʱ��           ��ʾÿһ֡�ƶ��ľ���=�ٶ�*ÿһ֡��ʱ��
        characterController.Move(moveDirction * speed * Time.deltaTime);

        //4.�����ڵ����ʱ��������Ҫ�ۼ����µ�����
        if(isGround == false)
        {

            jumpVector3.y += gravity * Time.deltaTime;//��jumpVector3ÿ��-20ֱ�������ڵ��棩
        }
        //5.��ɫ�����������ƶ��������������ϵ�������0��120������0��ʵ�����ϵ��ƶ���
        //��������ڵ����ʱ��jumpVector3�ǣ�0��-2��0����ʱ�����ƶ��������ƶ���Ȼ�����������ڵ�������ײ,����ɶ��û�ã�
        characterController.Move(jumpVector3 * Time.deltaTime);//����Ӱ���´�ֱ�����µ��ƶ�

        //6.��jumpVector3��ֵ
        Jump();

        //7.�����б�����ƶ��Ļ�������Ҫ�����������һ�����µ����ȣ���������б���ϱı�����
        if (onSlope())
        {
            characterController.Move(Vector3.down * characterController.height / 2 * slopeForce *Time.deltaTime);
        }

        //8.�������Ӧ�����߱�����Ч
        PlayFootAudioSource();

    }

    //-------------------------------------------------2.��Ծ---------------------------------------------
    public void Jump()
    {
        //�ж��û��Ƿ���������Ծ��
        isJump = Input.GetButtonDown(jumpIputName);
        //��������Ծ��ͬʱ�ڵ���Ļ�����������ʩ����Ծ��(��ʵ���Ǹ�������ϵ�������ֵ��С)
        if (isJump && isGround)
        {        
             jumpVector3.y = Mathf.Sqrt(jumpForce * -2f * gravity);//3��-2��-20 =120 ��yֵΪ120��������0��120������0����
        }
      
    }

    //--------------------------------------------3.������-----------------------------------------------------
    public void CheckGround()
    {
        //�ڸ����ĵ����Ӵ���������һ�����Զ�������壬����������Ƿ���ײ�����������ˣ������ײ���˾ͷ���true
        isGround= Physics.CheckSphere(groundCheckPoint.position, groundDistance, groundMask);
        //Debug.Log(isGround);
        //�������ڵ���͸���ɫʩ�Ӵ�ֱ���µ���������ʵ���Ǹ�������ֵ��
        if (isGround && jumpVector3.y<=0)
        {
            jumpVector3.y = -2f;
        }
    } 

    //------------------------------------------4.�ж��Ƿ���б��----------------------------------------------
    public bool onSlope()
    {
        //��Ծ״̬�����ܻ���Ϊ��б����
        if (isJump)
        {
            return false;
        }
        //�ж��Ƿ���б���ϣ�ԭ�����������һ�����ߣ����ɵ�һ�����ߣ�����������߲������ϵĻ���˵����ʱ������б����
        RaycastHit hit;//��¼���ߵ���Ϣ
        /*
         Physics.Raycast�Ĳ��� 1.���ǵ�λ�ã�Ҫ�������ߵ����壩 2.��ʲô���򷢳�����  3.������ߵ����ݣ����ߵķ���  4.���ߵĳ���
         */
        //����������κ���ײ���ཻ������ true�����򷵻� false��
        if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength))
        {
            //������صķ��߷��򲻵��ڣ�0��1��0����˵����б����
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }


    /*-------------------------------------------------------5.�������ߺͱ��ܵ���Ч------------------------------------------------*/
    public void PlayFootAudioSource()
    {
        //1.�����ж����Ƿ��ڵ�����(������Ծʱ)�����Ҳ��Ǿ�ֹ��״̬�����˾�ֹʱ��
        //moveDirction.sqrMagnitude���ظ�������ƽ������,������0.9f���ֵ�Ͼ������������������ˣ��Ͼ���λ������1��
        if (isGround && moveDirction.sqrMagnitude > 0.9f)//���ߺͱ��ܵ�״̬��
        {//a.�������ߺͱ��ܵ���Ч
            audioSource.clip = isRun ? runingSource : walkingSource;
            //b.������ڲ��žͲ����������ÿ�žͲ�����Ч
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else//�ھ�ֹ��״̬��
        {
            //�����Ч���ڲ��ž͹ر���
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }

        }
    }


}
