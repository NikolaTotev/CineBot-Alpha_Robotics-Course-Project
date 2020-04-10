using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Navigator : MonoBehaviour
{
    public enum RobotComponents
    {
        Base, RotationBase, ArmSection, RotationJoint, GimbalAssembly
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenProp()
    {

    }
    public void OpenPropertyPanel(int enumPosition)
    {
        RobotComponents callerType = (RobotComponents) enumPosition;
        switch (callerType)
        {
            case RobotComponents.Base:
                break;
            case RobotComponents.RotationBase:
                break;
            case RobotComponents.ArmSection:
                break;
            case RobotComponents.RotationJoint:
                break;
            case RobotComponents.GimbalAssembly:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(callerType), callerType, null);
        }
    }
}
