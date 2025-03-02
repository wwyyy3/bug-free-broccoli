using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using InfimaGames.LowPolyShooterPack;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;

public class ShooterAgent : Agent
{
    public GameManager gameManager;
    [SerializeField] private GameObject goal;
    [SerializeField] private GameObject monsterPrefab;
    
    private Character playerCharacter;
    private List<MonsterController> monsters = new List<MonsterController>();

    private int currentEpisode = 0;
    private float cumulativeReward = 0f;

    public override void Initialize()
    {
        currentEpisode = 0;
        cumulativeReward = 0f;
        playerCharacter = GetComponent<Character>();
        transform.position = new Vector3(83.8f, 1.8f, 13.8f);
    }

    public override void OnEpisodeBegin()
    {
        currentEpisode++;
        cumulativeReward = 0f;

        ResetAgent();
        SpawnObjects();
    }

    public override void CollectObservations(VectorSensor sensor)
    {

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        bool fire = actions.DiscreteActions[0] > 0;

        Rigidbody rb = playerCharacter.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(moveX, 0, moveZ) * 5f;
        }

        if (fire)
        {
            playerCharacter.AgentFire();
        }

       
        AddReward(-0.01f);  

        foreach (var monster in monsters)
        {
            if (monster.hitCount > 0)
            {
                AddReward(10f);
                monster.hitCount = 0;
            }
        }

        if (playerCharacter.currentHealth <= 0)
        {
            AddReward(-100f);
            EndEpisode();
        }
    }

    private void ResetAgent()
    {
        transform.position = new Vector3(83.8f, 1.8f, 13.8f); 
        playerCharacter.currentHealth = 10; 
    }

    private void SpawnObjects()
    {
        gameManager.SpawnMonsters(Vector3.zero);
        monsters.AddRange(Object.FindObjectsByType<MonsterController>(FindObjectsSortMode.None));
    }

    private void GoalReached ()
    {
        AddReward(100f);
        cumulativeReward = GetCumulativeReward();

        EndEpisode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            GoalReached();
        }
    }
}
