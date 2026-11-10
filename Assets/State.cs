using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class State 
{
   public enum STATE
   {
        IDLE, PATROL, PURSUE, ATTACK, SLEEP
   };

   public enum EVENT
   {
        ENTER, UPDATE, EXIST
   };


   public STATE name;
   protected EVENT stage;
   protected GameObject npc;
   protected Animator anim;
   protected Transform player;
   protected State nextState;
   protected NavMeshAgent agent;

   float visDist = 10.0f;
   float visAngle = 30.0f;
   float shootDist = 7.0f;

   public State(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
   {
        npc = _npc;
        agent = _agent;
        anim = _anim; // Corrigido: faltava inicializar o anim
        stage = EVENT.ENTER;
        player = _player;
   }

   public virtual void Enter() { stage = EVENT.UPDATE; }
   public virtual void Update() { stage = EVENT.UPDATE; }
   public virtual void Exist() { stage = EVENT.EXIST; }

   public State Process()
   {
        if (stage == EVENT.ENTER) Enter();
        if (stage == EVENT.UPDATE) Update();
        if (stage == EVENT.EXIST)
        {
            Exist();
            return nextState;
        }
        return this;
   }
}

public class Idle : State
{
     public Idle(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
          : base(_npc, _agent, _anim, _player)
     {
          name = STATE.IDLE;
     }

     public override void Enter()
     {
          // currentIndex foi movido para Patrol, mas se quiser usar aqui, declare-o. 
          // Removido o currentIndex daqui para evitar erro de variável não existente.
          anim.SetTrigger("isIdle");
          base.Enter();
     }
     public override void Update()
     {
          if (Random.Range(0, 100) < 10)
          {
               nextState = new Patrol(npc, agent, anim, player); // Corrigido: faltava o 'anim'
               stage = EVENT.EXIST; // Corrigido: EVENT.EXIST (maiúsculo/minúsculo correto)
          }
          base.Update();
     }
     public override void Exist() // Corrigido: de EXIST() para Exist() para bater com a classe base
     {
          anim.ResetTrigger("isIdle");
          base.Exist(); // Corrigido: faltavam os parênteses ()
     }
}

public class Patrol : State
{
     int currentIndex = -1;

     public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
     : base(_npc, _agent, _anim, _player)
     {
         name = STATE.PATROL;
         agent.speed = 2;
         agent.isStopped = false;
     } 

     public override void Enter()
     {
         anim.SetTrigger("isWalking");
         base.Enter();
     }

     public override void Update()
     {
         // Corrigido: 'remaininDistance' para 'remainingDistance'
         if (agent.remainingDistance < 1)
         {
              // Corrigido: Count - 1 para evitar estouro de índice (IndexOutOfRangeException)
              if (currentIndex >= GameEnvironment.Singleton.Checkpoints.Count - 1) 
                  currentIndex = 0;
              else 
                  currentIndex++;

              // Corrigido: Sintaxe correta para o SetDestination
              agent.SetDestination(GameEnvironment.Singleton.Checkpoints[currentIndex].transform.position);
         }
         base.Update();
     }

     public override void Exist()
     {
         anim.ResetTrigger("isWalking");
         base.Exist();
     }
}