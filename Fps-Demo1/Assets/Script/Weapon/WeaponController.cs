using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 武器控制器
/// 1.射击逻辑
/// 2.射击速度控制
/// 3.瞄准点和子弹数量，人物血量的ui设置
/// 4.火焰特性灯光，子弹命中的粒子特性，弹孔
/// 5.添加相对应的动画（射击，换弹，瞄准），人物的行走和奔跑动画的播放也放到这里来了（因为动画状态机就在这里）
/// </summary>
public class WeaponController : MonoBehaviour
{

    private Animator animator;//动画状态机
    public PlayerMovement playerMovement;//角色移动，用于给他们添加动画
    private Camera playerCamera;//主角的相机

    public Transform shootPoint;//射击点
    public int aClipBullets = 30;//一个弹夹中子弹数量 
    public int range = 100;//武器射程
    public float fireRate = 0.5f;//武器射速(多少秒能射一次，相当于射击cd)
    public float fireTimer = 0f;//计时器
    public int bulletsLeft = 300;//备弹
    public int nowClipBullects;//当前弹夹数量
  

    public ParticleSystem muzzleFlash;//枪口火焰特性
    public Light muzzleFlashLight;//枪口火焰灯光


    public GameObject hitParticle;//子弹击中的粒子特性
    public GameObject bullectHole;//弹孔


    private AudioSource audioSource;//声音资源
    public AudioClip AK47SoundClip;//ak的射击音效片段
    public AudioClip reloadClip1;//换子弹的音效1
    public AudioClip reloadClip2;//换子弹的音效2

    public bool gunShootInput;//检测是否触发了射击
    public bool isReload;//是否在换弹
    public bool isAim;//是否在瞄准

    public Transform casingSpawnPoint;//子弹壳抛出的位置
    public Transform casingPrefab;//子弹壳的预制体

    /*枚举类型区分全自动和半自动*/
    //1:全自动  2：半自动
    public enum ShootModel{AutoRife,SemiGun};
    public ShootModel shootingMode;//这个对象里面可以存储枚举中其中的一个值

    private int shootType = 1;//模式切换的一个中间参数 ，1.全自动 2.半自动  默认为1全自动
    private string shootUIName;//用于不同模式的ui显示

    [Header("按键设置")]
    [SerializeField][Tooltip("填充子弹按键")]private KeyCode reloadClipInputName;
    [SerializeField] [Tooltip("检视武器按键")] private KeyCode inspectClipInputName;
    [SerializeField] [Tooltip("全自动/半自动切换")] private KeyCode gunShootModelInputName;
   /* [SerializeField] [Tooltip("小刀攻击")] private KeyCode knife_attack_1InputName;*/


    [Header("UI设置")]
    public Image aimPointUI;//准星ui
    public Text bulletsNumbUI;//子弹数量ui
    public Text shootModelTextUI;//全自动半自动ui


    // Start is called before the first frame update
    void Start()
    {
        //1.初始化当前弹夹数量
        nowClipBullects = aClipBullets;
        //2.初始化子弹数量的ui
        UpdateALLUI();
        //3.设置填充子弹的按键
        reloadClipInputName = KeyCode.R;
        //4.获取声音源
        audioSource = GetComponent<AudioSource>();
        //5.获取动画状态机
        animator = GetComponent<Animator>();
        //6.设置检视武器按键
        inspectClipInputName = KeyCode.H;
        //7.获取主相机
        playerCamera = Camera.main;
        //8.ak默认全自动模式
        shootingMode = ShootModel.AutoRife;
        shootUIName = "全自动";
        //9.切换全自动和半自动的按钮
        gunShootModelInputName = KeyCode.X;
       /* //10.设置小刀动作按键
        knife_attack_1InputName = KeyCode.F;
       */
    }


