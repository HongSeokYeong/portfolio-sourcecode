using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;
using ENUMS;

// 캐릭터가 등장한후 달려가는 스테이트
namespace Stage1Enemy
{
    public class RunState : State<Stage1Enemy>
    {
        // 싱글톤 생성
        #region Singleton
        private static RunState _instance;

        private RunState()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = this;
        }

        public static RunState Instance
        {
            get
            {
                if (_instance == null)
                {
                    new RunState();
                }

                return _instance;
            }
        }
        #endregion

        private float runJumpTimer = 0.0f;
        private bool jump = false;
        private float jumpTime = 0.0f;

        // 스테이트로 진입 했을때 초기화를 위한 함수
        // 현재 코루틴에 이 스테이트에서 처리 해야할 코루틴을 저장하고
        // 해당 코루틴을 캐릭터 클래스의 코루틴으로 넘겨준다.
        public override void EnterState(Stage1Enemy _owner)
        {
            jump = false;
            runJumpTimer = 0.0f;
            jumpTime = 0.0f;

            int r = Random.Range(0, 100);

            if(r < 40)
            {
                jump = true;
            }

            jumpTime = Random.Range(0.2f,0.8f);
            _stateCoroutine = RunCoroutine(_owner);
            _owner.currentCoroutine = _stateCoroutine;
        }

        // 스테이트가 종료될때 호출되는 함수
        // 종료시에 특별히 신경 써줘야 할 부분을 처리 해준다.
        public override void ExitState(Stage1Enemy _owner)
        {
            _stateCoroutine = null;
        }

        // 스테이트가 진행중이면서 추가적으로 처리할게 있을때 사용하는 함수
        public override void UpdateState(Stage1Enemy _owner)
        {
            if(_owner.runJump)
            {
                if (jump)
                {
                    runJumpTimer += Time.deltaTime;
                    if (runJumpTimer >= jumpTime)
                    {
                        jump = false;
                        _owner.runJump = false;

                        _owner.stateMachine.ChangeState(RunJump.Instance);
                    }
                }
            }
        }

        // 캐릭터와 충돌 했을때 실행할 함수
        public override void OnTriggerEnter(Stage1Enemy _owner, Collider other, bool playMouse = false)
        {
            if (other.CompareTag("RightHand") || other.CompareTag("LeftHand"))
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
                    if (Random.Range(0, 100) <= 22)
                    {
                        // 회피 구현
                        EffectManager.Instance.SetEffect(EffectType.Miss_effect, other.transform.position);
                        _owner.stateMachine.ChangeState(AvoidState.Instance);
                    }
                    else
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
            }
        }

        // 스테이트의 메인 행동
        // 스테이트의 성격에 따라 처리해야될 기능을 구현하는 부분
        private IEnumerator RunCoroutine(Stage1Enemy _owner)
        {
            _owner.animator.SetTrigger("Run3");
            _owner.agentSpeed = 2;

            // 지정한 방향(WayPoint) 으로 달려가는 함수

            if (_owner.handJoint.connectedBody == null)
            {
                Stage1BalloonManager.Instance.StealBalloon(_owner.handJoint, _owner);
            }

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