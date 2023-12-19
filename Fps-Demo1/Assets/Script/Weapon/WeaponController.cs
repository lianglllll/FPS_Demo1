using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ����������
/// 1.����߼�
/// 2.����ٶȿ���
/// 3.��׼����ӵ�����������Ѫ����ui����
/// 4.�������Եƹ⣬�ӵ����е��������ԣ�����
/// 5.������Ӧ�Ķ������������������׼������������ߺͱ��ܶ����Ĳ���Ҳ�ŵ��������ˣ���Ϊ����״̬���������
/// </summary>
public class WeaponController : MonoBehaviour
{

    private Animator animator;//����״̬��
    public PlayerMovement playerMovement;//��ɫ�ƶ������ڸ�������Ӷ���
    private Camera playerCamera;//���ǵ����

    public Transform shootPoint;//�����
    public int aClipBullets = 30;//һ���������ӵ����� 
    public int range = 100;//�������
    public float fireRate = 0.5f;//��������(����������һ�Σ��൱�����cd)
    public float fireTimer = 0f;//��ʱ��
    public int bulletsLeft = 300;//����
    public int nowClipBullects;//��ǰ��������
  

    public ParticleSystem muzzleFlash;//ǹ�ڻ�������
    public Light muzzleFlashLight;//ǹ�ڻ���ƹ�


    public GameObject hitParticle;//�ӵ����е���������
    public GameObject bullectHole;//����


    private AudioSource audioSource;//������Դ
    public AudioClip AK47SoundClip;//ak�������ЧƬ��
    public AudioClip reloadClip1;//���ӵ�����Ч1
    public AudioClip reloadClip2;//���ӵ�����Ч2

    public bool gunShootInput;//����Ƿ񴥷������
    public bool isReload;//�Ƿ��ڻ���
    public bool isAim;//�Ƿ�����׼

    public Transform casingSpawnPoint;//�ӵ����׳���λ��
    public Transform casingPrefab;//�ӵ��ǵ�Ԥ����

    /*ö����������ȫ�Զ��Ͱ��Զ�*/
    //1:ȫ�Զ�  2�����Զ�
    public enum ShootModel{AutoRife,SemiGun};
    public ShootModel shootingMode;//�������������Դ洢ö�������е�һ��ֵ

    private int shootType = 1;//ģʽ�л���һ���м���� ��1.ȫ�Զ� 2.���Զ�  Ĭ��Ϊ1ȫ�Զ�
    private string shootUIName;//���ڲ�ͬģʽ��ui��ʾ

    [Header("��������")]
    [SerializeField][Tooltip("����ӵ�����")]private KeyCode reloadClipInputName;
    [SerializeField] [Tooltip("������������")] private KeyCode inspectClipInputName;
    [SerializeField] [Tooltip("ȫ�Զ�/���Զ��л�")] private KeyCode gunShootModelInputName;
   /* [SerializeField] [Tooltip("С������")] private KeyCode knife_attack_1InputName;*/


    [Header("UI����")]
    public Image aimPointUI;//׼��ui
    public Text bulletsNumbUI;//�ӵ�����ui
    public Text shootModelTextUI;//ȫ�Զ����Զ�ui


    // Start is called before the first frame update
    void Start()
    {
        //1.��ʼ����ǰ��������
        nowClipBullects = aClipBullets;
        //2.��ʼ���ӵ�������ui
        UpdateALLUI();
        //3.��������ӵ��İ���
        reloadClipInputName = KeyCode.R;
        //4.��ȡ����Դ
        audioSource = GetComponent<AudioSource>();
        //5.��ȡ����״̬��
        animator = GetComponent<Animator>();
        //6.���ü�����������
        inspectClipInputName = KeyCode.H;
        //7.��ȡ�����
        playerCamera = Camera.main;
        //8.akĬ��ȫ�Զ�ģʽ
        shootingMode = ShootModel.AutoRife;
        shootUIName = "ȫ�Զ�";
        //9.�л�ȫ�Զ��Ͱ��Զ��İ�ť
        gunShootModelInputName = KeyCode.X;
       /* //10.����С����������
        knife_attack_1InputName = KeyCode.F;
       */
    }


