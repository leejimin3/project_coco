using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : BaseSingleton<ObstacleManager>
{
    public Dictionary<ObstacleElement, List<KeyCode>> TriggeredObstacle = new Dictionary<ObstacleElement, List<KeyCode>>();

    public void AddObstacle(ObstacleElement ele, int num)
    {
        List<KeyCode> codes = GameManager.Instance.GetRandomFriendsKeys(num);
        TriggeredObstacle.Add(ele, codes);
        
        List<Sprite> sprites = GameManager.Instance.GetFriendIcon(codes);
        
        //string key = GameManager.Instance.KeyCodeInString(codes[0]);
        
        ele.ShowKey(sprites[0]);
    }

    public void RemoveObstacle(ObstacleElement ele)
    {
        ele.HideKey();
        TriggeredObstacle.Remove(ele);
    }

    public void SuccecsObstacle(ObstacleElement ele)
    {
        ele.HideKey();
        TriggeredObstacle.Remove(ele);
        Destroy(ele.gameObject);
        
    }
}
