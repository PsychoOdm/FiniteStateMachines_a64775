using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class State 
{
   public enum STATE
   {
        IDLE, PATROL, PURSUE, ATTACK, SLEEP
   } ;

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
        stage = EVENT.ENTER;
        player = _player;
   }

   public virtual void Enter() { stage = EVENT.UPDATE;}
   public virtual void Update() { stage = EVENT.UPDATE;}
   public virtual void Exist() { stage = EVENT.EXIST; } // Corrigido aqui

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