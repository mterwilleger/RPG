using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Enemy : MonoBehaviourPun
{
    [Header("Info")]
    public string enemyName;
    public float moveSpeed;

    public int curHp;
    public int maxHp;

    public float chaseRange;
    public float attackRange;

    private PlayerController targetPlayer;

    public float playerDetectRate = 0.2f;
    public float lastPlayerDetectTime;

    public string objectToSpawnOnDeath;

    [Header("Attack")]
    public int damage;
    public float attackRate;
    public float lastAttackTime;

    [Header("Components")]
    public HeaderInfo healthBar;
    public SpriteRenderer sr;
    public Rigidbody2D rig;

    void Start()
    {
        healthBar.Initialize(enemyName, maxHp);
    }

    void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

        if(targetPlayer != null)
        {
            float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);

            //if were able to attack player do so
            if(dist < attackRange && Time.time - lastAttackTime >= attackRange)
                Attack();
            //Otherwise do we move after the player?
            else if(dist > attackRange)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                rig.velocity = dir.normalized * moveSpeed;
            }
            else
            {
                rig.velocity = Vector2.zero;
            }
        }

        DetectPlayer();
    }

    //Attack the target player
    void Attack()
    {
        lastAttackTime = Time.time;
        targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, damage);
    }

    //updates target player
    void DetectPlayer()
    {
        if(Time.time - lastPlayerDetectTime > playerDetectRate)
        {
            lastPlayerDetectTime = Time.time;

            //look through all palyers
            foreach(PlayerController player in GameManager.instance.players)
            {
                //calculate distance between us and player
                float dist = Vector2.Distance(transform.position, player.transform.position);

                if(player == targetPlayer)
                {
                    if(dist > chaseRange)
                        targetPlayer = null;
                }
                else if(dist < chaseRange)
                {
                    if(targetPlayer == null)
                        targetPlayer = player;
                }
            }
        }
    }

}
