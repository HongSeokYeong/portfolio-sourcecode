using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateStuff
{
    // 스테이트의 변환, 업데이트 및 캐릭터의 충돌 여부를 판별하는 클래스
    public class StateMachine<T>
    {
        public State<T> currentState { get; private set; } // 현재 어떤 스테이트를 가지고 있는지 구별하기 위한 변수
        public T owner; // 스테이트 머신을 가지고 있는 주체가 누구인지 저장하기 위한 변수

        public StateMachine(T o)
        {
            owner = o;
            currentState = null;
        }

        // 스테이트를 변환 하는 함수
        public void ChangeState(State<T> _newState)
        {
            // 현재 스테이트가 있다면 해당 스테이트를 종료 시킨다.
            if (currentState != null)
            {
                currentState.ExitState(owner);
            }

            // 현재스테이트를 새로운 스테이트로 변경하고 해당 스테이트를 시작한다.
            currentState = _newState;
            currentState.EnterState(owner);
        }

        // 현재 저장되어 있는 스테이트를 주기적으로 업데이트 시켜준다.
        public void Update()
        {
            if(currentState != null)
            {
                currentState.UpdateState(owner);
            }
        }

        // 스테이트를 가지고 있는 주체의 충돌 여부를 판별 하는 함수.
        public void OnTriggerEnter(Collider other, bool playMouse = false)
        {
            if(currentState != null)
            {
                currentState.OnTriggerEnter(owner, other, playMouse);
            }
        }
    }

    // 스테이를 관리하기 위한 추상 클래스
    public abstract class State<T>
    {
        protected IEnumerator _stateCoroutine; // 스테이트의 코루틴 함수를 저장하기 위한 변수

        public abstract void EnterState(T _owner); // 스테이트의 시작 함수
        public abstract void ExitState(T _owner); // 스테이트의 종료 함수
        public abstract void UpdateState(T _owner); // 스테이트의 업데이트 함수
        public abstract void OnTriggerEnter(T _owner, Collider other, bool playMouse = false); // 충돌 처리를 위한 함수
    }
}