    void Update()
    {

        //0.ģʽ���л�
        //���������x�����м������Ϊ1ʱ
        if (Input.GetKeyDown(gunShootModelInputName)&& shootType != 1)
        {
            shootType = 1;
            shootUIName = "ȫ�Զ�";
            shootingMode = ShootModel.AutoRife;//��ö����������Ϊȫ�Զ�
            //shootModelTextUI.text = shootUIName;
        }else if(Input.GetKeyDown(gunShootModelInputName) && shootType != 0)
        {
            shootType = 0 ;
            shootUIName = "���Զ�";
            shootingMode = ShootModel.SemiGun;//��ö����������Ϊ���Զ�
          //  shootModelTextUI.text = shootUIName;
        }

        //����ö����������ѡ���Ӧ�����뷽ʽ���������Ŀ������ϰ�����о��ǿ��ܺ��滹���������Ĺ���
        switch (shootingMode)
        {
            case ShootModel.AutoRife:
                //ȫ�Զ�����ȡ����ʱ������
                gunShootInput = Input.GetMouseButton(0);
                fireRate = 0.1f;//ͬʱ������������
                break;
            case ShootModel.SemiGun:
                //���Զ�,ֻ��ȡ���µ���һ��
                gunShootInput = Input.GetMouseButtonDown(0);
                fireRate = 0.2f;
                break;
        }

        //1.��������������ʱ�򣬴�������¼� �������0 �м���2  �Ҽ� ��1��
        //gunShootInput = Input.GetMouseButton(0);
        if (gunShootInput)//���°����Ϳ���
        {
            GunFire();
        }else//û���𣬵�����Ҫ�ѿ���ĵƹ�����
        {
            muzzleFlashLight.enabled = false;
        }

        //2.��������������ٶȣ�������ȴ״̬�ۼ�ʱ��------------------------------------------
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        /*3.�ж��Ƿ��ڻ���*/
        //��ȡ��������0�������
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        //���ֻ��ӵ��Ķ���
        if (info.IsName("reload_ammo_left") || info.IsName("reload_out_of_ammo"))
        {
            isReload = true;
        }
        else
        {
            isReload = false;
            //3.�����ӵ���ʾ��ui
            UpdateALLUI();
        }


        //4.������r���͵����ӵ���������������Ϊ0ʱ��������������
        if(Input.GetKeyDown(reloadClipInputName) && nowClipBullects< aClipBullets && bulletsLeft>0)
        {
            reloadClip();
        }

        //5.������h��ʱ�����ż�������Ķ���
        if (Input.GetKeyDown(inspectClipInputName))
        {
            animator.SetTrigger("inspect");     
        }
        //6.���ñ��ܺ����ߵĶ���
        animator.SetBool("Run", playerMovement.isRun);
        animator.SetBool("Walk", playerMovement.isWalk);

        //7.�������ӵ�Ϊ�յ�ʱ���ӵ�������
        if(nowClipBullects == 0 && bulletsLeft > 0)
        {
            reloadClip();
        }
        //8.��������Ҽ�ʱ������׼
        DoAim();

       /* //9.����f����С������
        if (Input.GetKeyDown(knife_attack_1InputName))
        {
            animator.SetTrigger("knifeAttack");
        }
        */

    }

    //------------------------------------------1.�������-----------------------------------------------------
    public void GunFire()
    {
        //1.���ж��ܲ������
        //a.��Ŀ�ľ���Ϊ�˼�������ִ�е�����/*��ʼ֮ǰ���жϼ�ʱ���ǲ��ǵ��������ʱ���Ҫ���������ʱ���֮�䷵��*/
        //b.�������ӵ�����Ϊ�վͲ��������
        //c.���ӵ�����ʱ�������
        //d.����ʱ�������
        if (fireTimer < fireRate || nowClipBullects <=0 || isReload || playerMovement.isRun)
        {
            return;
        }

        //2.��������ķ���(��ǰ�ķ���)
        Vector3 shootDiretion= shootPoint.forward;
        //3.ʹ�����߼��,Ȼ��ģ������֮��Ĳ���
        RaycastHit hit;//���ڽ������߷��ص���Ϣ
        if (Physics.Raycast(shootPoint.position, shootDiretion, out hit ,range))
        {
            Debug.Log("����");
            //a.ʵ����һ�����е���Ч����
            // Quaternion.FromToRotation��һ������ת������һ������
            GameObject hitPartticleEffect= Instantiate(hitParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            //a.ʵ����һ��ǹ��
            GameObject bullectHoleEffect = Instantiate(bullectHole, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));

            //b.�ݻ�������Ч����
            Destroy(hitPartticleEffect, 1f);
            Destroy(bullectHoleEffect, 3f);
        }

        //4.�����ɺ���Ҫ���еĲ���

        //a.���ſ���Ķ���
        if (!isAim)
        {//��ͨ������
            //animator.CrossFadeInFixedTime��ʹ������Ϊ��λ��ʱ�䴴���ӵ���״̬���κ�����״̬�ĵ��뵭��Ч��
            //�����ֱ���״̬�������ɵĳ���ʱ��
            animator.CrossFadeInFixedTime("fire", 0.1f);
        }
        else
        {//��׼�󿪻�
            animator.CrossFadeInFixedTime("aim_fire", 0.1f);

        }
        //b.��������ǰ�ĵ���������һ
        nowClipBullects--;
        //c.�����ӵ�������ui
        UpdateALLUI();
        //d.���������ǹ����������
        muzzleFlash.Play();
        //e.�����������Ч
        PlayerShootSound();
        //f.���ŵƹ⣨���ǰ������ϣ�
        muzzleFlashLight.enabled = true;
        //g.�����ӵ��ǣ�ʵ������
        Instantiate(casingPrefab, casingSpawnPoint.transform.position, casingSpawnPoint.transform.rotation);
        /*---------------------�����ɺ���Ҫ����ʱ�����¹��㣬��ͷ����cd----------------------------------------*/
        fireTimer = 0f;
    }

