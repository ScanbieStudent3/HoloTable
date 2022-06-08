using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloTableScript : MonoBehaviour
{
    [SerializeField] HoloTrackWand holoTrackWand_0 = null;
    [SerializeField] HoloTrackWand holoTrackWand_1 = null;
    [SerializeField] GameObject MainBuilding;
    [SerializeField] GameObject Window;
    [SerializeField] GameObject Car;
    [SerializeField] Animator animationController;
   


    // Start is called before the first frame update
    void Start()
    {
        Car.SetActive(true);
        Window.SetActive(true);
        animationController.SetBool("StartZoomToRoom", true);
        Debug.Log(animationController.GetBool("ZoomToRoom"));
        Debug.Log("Zzooom");
        //animateRoomZoom.Play("ZoomToRoom");
        //animatorRoomZoom.Play("ZoomToRoom");

    }

    // Update is called once per frame
    void Update()
    {
        //animationController.SetBool("startZoomToRoom", true);
        if (IsWandPointingAtCollider());
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
        if (Physics.Raycast(tempRay.origin, tempRay.direction, out hit, Mathf.Infinity) /*&& tempHoloTrackWand.IsButtonAPressed()*/)
        {
            switch (hit.collider.tag)
            {
                case "Building":
                    //MainBuilding do stuff;
                    Debug.Log("Building case triggered");
                    //animatorRoomZoom.Play("ZoomToRoom");
                    Car.SetActive(true);
                    Window.SetActive(true);
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