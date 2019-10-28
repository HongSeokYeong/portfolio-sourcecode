using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;
using ENUMS;

// 캐릭터가 특정 확률로 점프하며 등장하는 스테이트
namespace Stage1Enemy
{
    public class RunJump : State<Stage1Enemy>
    {
        // 싱글톤 생성
        #region Singleton
        private static RunJump _instance;

        private RunJump()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = this;
        }

        public static RunJump Instance
        {
            get
            {
                if (_instance == null)
                {
                    new RunJump();
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
            _stateCoroutine = RunJumpCoroutine(_owner);
            _owner.currentCoroutine = _stateCoroutine;
        }

        // 스테이트가 종료될때 호출되는 함수
        // 종료시에 특별히 신경 써줘야 할 부분을 처리 해준다.
        public override void ExitState(Stage1Enemy _owner)
        {
            _stateCoroutine = null;
        }

        // 캐릭터와 충돌 했을때 실행할 함수
        public override void OnTriggerEnter(Stage1Enemy _owner, Collider other, bool playMouse = false)
        {
            if (playMouse)
            {
                _owner.ce = other.GetComponent<MouseCollisionEvent>();
            }
            else
            {
                _owner.ce = other.GetComponent<HandCollisionEvent>();
            }

            if (_owner.ce.CalculateHandSpeed() >= 0.1f)
            {
                EffectManager.Instance.SetEffect(EffectType.HitCat_effect, other.transform.position);

                SoundManager.PlaySound(ENUMS.SoundType.efs_hit.ToString());

                Stage1GameManager.Instance.TakeScreenShot();

                float touchPos = _owner.ce.CalculateHandPos();

                if (_owner.transform.forward.x > 0)
                {
                    if (touchPos <= 0) // 손이 왼쪽에서 오른쪽으로 이동
                    {
                        DownState.Instance.frontDown = true; // 앞으로 넘어짐
                        Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
                    }
                    else
                    {
                        DownState.Instance.frontDown = false;
                        Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
                    }
                }
                else
                {
                    if (touchPos <= 0) // 손이 왼쪽에서 오른쪽으로 이동
                    {
                        DownState.Instance.frontDown = false;
                        Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
                    }
                    else
                    {
                        DownState.Instance.frontDown = true; // 앞으로 넘어짐
                        Stage1GameManager.Instance.CalculateGameScore(ScoreProperty.SCORE_PLUS_1);
                    }
                }

                _owner.stateMachine.ChangeState(DownState.Instance);
            }
        }

        // 스테이트가 진행중이면서 추가적으로 처리할게 있을때 사용하는 함수
        public override void UpdateState(Stage1Enemy _owner)
        {
        }

        // 스테이트의 메인 행동
        // 스테이트의 성격에 따라 처리해야될 기능을 구현하는 부분
        private IEnumerator RunJumpCoroutine(Stage1Enemy _owner)
        {
            _owner.animator.SetTrigger("RunJump");

            while (!PathManager.Instance.AgentOnTarget(_owner))
            {
                var direction = _owner.targetPosition.position - _owner.transform.position;

                _owner.transform.rotation = Quaternion.Slerp(_owner.transform.rotation, Quaternion.LookRotation(direction), 10.0f * Time.deltaTime);

                _owner.transform.position = Vector3.MoveTowards(_owner.transform.position, _owner.targetPosition.position, Time.deltaTime * _owner.agentSpeed);
                yield return null;
            }

            Stage1BalloonManager.Instance.ResetBalloon(_owner.handJoint, _owner.balloonType);

            _owner.animator.SetTrigger("Idle");
            Stage1GameManager.Instance.StartCatPeepCoroutine();

            yield return null;
        }
    }
}