using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;
using ENUMS;

// AI 클래스를 상속받은 캐릭터 클래스
// 캐릭터의 초기화와 스테이트 머신의 처리를 담당 한다
namespace Stage1Enemy
{
    public class Stage1Enemy : AI
    {
        public StateMachine<Stage1Enemy> stateMachine { get; set; } // 스테이트 머신 저장 변수
        public BalloonType balloonType;
        public Stage1Position startPosition;

        public GameObject ModelData;

        private MegaMorph megaMorph;
        public int dir = 1;

        public bool avoid = false;
        public bool inScreen = false;
        public float avoidTime = 0.0f;

        public int soundIndex = 0;

        public int backDownNum = 0;
        public int hitDownNum = 0;

        public bool runJump = false;

        // Use this for initialization
        void Awake()
        {
            balloonType = BalloonType.NONE;
            currentCoroutine = null;

            StartCoroutine(MainCoroutine()); // 메인 코루틴을 실행 시킨다.

            stateMachine = new StateMachine<Stage1Enemy>(this); // 스테이트 머신 생성

            if (ModelData != null)
            {
                // 캐릭터의 얼굴 애니메이션을 위한 morph 세팅, morph는 에셋을 구입하여 사용
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

        // Update is called once per frame
        void Update()
        {
            // 스테이트 머신의 업데이트 처리
            stateMachine.Update();

            // 캐릭터가 화면 안에 있는지 밖에 있는지 판별한다.
            Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);

            if (pos.x >= 0.1f && pos.x <= 0.9f)
            {
                inScreen = true;
            }
            else
            {
                inScreen = false;
            }

            // 캐릭터가 회피할수 있는 시간을 체크한다.
            if (avoid)
            {
                avoidTime += Time.deltaTime;
                if (avoidTime >= 0.1f)
                {
                    avoid = false;
                    avoidTime = 0.0f;
                }
            }
        }

        // 캐릭터가 현재 가지고 있는 스테이트를 기반으로 코루틴을 실행 시킨다.
        // 각 스테이트가 실행할때 currentCoroutine 변수에 실행할 코루틴을 넘겨준다.
        public override IEnumerator MainCoroutine()
        {
            while (true)
            {
                if (currentCoroutine != null && currentCoroutine.MoveNext())
                {
                    yield return currentCoroutine.Current;
                }
                else
                {
                    yield return null;
                }
            }
        }

        // 애니메이션에서 호출되는 이벤트로 캐릭터의 사운드를 출력한다.
        public void PlaySoundBackDown()
        {
            if (backDownNum == 0)
            {
                SoundManager.PlaySound(ENUMS.SoundType.efs_backDown.ToString());
                backDownNum = 1;
            }
            else
            {
                SoundManager.PlaySound(ENUMS.SoundType.efs_backDown_2.ToString());
                backDownNum = 0;
            }
        }

        // 애니메이션에서 호출되는 이벤트로 캐릭터의 사운드를 출력한다.
        public void PlaySoundDown()
        {
            if(hitDownNum == 0)
            {
                SoundManager.PlaySound(ENUMS.SoundType.efs_down.ToString());
                hitDownNum = 1;
            }
            else
            {
                SoundManager.PlaySound(ENUMS.SoundType.efs_down_2.ToString());
                hitDownNum = 0;
            }
        }

        // 애니메이션에서 호출되는 이벤트로 캐릭터의 사운드를 출력한다.
        public void PlaySoundFoot(int i)
        {
            if (i == 1)
            {
                SoundManager.PlaySound(ENUMS.SoundType.efs_runA.ToString());
            }
            else
            {
                SoundManager.PlaySound(ENUMS.SoundType.efs_runB.ToString());
            }
        }

        // 애니메이션에서 호출되는 이벤트로 캐릭터의 사운드를 출력한다.
        public void PlaySoundCatDown()
        {
            int r = Random.Range(0, 5);
            switch (r)
            {
                case 0:
                    SoundManager.PlaySound(ENUMS.SoundType.vcs_dark100A.ToString());
                    break;
                case 1:
                    SoundManager.PlaySound(ENUMS.SoundType.vcs_dark100B.ToString());
                    break;
                case 2:
                    SoundManager.PlaySound(ENUMS.SoundType.vcs_dark100C.ToString());
                    break;
                case 3:
                    SoundManager.PlaySound(ENUMS.SoundType.vcs_dark100D.ToString());
                    break;
                case 4:
                    SoundManager.PlaySound(ENUMS.SoundType.vcs_dark100E.ToString());
                    break;
                default:
                    break;
            }
        }

        // 캐릭터의 특정 루틴이 반복 되었을때 다시 달리는 스테이트로 변환시켜주는 함수
        public void ResetRunState()
        {
            stateMachine.ChangeState(RunState.Instance);
        }

        // 캐릭터가 플레이어와 충돌 되었을때 호출되는 함수
        // 충돌 되었을때 각 스테이트에서 처리해야할 것을 처리 한다.
        public override void TriggerEnter(Collider other, bool mousePlay = false)
        {
            if (other.CompareTag("RightHand") || other.CompareTag("LeftHand"))
            {
                if (!avoid && inScreen)
                {
                    stateMachine.OnTriggerEnter(other, mousePlay);
                }
            }
        }
    }
}