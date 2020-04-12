using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConfigElementBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Action MouseEnterTooltip = null;
    public Action MouseExitTooltip = null;

    public Action MouseEnter = null;
    public Action MouseExit = null;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseEnter?.Invoke();
        MouseEnterTooltip?.Invoke();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        MouseExit?.Invoke();
        MouseExitTooltip?.Invoke();
    }
}
