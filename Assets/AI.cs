using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour {

    NavMeshAgent agent;
    Animator anim;
    State currentState;

    public Transform player;

    void Start() {

        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();
        currentState = new Idle(this.gameObject, agent, anim, player);
    }

    void Update() {

        currentState = currentState.Process();
    }
}
