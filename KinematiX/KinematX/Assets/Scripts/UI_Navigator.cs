using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Navigator : MonoBehaviour
{
    public enum ElementTypes
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
        ElementTypes callerType = (ElementTypes) enumPosition;
        switch (callerType)
        {
            case ElementTypes.Base:
                break;
            case ElementTypes.RotationBase:
                break;
            case ElementTypes.ArmSection:
                break;
            case ElementTypes.RotationJoint:
                break;
            case ElementTypes.GimbalAssembly:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(callerType), callerType, null);
        }
    }
}
