using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 화면 양 끝에서 단순하게 고개만 내미는 캐릭터 클래스
public class PeepCat : MonoBehaviour
{
    public Animator animator;

    public GameObject ModelData;

    private MegaMorph megaMorph;

    // 고개 내미는 애니메이션 및 얼굴 애니메이션을 위한 morph 세팅
    public void StartPeep()
    {
        StartCoroutine(Peeping());

        if (ModelData != null)
        {
            megaMorph = (MegaMorph)ModelData.GetComponent<MegaMorph>();
            SetMorph(1);
        }
    }

    // 캐릭터 얼굴 애니메이션을 위한 morph 세팅 함수, 에셋을 구입하여 사용
    public void SetMorph(int morphCnt)
    {
        if (megaMorph == null)
            return;

        float looptime = 0.0f;

        int start = morphCnt * 29;
        int end = (morphCnt + 1) * 29;
        int count = 0;

        if (megaMorph.chanBank[count] == null || !MorpManager.Instance.IsChanBankData())
            return;

        for (int i = start; i < end; i++)
        {
            megaMorph.chanBank[count] = MorpManager.Instance.GetChanBank(i);

            MegaMorphChan mc = megaMorph.chanBank[count];

            count++;

            if (mc.control != null) // ISSUE: On 2nd load we suddenly have controls for no reason
            {
                megaMorph.animate = true;
                if (mc.control.Times != null && mc.control.Times.Length > 0)
                {
                    float t = mc.control.Times[mc.control.Times.Length - 1];
                    if (t > looptime)
                        looptime = t;
                }
            }
            megaMorph.looptime = looptime;
        }
    }

    // 고개 내미는 동작 1회 실행
    // 동작이 종료되면 게임 매니저에 알려준다.
    public IEnumerator Peeping()
    {
        animator.SetTrigger("Peep");
        SoundManager.PlaySound(ENUMS.SoundType.efs_peep.ToString());
        yield return new WaitForSeconds(0.5f);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
        {
            yield return null;
        }
        Stage1GameManager.Instance.catPeepping = false;
        animator.SetTrigger("Idle");
        yield return null;
    }
}