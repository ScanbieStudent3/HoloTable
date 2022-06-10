using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum State
{
    Building,
    Room, 
    Window,
    Car
}

public class HoloTableScript : MonoBehaviour
{
    [SerializeField] HoloTrackWand holoTrackWand_0 = null;
    [SerializeField] HoloTrackWand holoTrackWand_1 = null;
    [SerializeField] GameObject MainBuilding;
    [SerializeField] GameObject Window;
    [SerializeField] GameObject Car;
    [SerializeField] GameObject CarFrame;
    [SerializeField] Animator animationController_Building;
    [SerializeField] Animator animationController_Car;
    [SerializeField] Animator animationController_Window;
    [SerializeField] GameObject Room;
    State state = State.Building;

    // Start is called before the first frame update
    void Start()
    {
        Room.SetActive(true);
        Window.SetActive(false);
        CarFrame.SetActive(true);
        Car.SetActive(false);

        animationController_Building.SetBool("StartZoomOutToRoom", false);
        animationController_Building.SetBool("StartZoomToRoom", false);
        animationController_Car.SetBool("Loop", false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a") || Input.GetKeyDown("c") || holoTrackWand_0.IsButtonBPressed() || holoTrackWand_1.IsButtonBPressed() || holoTrackWand_0.IsButtonAPressed() || holoTrackWand_1.IsButtonAPressed())
        {
            switch(state)
            {
                case State.Building:
                    Window.SetActive(false);
                    Room.SetActive(true);
                    CarFrame.SetActive(true);
                    Car.SetActive(false);

                    animationController_Building.SetBool("StartZoomOutToRoom", false);
                    animationController_Building.SetBool("StartZoomToRoom", true);

                    animationController_Window.SetBool("WindowAnimation", false);
                    animationController_Car.SetBool("Loop", false);

                    state = State.Room;

                    break;

                case State.Room:
                    Window.SetActive(true);
                    Room.SetActive(false);
                    CarFrame.SetActive(false);
                    Car.SetActive(false);


                    animationController_Building.SetBool("StartZoomOutToRoom", false);
                    animationController_Building.SetBool("StartZoomToRoom", false);

                    animationController_Window.SetBool("WindowAnimation", true);
                    animationController_Car.SetBool("Loop", false);

                    state = State.Window;

                    break;

                case State.Window:
                    Window.SetActive(false);
                    Room.SetActive(false);
                    CarFrame.SetActive(false);
                    Car.SetActive(true);


                    animationController_Building.SetBool("StartZoomOutToRoom", false);
                    animationController_Building.SetBool("StartZoomToRoom", false);

                    animationController_Window.SetBool("WindowAnimation", false);
                    animationController_Car.SetBool("Loop", true);

                    state = State.Car;

                    break;


                case State.Car:
                    Window.SetActive(false);
                    Room.SetActive(true);
                    CarFrame.SetActive(true);
                    Car.SetActive(false);

                    animationController_Building.SetBool("StartZoomToRoom", true);
                    animationController_Building.SetBool("StartZoomOutToRoom", false);

                    animationController_Car.SetBool("Loop", false);
                    animationController_Window.SetBool("WindowAnimation", false);

                    state = State.Building;

                    break;
            }
        }

        if (holoTrackWand_0.IsButtonBPressed() || holoTrackWand_1.IsButtonBPressed() || Input.GetKeyDown("b"))
        {
            if (animationController_Car.GetBool("Loop"))
            {
                Window.SetActive(true);
                Room.SetActive(true);
                CarFrame.SetActive(true);
                Car.SetActive(false);

                animationController_Building.SetBool("StartZoomOutToRoom", false);
                animationController_Building.SetBool("StartZoomToRoom", true);

                animationController_Car.SetBool("Loop", false);
                animationController_Building.SetBool("StartZoomToRoom", false);
            }
            else if (animationController_Building.GetBool("WindowAnimation"))
            {
                Window.SetActive(true);
                Room.SetActive(true);
                CarFrame.SetActive(true);
                Car.SetActive(false);

                animationController_Building.SetBool("StartZoomOutToRoom", false);
                animationController_Building.SetBool("StartZoomToRoom", true);

                animationController_Car.SetBool("Loop", false);
            }
            else if (animationController_Window.GetBool("StartZoomToRoom"))
            {
                Window.SetActive(true);
                Room.SetActive(true);
                CarFrame.SetActive(true);
                Car.SetActive(false);

                animationController_Building.SetBool("StartZoomOutToRoom", true);
                animationController_Building.SetBool("StartZoomToRoom", false);

                animationController_Car.SetBool("Loop", false);

            }
        }

        IsWandPointingAtCollider();
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
        if (Physics.Raycast(tempRay.origin, tempRay.direction, out hit, Mathf.Infinity) && (tempHoloTrackWand.IsButtonAPressed() || tempHoloTrackWand.IsButtonBPressed()))
        {
            switch (hit.collider.tag)
            {
                case "Room":
                    Window.SetActive(true);
                    Room.SetActive(true);
                    CarFrame.SetActive(true);
                    Car.SetActive(false);

                    animationController_Building.SetBool("StartZoomOutToRoom", false);
                    animationController_Building.SetBool("StartZoomToRoom", true);

                    animationController_Window.SetBool("WindowAnimation", false);
                    animationController_Car.SetBool("Loop", false);

                    break;

                case "Window":
                    Window.SetActive(true);
                    Room.SetActive(false);
                    CarFrame.SetActive(false);
                    Car.SetActive(false);


                    animationController_Building.SetBool("StartZoomOutToRoom", false);
                    animationController_Building.SetBool("StartZoomToRoom", false);

                    animationController_Window.SetBool("WindowAnimation", true);
                    animationController_Car.SetBool("Loop", false);

                    break;

                case "CarFrame":
                    Window.SetActive(false);
                    Room.SetActive(false);
                    CarFrame.SetActive(false);
                    Car.SetActive(true);


                    animationController_Building.SetBool("StartZoomOutToRoom", false);
                    animationController_Building.SetBool("StartZoomToRoom", false);

                    animationController_Window.SetBool("WindowAnimation", false);
                    animationController_Car.SetBool("Loop", true);

                    break;
            }

            return true;
        }

        return false;
    }
}