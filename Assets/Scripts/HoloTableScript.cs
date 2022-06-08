using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloTableScript : MonoBehaviour
{
    [SerializeField] HoloTrackWand holoTrackWand_0 = null;
    [SerializeField] HoloTrackWand holoTrackWand_1 = null;
    [SerializeField] GameObject MainBuilding = null;
    [SerializeField] GameObject Window = null;
    [SerializeField] GameObject Car = null;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(IsWandPointingAtCollider());
    }

    bool IsWandPointingAtCollider()
    {
        if(HoloTableRayCast(holoTrackWand_0))
            return true;

        if (HoloTableRayCast(holoTrackWand_1))
            return true;

        return false;
    }

    bool HoloTableRayCast(HoloTrackWand tempHoloTrackWand)
    {
        Ray tempRay = tempHoloTrackWand.GetRay();

        RaycastHit hit;
        if (Physics.Raycast(tempRay.origin, tempRay.direction, out hit, Mathf.Infinity) && tempHoloTrackWand.IsButtonAPressed())
        {
            switch (hit.collider.tag)
            {
                case "Building":
                    //MainBuilding do stuff;
                    Debug.Log("Building hit");
                    break;

                case "Window":
                    //Window do stuff;
                    Debug.Log("Window hit");
                    break;

                case "Car":
                    //Car do stuff;
                    Debug.Log("Car hit");
                    break;
            }

            return true;
        }

        Debug.DrawRay(tempRay.origin, tempRay.direction * 10000f, Color.yellow);

        return false;
    }
}