    /*------------------------------------------2.����UI-----------------------------------------------------------*/
    public void UpdateALLUI()
    {
        //�����ӵ���ui
        bulletsNumbUI.text = nowClipBullects + "/" + bulletsLeft;
        //����ȫ�Զ����Զ����͵�ui
        shootModelTextUI.text = shootUIName;
    }

    /*------------------------------------------3.����------------------------------------------------------------*/
    public void reloadClip()
    {
        //0.������������
        DoReloadAnimation();

        //1.��������ʣ����ӵ��ӵ������ӵ�����
        bulletsLeft += nowClipBullects;
        //2.Ȼ�����һ�����е��������ӵ����뵱ǰ�ĵ�����,��������ӵ������Ͱ�ȫ���ӵ����뵱ǰ�ĵ����У�ͬʱ�����ӵ��ÿ�
        if (bulletsLeft < aClipBullets)//�ӵ�����ԣ
        {
            nowClipBullects = bulletsLeft;
            bulletsLeft = 0;
        }
        else//�ӵ�����
        {
            nowClipBullects = aClipBullets;
            bulletsLeft -= aClipBullets;

        }
       
    }

    /*------------------------------------------4.���������Ч--------------------------------------------------*/
    public void PlayerShootSound()
    {
        //��ȡ��ЧƬ�Σ�����
        audioSource.clip = AK47SoundClip;
        audioSource.Play();
    }

    /*------------------------------------------5.���Ż�����������������Ч---------------------------------------*/
    public void DoReloadAnimation()
    {
        //���ݵ����е�֪�������Ų�ͬ�Ķ���
        if (nowClipBullects > 0)
        {//���Ŷ���1   ����1������������   2�������Ĳ㼶  3.ʱ���ƫ��
            animator.Play("reload_ammo_left",0,0);
            //ͬʱ���Ż�����Ч
            audioSource.clip = reloadClip1;
            audioSource.Play();
        }
        if(nowClipBullects == 0)
        {//�����е��ӵ�Ϊ0�����Ŷ���2
            animator.Play("reload_out_of_ammo", 0, 0);
            //ͬʱ���Ż�����Ч
            audioSource.clip = reloadClip2;
            audioSource.Play();


        }

    }

    /*------------------------------------------6.��׼���߼�-----------------------------------------------------*/
    public void DoAim()
    {
        //����������Ҽ�������װ�������Ǳ��ܵ�ʱ�򣬽�����׼
        if(Input.GetMouseButton(1) && !isReload && !playerMovement.isRun){
            //������׼״̬��׼����ʧ����Ұ��ǰ 
            isAim = true;
            animator.SetBool("Aim", true);//������׼״̬
            aimPointUI.gameObject.SetActive(false);//׼����ʧ
            playerCamera.fieldOfView = 25;//��׼��ʱ���������Ұ����

        }
        else
        {//������׼״̬
            isAim = false;
            animator.SetBool("Aim", false);
            aimPointUI.gameObject.SetActive(true);//׼������
            playerCamera.fieldOfView = 60;//�˳���׼��ʱ���������Ұ�ָ�ԭ����״̬

        }
    }

}
