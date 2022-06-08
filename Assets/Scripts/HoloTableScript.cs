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
    [SerializeField] GameObject CarFrame;
    [SerializeField] Animator animationController;
    [SerializeField] GameObject Room;
   
    // Start is called before the first frame update
    void Start()
    {
       
        //animateRoomZoom.Play("ZoomToRoom");
        //animatorRoomZoom.Play("ZoomToRoom");

    }

    // Update is called once per frame
    void Update()
    {
        //animationController.SetBool("startZoomToRoom", true);
        if (holoTrackWand_0.IsButtonBPressed() || holoTrackWand_1.IsButtonBPressed() || Input.GetKeyDown("a"))
        {
            animationController.SetBool("StartZoomOutToRoom", true);
        }
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
                case "Room":
                    //MainBuilding do stuff;
                    Debug.Log("Building hit");

                    animationController.SetBool("StartZoomToRoom", true);
                    Window.SetActive(true);
                    Room.SetActive(true);
                    CarFrame.SetActive(true);
                    Car.SetActive(false);

                    break;

                case "Window":
                    //Window do stuff;
                    Debug.Log("Window hit");

                    Window.SetActive(true);
                    Room.SetActive(false);
                    CarFrame.SetActive(false);
                    Car.SetActive(false);

                    //zoomanimation
                    break;

                case "CarFrame":
                    //Car do stuff;

                    Window.SetActive(false);
                    Room.SetActive(false);
                    CarFrame.SetActive(false);
                    Car.SetActive(true);
                    //car animation zoom 
                    //car driving

                    Debug.Log("Car hit");
                    break;
            }

            return true;
        }

        Debug.DrawRay(tempRay.origin, tempRay.direction * 10000f, Color.yellow);

        return false;
    }
}