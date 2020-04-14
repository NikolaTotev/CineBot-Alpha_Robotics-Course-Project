using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GizmoDrawer : MonoBehaviour
{
    public Transform TargetObject;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, TargetObject.localPosition, Color.red);
    }
}
