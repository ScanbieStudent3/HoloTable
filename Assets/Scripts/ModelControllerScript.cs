using Leap;
using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ControllMode
{
    Arcade = 0,
    ArcadePinch = 1,
    SinglePinch = 2,
    Ball = 3,
}

public struct HandVariables
{
    public HandVariables(Hand tempHand = null, float minPinchStrength = 0f, float minFistStrength = 0f)
    {
        hand = new Hand();
        palmVelocity = Vector.Zero;
        PinchingPalmVelocity = Vector.Zero;
        speedZ = 0f;
        speedX = 0f;
        isHandPinching = false;
        isHandMakingFist = false;
        isInitialized = false;

        if (hand != null)
            CalculateHandData(hand, minPinchStrength, minFistStrength);
    }

    public void CalculateHandData(Hand tempHand, float minPinchStrength, float minFistStrength)
    {
        hand = tempHand;

        palmVelocity = tempHand.PalmVelocity;
        if (tempHand.PinchStrength > minPinchStrength)
        {
            PinchingPalmVelocity = tempHand.PalmVelocity;
            isHandPinching = true;
        }
        else
            isHandPinching = false;

        speedX = tempHand.PalmVelocity.z;
        speedZ = tempHand.PalmVelocity.x;

        isHandMakingFist = hand.GetFistStrength() > minFistStrength;

        isInitialized = true;
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }

    public Hand hand;
    public Vector palmVelocity;
    public Vector PinchingPalmVelocity;
    public float speedZ;
    public float speedX;
    public bool isHandPinching;
    public bool isHandMakingFist;
    private bool isInitialized;
}

public class ModelControllerScript : MonoBehaviour
{
    [SerializeField] GameObject[] _GameObjects;
    [SerializeField] GameObject _HandModels;
    GameObject _ActiveGameObject;

    [Header("Controls")]
    [SerializeField] ControllMode _ControllMode = ControllMode.SinglePinch;
    [SerializeField] float _PinchModeCooldown = 1f;
    [SerializeField] float _MinPinchStrength = 0.8f;
    [SerializeField] float _MinFistStrength = 0.8f;

    [Header("Swap object")]
    [SerializeField] float _MinVelocityNext = 3f;
    [SerializeField] float _NextCooldown = 1f;

    [Header("Rotation")]
    [SerializeField] float _RotateMultiplier = 2f;
    [SerializeField] float _MinVelocityRotate = 0.1f;

    [Header("Zoom")]
    [SerializeField] float _ZoomMultiplier = 1f;
    [SerializeField] float _MinZoom = 0.25f;
    [SerializeField] float _MaxZoom = 2.5f;

    [Header("Idle")]
    [SerializeField] float _IdleRotationSpeed = 0.1f;
    [SerializeField] float _IdleRotationDelay= 3f;

    float _CurrentNextCooldown = 0f;
    float _CurrentPinchModeCooldown = 0f;
    float _PalmDistanceLastUpdate = 0f;
    float _PinchDistanceLastUpdate = 0f;
    float _LeftThumbFingersAngleLastUpdate = 0f;
    float _RightThumbFingersAngleLastUpdate = 0f;
    float _CurrentIdleDelay = 0f;

    int _ActiveGameObjectIndex = 0;

    bool _PinchModeEnabled = false;
    bool _WasPinchingLastUpdate = false;
    bool _IsIdle = false;

    // Start is called before the first frame update
    void Start()
    {
        if (_HandModels == null)
            Debug.LogError("Hand Models gameobject not found!");

        for (int i = 0; i < _GameObjects.Length; i++)
        {
            if (_GameObjects[i] == null)
                Debug.LogError("GameObject " + i + " is invaled!");
            else
                _GameObjects[i].SetActive(false);
        }

        if (_GameObjects.Length > 0)
        {
            _ActiveGameObject = _GameObjects[0];
            _ActiveGameObject.SetActive(true);
        }

        _CurrentNextCooldown = _NextCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (_CurrentNextCooldown >= 0f)
            _CurrentNextCooldown -= Time.deltaTime;

        if (_CurrentPinchModeCooldown >= 0f)
            _CurrentPinchModeCooldown -= Time.deltaTime;

        if (_CurrentIdleDelay >= 0f)
            _CurrentIdleDelay -= Time.deltaTime;

        if (Input.GetButtonDown("Submit"))
            SwapMode();

        CapsuleHand[] temp = _HandModels.GetComponentsInChildren<CapsuleHand>();
        if (temp != null)
        {
            //TrackFingerAim(temp);
            Track(temp);
            if (_CurrentIdleDelay <= 0f)
            {
                if(!_IsIdle)
                {
                    _IsIdle = true;
                    ResetCurrentObject();
                }
                RotateCurrentObject(Vector3.up, -_IdleRotationSpeed);
            }
        }
    }

