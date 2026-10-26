using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> balls;

    private int randomIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnBall(randomIndex));
    }

    public IEnumerator SpawnBall(int randomIndex)
    {
        balls[randomIndex].SetActive(true);
        balls.RemoveAt(randomIndex);

        if (balls.Count > 0)
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(SpawnBall(Random.Range(0, balls.Count)));
        }
    }
}