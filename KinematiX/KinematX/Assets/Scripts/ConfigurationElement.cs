using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Element", menuName = "Element")]
public class ConfigurationElement : ScriptableObject
{
    public UI_Navigator.ElementTypes elementType;
    public string ElementName;
    public ConfigurationElement Parent;
    public ConfigurationElement Child;

    public virtual void Print()
    {

    }
    

}
