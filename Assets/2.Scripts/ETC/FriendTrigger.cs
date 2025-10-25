using System;
using UnityEngine;

public class FriendTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coco"))
        {
            GameManager.Instance.OpenNextFriend();
        }
    }
}
