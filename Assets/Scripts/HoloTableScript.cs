using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloTableScript : MonoBehaviour
{
    [SerializeField] HoloTrackWand holoTrackWand_0 = null;
    [SerializeField] HoloTrackWand holoTrackWand_1 = null;
    [SerializeField] GameObject LTBuilding = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        IsWandPointingAtCollider();
    }

    bool IsWandPointingAtCollider(/*Collider hitbox*/)
    {
        Ray tempRay = holoTrackWand_0.GetRay();

        RaycastHit hit;

        if (Physics.Raycast(tempRay.origin, tempRay.direction, out hit, Mathf.Infinity))
        {
            
        }

        Debug.DrawRay(tempRay.origin, tempRay.direction * 10000f, Color.yellow);

        return false;
    }

}
