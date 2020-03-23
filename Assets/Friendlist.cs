using Barebones.Networking;
using UnityEngine;

public class Friendlist : MonoBehaviour
{
    GameObject friendsList => gameObject;

    public Transform prefabParent;
    public GameObject friendPlatePrefab;
    public Vector2 offScreenPos;
    public Vector2 onScreenPos;

    bool isOpen = false;

    public void OpenCloseList()
    {
        if(!isOpen)
        {
            LeanTween.scale(friendsList, onScreenPos, 0.1f);
            MsfTimer.WaitForSeconds(0.2f, () =>
            {
                isOpen = true;
                return;
            });            
        }            

        if (isOpen)
        {
            LeanTween.scale(friendsList, offScreenPos, 0.1f);
            MsfTimer.WaitForSeconds(0.2f, () =>
            {
                isOpen = false;
                return;
            });
        }            
    }

    public void AddFriendPlate()
    {
        Instantiate(friendPlatePrefab, prefabParent);
    }
}
