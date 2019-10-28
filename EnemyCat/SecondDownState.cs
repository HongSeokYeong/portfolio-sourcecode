using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;
using ENUMS;

// 도망가는 캐릭터를 다시 공격해서 넘어졌을때의 스테이트
namespace Stage1Enemy
{
    public class SecondDownState : State<Stage1Enemy>
    {
        // 싱글톤 생성
        #region Singleton
        private static SecondDownState _instance;

        private SecondDownState()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = this;
        }

        public static SecondDownState Instance
        {
            get
            {
                if (_instance == null)
                {
                    new SecondDownState();
                }

                return _instance;
            }
        }
        #endregion

        public bool frontDown;

        // 스테이트로 진입 했을때 초기화를 위한 함수
        // 현재 코루틴에 이 스테이트에서 처리 해야할 코루틴을 저장하고
        // 해당 코루틴을 캐릭터 클래스의 코루틴으로 넘겨준다.
        public override void EnterState(Stage1Enemy _owner)
        {
            _stateCoroutine = DownCoroutine(_owner);
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
            if(other.CompareTag("RightHand") || other.CompareTag("LeftHand"))
            {
                EffectManager.Instance.SetEffect(EffectType.Hit_effect, other.transform.position);
                Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
            }
        }

        // 스테이트가 진행중이면서 추가적으로 처리할게 있을때 사용하는 함수
        public override void UpdateState(Stage1Enemy _owner)
        {
        }

        // 스테이트의 메인 행동
        // 스테이트의 성격에 따라 처리해야될 기능을 구현하는 부분
        private IEnumerator DownCoroutine(Stage1Enemy _owner)
        {
            if (frontDown)
            {
                _owner.animator.SetTrigger(AnimClip.HitDown.ToString()); // 앞으로 넘어짐 HitDown
            }
            else
            {
                _owner.animator.SetTrigger(AnimClip.BackDown.ToString()); // 뒤로 넘어짐 BackDown
            }

            yield return new WaitForSeconds(0.5f);

            while (_owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
            {
                yield return null;
            }

            _owner.stateMachine.ChangeState(CryRunState.Instance);
            yield return null;
        }
    }
}