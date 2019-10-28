using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;
using ENUMS;

// 캐릭터가 공격 당한 후 도망가는 스테이트
namespace Stage1Enemy
{
    public class RunAwayState : State<Stage1Enemy>
    {
        // 싱글톤 생성
        #region Singleton
        private static RunAwayState _instance;

        private RunAwayState()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = this;
        }

        public static RunAwayState Instance
        {
            get
            {
                if (_instance == null)
                {
                    new RunAwayState();
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
            _owner.animator.SetTrigger("Run4");

            float f1 = Vector3.Distance(_owner.transform.position, PathManager.Instance.wayPoint[2].position);
            float f2 = Vector3.Distance(_owner.transform.position, PathManager.Instance.wayPoint[3].position);

            // f1이 더 작으면 왼쪽 박스에 더 가깝다.
            // 더 가까운 쪽으로 도망 나간다.
            if (f1 < f2)
            {
                _owner.targetPosition = PathManager.Instance.wayPoint[2];
            }
            else
            {
                _owner.targetPosition = PathManager.Instance.wayPoint[3];
            }

            _stateCoroutine = RunAwayCoroutine(_owner);
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
            if (other.CompareTag("RightHand") || other.CompareTag("LeftHand"))
            {
                SoundManager.PlaySound(ENUMS.SoundType.efs_hit.ToString());
                EffectManager.Instance.SetEffect(EffectType.Hit_effect, other.transform.position);

                if (playMouse)
                {
                    _owner.ce = other.GetComponent<MouseCollisionEvent>();
                }
                else
                {
                    _owner.ce = other.GetComponent<HandCollisionEvent>();
                }

                float touchPos = _owner.ce.CalculateHandPos();

                if (_owner.transform.forward.z > 0)
                {
                    if (touchPos <= 0) // 앞에서 가격
                    {
                        SecondDownState.Instance.frontDown = true;
                        Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
                    }
                    else
                    {
                        SecondDownState.Instance.frontDown = false;
                        Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
                    }
                }
                else
                {
                    if (touchPos <= 0) // 뒤에서 가격
                    {
                        SecondDownState.Instance.frontDown = false;
                        Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
                    }
                    else
                    {
                        SecondDownState.Instance.frontDown = true;
                        Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
                    }
                }

                _owner.stateMachine.ChangeState(SecondDownState.Instance);
            }
        }

        // 스테이트가 진행중이면서 추가적으로 처리할게 있을때 사용하는 함수
        public override void UpdateState(Stage1Enemy _owner)
        {
        }

        // 스테이트의 메인 행동
        // 스테이트의 성격에 따라 처리해야될 기능을 구현하는 부분
        private IEnumerator RunAwayCoroutine(Stage1Enemy _owner)
        {
            int r = Random.Range(0, 2);
            if(r == 0)
            {
                SoundManager.PlaySound(ENUMS.SoundType.vcs_dark102.ToString());
            }

            while (!PathManager.Instance.AgentOnTarget(_owner))
            {
                var direction = _owner.targetPosition.position - _owner.transform.position;

                _owner.transform.rotation = Quaternion.Slerp(_owner.transform.rotation, Quaternion.LookRotation(direction), 10.0f * Time.deltaTime);

                _owner.transform.position = Vector3.MoveTowards(_owner.transform.position, _owner.targetPosition.position, Time.deltaTime * _owner.agentSpeed);
                yield return null;
            }

            _owner.animator.SetTrigger("Idle");

            Stage1GameManager.Instance.StartCatPeepCoroutine();
            yield return null;
        }
    }
}