    //Swap control mode to the next mode
    void SwapMode()
    {
        if (((int)_ControllMode) < Enum.GetNames(typeof(ControllMode)).Length - 1)
            _ControllMode++;
        else
            _ControllMode = 0;
    }

    //Change _ActiveObject to the next object from _GameObjects
    void NextObject()
    {
        if (_CurrentNextCooldown > 0 || _GameObjects.Length <= 0)
            return;

        if (_ActiveGameObject != null)
            _ActiveGameObject.SetActive(false);

        if (_ActiveGameObjectIndex + 1 >= _GameObjects.Length)
            _ActiveGameObjectIndex = 0;
        else
            _ActiveGameObjectIndex += 1;

        _ActiveGameObject = _GameObjects[_ActiveGameObjectIndex];
        _ActiveGameObject.SetActive(true);

        _CurrentNextCooldown += _NextCooldown;
    }

    //Rotate _ActiveObject over the given local axis
    void RotateCurrentObject(Vector3 axis, float velocity, float individualRotationMultiplier = 1f)
    {
        if (_ActiveGameObject != null)
            _ActiveGameObject.transform.Rotate(axis, velocity * individualRotationMultiplier * _RotateMultiplier);
    } 

    //Resets the rotation and scale of the _ActiveObject
    void ResetCurrentObject()
    {
        if (_ActiveGameObject)
        {
            _ActiveGameObject.transform.localScale = Vector3.one;
            _ActiveGameObject.transform.rotation = Quaternion.identity;
        }
    }

    //Proof of concept code can later be adjusted to create IsPointingAt
    void TrackFingerAim(CapsuleHand[] temp)
    {
        Finger tempFinger = null;
        for (int i = 0; i < temp.Length; i++)
        {
            if (temp[i] != null)
                tempFinger = temp[i].GetLeapHand().Fingers[1];

            if (tempFinger == null)
                continue;

            Color tempColor = new Color(0.5f, 0.5f, 0.5f);
            Debug.DrawLine(tempFinger.Direction.ToVector3(), new Vector3(0, 5, 0), tempColor);
        }
    }

    //Check if both hands are pinching
    bool AreBothHandsPinching(HandVariables handVariablesL, HandVariables handVariablesR)
    {
        if (handVariablesL.isHandPinching && handVariablesR.isHandPinching)
            return true;
        else
        {
            _WasPinchingLastUpdate = false;
            _PinchDistanceLastUpdate = 0f;
            return false;
        }
    }

    //Zoom _ActiveObject based on the difference between _PinchDistanceLastUpdate and the current distance between hands
    void PinchZoom(HandVariables handVariablesL, HandVariables handVariablesR)
    {
        if (!_WasPinchingLastUpdate)
        {
            _WasPinchingLastUpdate = true;
            _PinchDistanceLastUpdate = CalculatePalmDisctance(handVariablesL, handVariablesR);
        }
        else
        {
            float tempPinchDistance = _PinchDistanceLastUpdate;
            _PinchDistanceLastUpdate = CalculatePalmDisctance(handVariablesL, handVariablesR);
            Zoom(_PinchDistanceLastUpdate - tempPinchDistance);
        }
    }

    //Alternative way to zoom _ActiveObject based on the distance between the hands(without having to pinch)
    void AlternativeZoom(HandVariables handVariablesL, HandVariables handVariablesR)
    {
        if (!handVariablesL.IsInitialized() || !handVariablesR.IsInitialized())
            return;

        float palmDistance = (handVariablesL.hand.StabilizedPalmPosition - handVariablesR.hand.StabilizedPalmPosition).Magnitude;
        float palmDifference = _PalmDistanceLastUpdate - palmDistance;

        if(_PalmDistanceLastUpdate !=0 && palmDifference != 0)
            Zoom(-palmDifference);

        _PalmDistanceLastUpdate = palmDistance;
    }