    void Update()
    {

        //0.模式的切换
        //如果按下了x并且中间参数不为1时
        if (Input.GetKeyDown(gunShootModelInputName)&& shootType != 1)
        {
            shootType = 1;
            shootUIName = "全自动";
            shootingMode = ShootModel.AutoRife;//将枚举类型设置为全自动
            //shootModelTextUI.text = shootUIName;
        }else if(Input.GetKeyDown(gunShootModelInputName) && shootType != 0)
        {
            shootType = 0 ;
            shootUIName = "半自动";
            shootingMode = ShootModel.SemiGun;//将枚举类型设置为半自动
          //  shootModelTextUI.text = shootUIName;
        }

        //根据枚举类型类型选择对应的输入方式吗，用这个的目的是练习，还有就是可能后面还会有其他的功能
        switch (shootingMode)
        {
            case ShootModel.AutoRife:
                //全自动，获取按下时的所有
                gunShootInput = Input.GetMouseButton(0);
                fireRate = 0.1f;//同时设置它的射速
                break;
            case ShootModel.SemiGun:
                //半自动,只获取按下的那一次
                gunShootInput = Input.GetMouseButtonDown(0);
                fireRate = 0.2f;
                break;
        }

        //1.当按下鼠标左键的时候，触发射击事件 （左键：0 中键：2  右键 ：1）
        //gunShootInput = Input.GetMouseButton(0);
        if (gunShootInput)//按下按键就开火
        {
            GunFire();
        }else//没开火，但是需要把开火的灯光隐藏
        {
            muzzleFlashLight.enabled = false;
        }

        //2.控制武器的射击速度，进行冷却状态累加时间------------------------------------------
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        /*3.判断是否在换弹*/
        //获取动画机第0层的数据
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        //两种换子弹的动画
        if (info.IsName("reload_ammo_left") || info.IsName("reload_out_of_ammo"))
        {
            isReload = true;
        }
        else
        {
            isReload = false;
            //3.更新子弹显示的ui
            UpdateALLUI();
        }


        //4.当按下r键和弹夹子弹不是满，备弹不为0时，触发换弹动作
        if(Input.GetKeyDown(reloadClipInputName) && nowClipBullects< aClipBullets && bulletsLeft>0)
        {
            reloadClip();
        }

        //5.当按下h键时，播放检查武器的动画
        if (Input.GetKeyDown(inspectClipInputName))
        {
            animator.SetTrigger("inspect");     
        }
        //6.设置奔跑和行走的动画
        animator.SetBool("Run", playerMovement.isRun);
        animator.SetBool("Walk", playerMovement.isWalk);

        //7.当弹夹子弹为空的时候，子弹换弹夹
        if(nowClipBullects == 0 && bulletsLeft > 0)
        {
            reloadClip();
        }
        //8.按下鼠标右键时进入瞄准
        DoAim();

       /* //9.按下f播放小刀攻击
        if (Input.GetKeyDown(knife_attack_1InputName))
        {
            animator.SetTrigger("knifeAttack");
        }
        */

    }

    //------------------------------------------1.射击功能-----------------------------------------------------
    public void GunFire()
    {
        //1.先判断能不能射击
        //a.其目的就是为了减少射线执行的数度/*开始之前先判断计时器是不是到达了射击时间的要求，如果不到时间就之间返回*/
        //b.当弹夹子弹数量为空就不能射击了
        //c.换子弹动作时不能射击
        //d.奔跑时不能射击
        if (fireTimer < fireRate || nowClipBullects <=0 || isReload || playerMovement.isRun)
        {
            return;
        }

        //2.定义射击的方向(向前的方向)
        Vector3 shootDiretion= shootPoint.forward;
        //3.使用射线检测,然后模拟射中之后的操作
        RaycastHit hit;//用于接收射线返回的信息
        if (Physics.Raycast(shootPoint.position, shootDiretion, out hit ,range))
        {
            Debug.Log("打到了");
            //a.实例化一个击中的特效对象
            // Quaternion.FromToRotation从一个方向转到另外一个方向
            GameObject hitPartticleEffect= Instantiate(hitParticle, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            //a.实例化一个枪孔
            GameObject bullectHoleEffect = Instantiate(bullectHole, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));

            //b.摧毁两个特效对象
            Destroy(hitPartticleEffect, 1f);
            Destroy(bullectHoleEffect, 3f);
        }

        //4.射击完成后需要进行的操作

        //a.播放开火的动画
        if (!isAim)
        {//普通的腰射
            //animator.CrossFadeInFixedTime是使用以秒为单位的时间创建从单曲状态到任何其他状态的淡入淡出效果
            //参数分别是状态名，过渡的持续时间
            animator.CrossFadeInFixedTime("fire", 0.1f);
        }
        else
        {//瞄准后开火
            animator.CrossFadeInFixedTime("aim_fire", 0.1f);

        }
        //b.完成射击后当前的弹夹数量减一
        nowClipBullects--;
        //c.更新子弹数量的ui
        UpdateALLUI();
        //d.播放射击的枪火粒子特性
        muzzleFlash.Play();
        //e.播放射击的音效
        PlayerShootSound();
        //f.播放灯光（就是把他勾上）
        muzzleFlashLight.enabled = true;
        //g.生成子弹壳（实例化）
        Instantiate(casingPrefab, casingSpawnPoint.transform.position, casingSpawnPoint.transform.rotation);
        /*---------------------射击完成后需要将记时器重新归零，这就发射的cd----------------------------------------*/
        fireTimer = 0f;
    }

