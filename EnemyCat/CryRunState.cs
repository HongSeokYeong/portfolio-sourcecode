using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

// 캐릭터가 플레이어에게 맞고난 후 진입되는 스테이트
namespace Stage1Enemy
{
    public class CryRunState : State<Stage1Enemy>
    {
        // 싱글톤 생성
        #region Singleton
        private static CryRunState _instance;

        private CryRunState()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = this;
        }

        public static CryRunState Instance
        {
            get
            {
                if (_instance == null)
                {
                    new CryRunState();
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
            _stateCoroutine = CryRunCoroutine(_owner);
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
                SoundManager.PlaySound(ENUMS.SoundType.efs_hit.ToString());
                EffectManager.Instance.SetEffect(ENUMS.EffectType.Hit_effect, other.transform.position);
                Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
            }
        }

        // 스테이트가 진행중이면서 추가적으로 처리할게 있을때 사용하는 함수
        public override void UpdateState(Stage1Enemy _owner)
        {
        }

        // 스테이트의 메인 행동
        // 스테이트의 성격에 따라 처리해야될 기능을 구현하는 부분
        private IEnumerator CryRunCoroutine(Stage1Enemy _owner)
        {
            _owner.animator.SetTrigger("CryRun");

            // 지정한 방향(WayPoint) 으로 달려가는 함수
            while (!PathManager.Instance.AgentOnTarget(_owner))
            {
                var direction = _owner.targetPosition.position - _owner.transform.position;

                _owner.transform.rotation = Quaternion.Slerp(_owner.transform.rotation, Quaternion.LookRotation(direction), 10.0f * Time.deltaTime);

                _owner.transform.position = Vector3.MoveTowards(_owner.transform.position, _owner.targetPosition.position, Time.deltaTime * _owner.agentSpeed);
                yield return null;
            }

            // 플레이어와 부딪히지 않고 목표 지점에 도착한 상황
            // 도착 했을때의 행동을 구현

            _owner.animator.SetTrigger("Idle");

            Stage1GameManager.Instance.StartCatPeepCoroutine();

            yield return null;
        }
    }
}