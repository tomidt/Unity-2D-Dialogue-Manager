using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAction : MonoBehaviour
{
    public GameObject obj;

    public void ActionToPerform()
    {
        Debug.Log("Event fired!");
        obj.SetActive(true);
    }
}
