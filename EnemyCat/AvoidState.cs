using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;
using ENUMS;

// 캐릭터가 공격 당했을때 특정 확률로 진입되는 스테이트
namespace Stage1Enemy
{
    public class AvoidState : State<Stage1Enemy>
    {
        // 싱글톤 생성
        #region Singleton
        private static AvoidState _instance;

        private AvoidState()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = this;
        }

        public static AvoidState Instance
        {
            get
            {
                if (_instance == null)
                {
                    new AvoidState();
                }

                return _instance;
            }
        }
        #endregion

        // 스테이트로 진입 했을때 초기화를 위한 함수
        // 현재 코루틴에 이 스테이트에서 처리 해야할 코루틴을 저장하고
        // 해당 코루틴을 캐릭터 클래스의 코루틴으로 넘겨준다.
        public override void EnterState(Stage1Enemy _owner)
        {
            _stateCoroutine = AvoidCoroutine(_owner);
            _owner.currentCoroutine = _stateCoroutine;
        }

        // 스테이트가 종료될때 호출되는 함수
        // 종료시에 특별히 신경 써줘야 할 부분을 처리 해준다.
        public override void ExitState(Stage1Enemy _owner)
        {
            _stateCoroutine = null;
        }

        // 캐릭터와 충돌 했을때 실행할 함수
        public override void OnTriggerEnter(Stage1Enemy _owner, Collider other, bool playMouse)
        {
        }

        // 스테이트가 진행중이면서 추가적으로 처리할게 있을때 사용하는 함수
        public override void UpdateState(Stage1Enemy _owner)
        {
        }

        // 스테이트의 메인 행동
        // 스테이트의 성격에 따라 처리해야될 기능을 구현하는 부분
        private IEnumerator AvoidCoroutine(Stage1Enemy _owner)
        {
            int r = 0;
            SoundManager.PlaySound(ENUMS.SoundType.efs_dodge.ToString());

            _owner.animator.SetTrigger("DodgeL");

            do
            {
                r = Random.Range(0, 3);
                yield return null;
            } while (_owner.soundIndex == r);

            _owner.soundIndex = r;

            switch (_owner.soundIndex)
            {
                case 0:
                    SoundManager.PlaySound(ENUMS.SoundType.vcs_dark101A.ToString());
                    break;
                case 1:
                    SoundManager.PlaySound(ENUMS.SoundType.vcs_dark101B.ToString());
                    break;
                case 2:
                    SoundManager.PlaySound(ENUMS.SoundType.vcs_dark101C.ToString());
                    break;
                default:
                    break;
            }

            yield return new WaitForSeconds(0.5f);

            while (_owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
            {
                yield return null;
            }

            _owner.avoid = true;

            _owner.stateMachine.ChangeState(RunState.Instance);
            yield return null;
        }
    }
}