    //Zoom _ActiveObject by scaling it based on the parameter amount
    void Zoom(float amount)
    {
        if (!_ActiveGameObject)
            return;

        Vector3 newScale = Vector3.zero;
        newScale.x = _ActiveGameObject.transform.localScale.x + (amount * _ZoomMultiplier);
        newScale.y = _ActiveGameObject.transform.localScale.y + (amount * _ZoomMultiplier);
        newScale.z = _ActiveGameObject.transform.localScale.z + (amount * _ZoomMultiplier);

        if (newScale.x < _MinZoom || newScale.y < _MinZoom || newScale.z < _MinZoom)
            newScale = new Vector3(_MinZoom, _MinZoom, _MinZoom);

        if (newScale.x > _MaxZoom || newScale.y > _MaxZoom || newScale.z > _MaxZoom)
            newScale = new Vector3(_MaxZoom, _MaxZoom, _MaxZoom);

        _ActiveGameObject.transform.localScale = newScale;
    }

    //Return the distance between the palms of both hands
    float CalculatePalmDisctance(HandVariables handVariablesL, HandVariables handVariablesR)
    {
        Vector tempVector = handVariablesL.hand.StabilizedPalmPosition - handVariablesR.hand.StabilizedPalmPosition;
        return Mathf.Abs(tempVector.Magnitude);
    }

    //Create and initialize handvariables and call correct tracking mode based on _ControllMode
    void Track(CapsuleHand[] temp)
    {
        CapsuleHand capsuleHandScriptL = null;
        CapsuleHand capsuleHandScriptR = null;

        if (temp.Length >= 1)
            capsuleHandScriptL = temp[0];

        if (temp.Length >= 2)
            capsuleHandScriptR = temp[1];

        HandVariables handVariablesL = new HandVariables();
        HandVariables handVariablesR = new HandVariables();

        if (capsuleHandScriptL != null)
        {
            Hand hand = capsuleHandScriptL.GetLeapHand();
            if (hand != null)
            {
                handVariablesL.CalculateHandData(hand, _MinPinchStrength, _MinFistStrength);
                _CurrentIdleDelay = _IdleRotationDelay;
                _IsIdle = false;
            }
        }

        if (capsuleHandScriptR != null)
        {
            Hand hand = capsuleHandScriptR.GetLeapHand();
            if (hand != null)
            {
                handVariablesR.CalculateHandData(hand, _MinPinchStrength, _MinFistStrength);
                _CurrentIdleDelay = _IdleRotationDelay;
                _IsIdle = false;
            }
        }

        switch (_ControllMode)
        {
            case ControllMode.Arcade:
                TrackSwipeArcade(handVariablesL, handVariablesR);
                break;
            case ControllMode.SinglePinch:
                TrackSwipeSinglePinch(handVariablesL, handVariablesR);
                break;
            case ControllMode.ArcadePinch:
                TrackSwipeArcadePinch(handVariablesL, handVariablesR);
                break;
            case ControllMode.Ball:
                TrackBall(handVariablesL, handVariablesR);
                break;
        }
    }

    void TrackSwipeArcade(HandVariables handVariablesL, HandVariables handVariablesR)
    {
        float combinedPalmVelocity = (handVariablesL.palmVelocity.Magnitude + handVariablesR.palmVelocity.Magnitude);

        if (AreBothHandsPinching(handVariablesL, handVariablesR))
            PinchZoom(handVariablesL, handVariablesR);
        else if (combinedPalmVelocity > _MinVelocityNext)
            NextObject();
        else if (combinedPalmVelocity > _MinVelocityRotate)
            RotateCurrentObject(Vector3.up, combinedPalmVelocity);
    }

