using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGate : MonoBehaviour
{
    [SerializeField] private GameObject bossGate;
    [SerializeField] private EnemyHealth bossHealth;

    void Start()
    {
        bossHealth.OnEnemyDeath += OpenGate;
    }

    private void OpenGate()
    {
        bossGate.SetActive(false);
        bossHealth.OnEnemyDeath -= OpenGate;
    }
}