    /*------------------------------------------2.更新UI-----------------------------------------------------------*/
    public void UpdateALLUI()
    {
        //更新子弹的ui
        bulletsNumbUI.text = nowClipBullects + "/" + bulletsLeft;
        //更新全自动半自动类型的ui
        shootModelTextUI.text = shootUIName;
    }

    /*------------------------------------------3.换弹------------------------------------------------------------*/
    public void reloadClip()
    {
        //0.触发换弹动作
        DoReloadAnimation();

        //1.将弹夹中剩余的子弹加到备用子弹数中
        bulletsLeft += nowClipBullects;
        //2.然后分配一个弹夹的数量的子弹放入当前的弹夹中,如果备用子弹不够就把全部子弹放入当前的弹夹中，同时备用子弹置空
        if (bulletsLeft < aClipBullets)//子弹不充裕
        {
            nowClipBullects = bulletsLeft;
            bulletsLeft = 0;
        }
        else//子弹充足
        {
            nowClipBullects = aClipBullets;
            bulletsLeft -= aClipBullets;

        }
       
    }

    /*------------------------------------------4.播放射击音效--------------------------------------------------*/
    public void PlayerShootSound()
    {
        //获取音效片段，播放
        audioSource.clip = AK47SoundClip;
        audioSource.Play();
    }

    /*------------------------------------------5.播放换弹动作动画还有音效---------------------------------------*/
    public void DoReloadAnimation()
    {
        //根据弹夹中的知道数播放不同的动画
        if (nowClipBullects > 0)
        {//播放动画1   参数1：动画的名称   2：动画的层级  3.时间的偏移
            animator.Play("reload_ammo_left",0,0);
            //同时播放换弹音效
            audioSource.clip = reloadClip1;
            audioSource.Play();
        }
        if(nowClipBullects == 0)
        {//弹夹中的子弹为0，播放动画2
            animator.Play("reload_out_of_ammo", 0, 0);
            //同时播放换弹音效
            audioSource.clip = reloadClip2;
            audioSource.Play();


        }

    }

    /*------------------------------------------6.瞄准的逻辑-----------------------------------------------------*/
    public void DoAim()
    {
        //当按下鼠标右键，不是装弹，不是奔跑的时候，进行瞄准
        if(Input.GetMouseButton(1) && !isReload && !playerMovement.isRun){
            //进入瞄准状态，准星消失，视野靠前 
            isAim = true;
            animator.SetBool("Aim", true);//进入瞄准状态
            aimPointUI.gameObject.SetActive(false);//准星消失
            playerCamera.fieldOfView = 25;//瞄准的时候摄像机视野拉近

        }
        else
        {//不是瞄准状态
            isAim = false;
            animator.SetBool("Aim", false);
            aimPointUI.gameObject.SetActive(true);//准星启用
            playerCamera.fieldOfView = 60;//退出瞄准的时候摄像机视野恢复原来的状态

        }
    }

}