    void TrackSwipeSinglePinch(HandVariables handVariablesL, HandVariables handVariablesR)
    {
        if (AreBothHandsPinching(handVariablesL, handVariablesR))
        {
            PinchZoom(handVariablesL, handVariablesR);
            return;
        }

        float combinedPalmVelocity = 0f;
        float combinedSpeedX = 0f;
        float combinedSpeedZ = 0f;

        if (handVariablesL.isHandPinching)
        {
            combinedPalmVelocity += handVariablesL.palmVelocity.Magnitude;
            combinedSpeedX += handVariablesL.speedX;
            combinedSpeedZ += handVariablesL.speedZ;
        }
        if (handVariablesR.isHandPinching)
        {
            combinedPalmVelocity += handVariablesR.palmVelocity.Magnitude;
            combinedSpeedX += handVariablesR.speedX;
            combinedSpeedZ += handVariablesR.speedZ;
        }

        if (combinedPalmVelocity > _MinVelocityNext)
            NextObject();

        if (Mathf.Abs(combinedSpeedX) > _MinVelocityRotate)
            RotateCurrentObject(Vector3.right, combinedSpeedX);

        if (Mathf.Abs(combinedSpeedZ) > _MinVelocityRotate)
            RotateCurrentObject(Vector3.up, combinedSpeedZ);
    }

    void TrackSwipeArcadePinch(HandVariables handVariablesL, HandVariables handVariablesR)
    {
        float combinedPalmVelocity = (handVariablesL.palmVelocity.Magnitude + handVariablesR.palmVelocity.Magnitude);
        
        if (AreBothHandsPinching(handVariablesL, handVariablesR))
            PinchZoom(handVariablesL, handVariablesR);
        else if (handVariablesL.isHandPinching || handVariablesR.isHandPinching)
            PinchModeSwap();
        else if (combinedPalmVelocity > _MinVelocityNext)
            NextObject();
        else if (_PinchModeEnabled && combinedPalmVelocity > _MinVelocityRotate)
            RotateCurrentObject(Vector3.right, combinedPalmVelocity);
        else if (!_PinchModeEnabled && combinedPalmVelocity > _MinVelocityRotate)
            RotateCurrentObject(Vector3.up, combinedPalmVelocity);
    }
    
    void PinchModeSwap()
    {
        if (_CurrentPinchModeCooldown > 0)
            return;

        _PinchModeEnabled = !_PinchModeEnabled;

        _CurrentPinchModeCooldown = _PinchModeCooldown;
    }

    void TrackBall(HandVariables handVariablesL, HandVariables handVariablesR)
    {

        if (AreBothHandsPinching(handVariablesL, handVariablesR))
            PinchZoom(handVariablesL, handVariablesR);
        else if (!handVariablesL.IsInitialized() || !handVariablesR.IsInitialized() || handVariablesL.isHandMakingFist || handVariablesR.isHandMakingFist)
        {
            _PalmDistanceLastUpdate = 0f;
            _LeftThumbFingersAngleLastUpdate = 0f;
            _RightThumbFingersAngleLastUpdate = 0f;
            return;
        }

        float amountOfRotation = 0f;
        Vector tempRotationAxis = handVariablesL.hand.StabilizedPalmPosition - handVariablesR.hand.StabilizedPalmPosition;
        Vector3 rotationAxis = new Vector3(tempRotationAxis.x, tempRotationAxis.y, tempRotationAxis.z);

        float angleL = AngleBetweenVector2(Vector3.up, handVariablesL.hand.DistalAxis());
        float angleR = AngleBetweenVector2(Vector3.up, handVariablesR.hand.DistalAxis());

        if (_LeftThumbFingersAngleLastUpdate != 0f)
            amountOfRotation += _LeftThumbFingersAngleLastUpdate - angleL;

        if (_RightThumbFingersAngleLastUpdate != 0f)
            amountOfRotation += _RightThumbFingersAngleLastUpdate - angleR;

        RotateCurrentObject(rotationAxis, -amountOfRotation, 0.25f);

        _LeftThumbFingersAngleLastUpdate = angleL;
        _RightThumbFingersAngleLastUpdate = angleR;
    }

    float AngleBetweenVector2(Vector3 vec1, Vector3 vec2)
    {
        //Vector3 diference = vec2 - vec1;
        //float sign = (vec2.x < vec1.x) ? -1.0f : 1.0f;
        //return Vector3.Angle(Vector3.right, diference) * sign;
        return Vector3.Angle(vec1, vec2);
    }

    ////This returns the angle in radians
    //public static float AngleInRad(Vector3 vec1, Vector3 vec2)
    //{
    //    return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
    //}

    ////This returns the angle in degrees
    //public static float AngleInDeg(Vector3 vec1, Vector3 vec2)
    //{
    //    return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
    //}
}