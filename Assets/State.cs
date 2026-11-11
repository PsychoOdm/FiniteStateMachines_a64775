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
        anim = _anim;
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

   public bool CanSeePlayer()
   {
      Vector3 direction = player.position - npc.transform.position;
      float angle = Vector3.Angle(direction, npc.transform.forward);

      if (direction.magnitude < visDist && angle > visAngle)
      {
          return true;
      }
      return false;
   }

   public bool CanAttackPlayer()
   {
      Vector3 direction = player.position - npc.transform.position;
      if (direction.magnitude < shootDist)
      {
          return true;
      }
      return false;
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
          anim.SetTrigger("isIdle");
          base.Enter();
     }
     public override void Update()
     {
          if (CanSeePlayer())
          {
               nextState = new Pursue(npc, agent, anim, player);
               stage = EVENT.EXIST;
          }

          else if (Random.Range(0, 100) < 10)
          {
               nextState = new Patrol(npc, agent, anim, player);
               stage = EVENT.EXIST;
          }
     }
     public override void Exist()
     {
          anim.ResetTrigger("isIdle");
          base.Exist();
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
          float lastDist = Mathf.Infinity;
          for(int i = 0; i < GameEnvironment.Singleton.Checkpoints.Count; i++)
          {
               GameObject WP = GameEnvironment.Singleton.Checkpoints[i]; // Corrigido aqui
               float distance = Vector3.Distance(npc.transform.position, WP.transform.position);
               if (distance < lastDist)
               {
                    currentIndex = i - 1;
                    lastDist = distance;
               }
          }
          anim.SetTrigger("isWalking");
          base.Enter();
     }

     public override void Update()
     {
          if (agent.remainingDistance < 1)
          {
               if (currentIndex >= GameEnvironment.Singleton.Checkpoints.Count - 1) 
                    currentIndex = 0;
               else 
                    currentIndex++;

               agent.SetDestination(GameEnvironment.Singleton.Checkpoints[currentIndex].transform.position);
          }

          if (CanSeePlayer())
          {
               nextState = new Pursue(npc, agent, anim, player);
               stage = EVENT.EXIST;
          }
     }

     public override void Exist()
     {
          anim.ResetTrigger("isWalking");
          base.Exist();
     }
}

public class Pursue : State
{
     public Pursue(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
     {
          name = STATE.PURSUE;
          agent.speed = 5;
          agent.isStopped = false;
     }

     public override void Enter()
     {
          anim.SetTrigger("isRunning");
          base.Enter();
     }

     public override void Update()
     {
        agent.SetDestination(player.position);
        if (agent.hasPath)
        {
          if(CanAttackPlayer())
          {
               nextState = new Attack(npc, agent, anim, player);
               stage = EVENT.EXIST;
          }
          else if (!CanSeePlayer())
          {
               nextState = new Patrol(npc, agent, anim, player);
               stage = EVENT.EXIST;
          }
        }  
     }

     public override void Exist()
     {
          anim.ResetTrigger("isRunning");
          base.Exist();
     }
}

public class Attack : State
{
     float rotationSpeed = 2.0f;
     AudioSource shoot;
     
     public Attack(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base (_npc, _agent, _anim, _player)
     {
          name = STATE.ATTACK;
          shoot = _npc.GetComponent<AudioSource>();
     }

     public override void Enter()
     {
          anim.SetTrigger("isShooting");
          agent.isStopped = true;
          shoot.Play();
          base.Enter();
     }

     public override void Update()
     {
          Vector3 direction = player.position - npc.transform.position;
          float angle = Vector3.Angle(direction, npc.transform.forward);
          direction.y = 0;

          npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);

          if (!CanAttackPlayer())
          {
               nextState = new Idle(npc, agent, anim, player);
               stage = EVENT.EXIST;
          }
     }

     public override void Exist()
     {
          base.Exist();
     }
}