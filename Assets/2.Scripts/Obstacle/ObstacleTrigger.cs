using System;
using UnityEngine;

public class ObstacleTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            ObstacleElement ele = other.GetComponent<ObstacleElement>();
            ObstacleManager.Instance.AddObstacle(ele, 1);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            ObstacleElement ele = other.GetComponent<ObstacleElement>();
            ObstacleManager.Instance.RemoveObstacle(ele);
        }
    }
}
    