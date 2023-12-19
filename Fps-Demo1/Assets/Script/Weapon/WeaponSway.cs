using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����ɫ���ߵ�ʱ������Ӧ����С���ȵİڶ���
/// ����˵����������ƶ�ʱ���ֱ���ҲӦ������ƫһ��
/// </summary>
public class WeaponSway : MonoBehaviour
{
    public float swayRange = 0.03f;//ҡ�ڷ���
    public float maxSwayRange = 0.06f;//����ҡ�ڷ���
    public float smoothSway = 6f;//ҡ��ƽ��ֵ ,��Ϊ��һ��λ�ñ仯����һ��λ���Ǻ�ͻأ��
    [SerializeField] private Vector3 originPosition;//�ֱ۳�ʼλ��



    // Start is called before the first frame update
    void Start()
    {
        //��ȡ�ֱ�һ��ʼ��λ��
        //position�Ǹ�������ԭ��Ϊ���� 2. localPosition�Ǹ��ݸ��ڵ�Ϊ����,���û�и��ڵ�,localpositon��position��û�������
        originPosition = transform.localPosition;
        
    }

    // Update is called once per frame
    void Update()
    {
        //��ȡ������ֵ,��Ϊ�ֱ�ֻ��ҪС���ȵ��ƶ������Կ��Գ�һ��ҡ�ڵķ���
        //ȡ����Ŀ�ľ��Ǻÿ�һ�㣬��������ֱ�����һ���
        float movementX = -Input.GetAxis("Mouse X") * swayRange;
        float movementY = -Input.GetAxis("Mouse Y") * swayRange;

        //���Ʋ��ܳ�������ҡ��
        Mathf.Clamp(movementX, -maxSwayRange, maxSwayRange);
        Mathf.Clamp(movementY, -maxSwayRange, maxSwayRange);



        //�����ֱ۱仯����ά����
        Vector3 finnallyPosition = new Vector3(movementX, movementY, 0);
        //���õ�ǰ�ֱ۵�λ��
        //Vector3.Lerp   ��ֵ������ a + (b - a) * t��ÿ�η��ص�ֵ���Ǵ�a��b��ĳ�ξ���
        //ʹ�ò�ֵ t �ڵ� a �� b ֮����в�ֵ������ t �����ڷ�Χ [0, 1] �ڡ�
        //������ڲ���ռ�����ն�֮������ض��ٷֱȵĵ㣨���磬�Ա�����Щ��֮�����ƶ�����
        //����һ����ǰ�ֱ۵�λ��  ������������������λ�ú������Ϸ��ʼ��ԭʼλ����ӣ�������겻����ʱ���ܻص�ԭ����
        //����������0-1֮��  �磺t = 0.1 Ҳ���Ƿ�����a��b�����е�10��֮1  (��λ�õ�ת�Ƹ��ӵ�ƽ��һ�㣬�򵥵�˵����ÿһ֡������ʱ�䣬�ı�һ�㣬)
        transform.localPosition = Vector3.Lerp(transform.localPosition, finnallyPosition +originPosition, Time.deltaTime * smoothSway);


    }
}
    