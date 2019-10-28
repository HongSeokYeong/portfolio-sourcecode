using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateStuff;

// 캐릭터가 기본적으로 가져야 할 변수와 함수를 포함한 추상 클래스
public abstract class AI : MonoBehaviour
{
    public IEnumerator currentCoroutine; // 현재 실행중인 코루틴을 저장하기 위한 변수
    public Animator animator; // 캐릭터의 애니메이터를 저장하기 위한 변수
    public Transform targetPosition; // 캐릭터가 움직여야 할 위치를 저장하는 변수
    public float agentSpeed; // 캐릭터의 이동 속도를 위한 변수
    public ConfigurableJoint handJoint; // 캐릭터가 특정 오브젝트를 잡기 위한 조인트 변수
    public CollisionEvent ce; // 충돌 이벤트를 위한 변수

    public abstract IEnumerator MainCoroutine(); // 현재 스테이트의 코루틴을 실행 시키기 위한 함수

    public abstract void TriggerEnter(Collider other, bool mousePlay = false); // 캐릭터의 충돌을 확인하기 위한 함수
}