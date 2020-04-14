using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationManager : MonoBehaviour
{
  // Start is called before the first frame update
    void Start()
    {
        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Robot_Base").GetComponent<ConfigElementBehavior>().MouseEnterTooltip = () => ToolTipManager.Show("Robot Base (Static)");
        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Robot_Base").GetComponent<ConfigElementBehavior>().MouseExitTooltip = () => ToolTipManager.Hide();

        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Arm_Section_A").GetComponent<ConfigElementBehavior>().MouseEnterTooltip = () => ToolTipManager.Show("Arm Section A");
        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Arm_Section_A").GetComponent<ConfigElementBehavior>().MouseExitTooltip = () => ToolTipManager.Hide();

        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Arm_Section_B").GetComponent<ConfigElementBehavior>().MouseEnterTooltip = () => ToolTipManager.Show("Arm Section B");
        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Arm_Section_B").GetComponent<ConfigElementBehavior>().MouseExitTooltip = () => ToolTipManager.Hide();

        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Rotation_Joint_1").GetComponent<ConfigElementBehavior>().MouseEnterTooltip = () => ToolTipManager.Show("Joint 1 (Rotation)");
        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Rotation_Joint_1").GetComponent<ConfigElementBehavior>().MouseExitTooltip = () => ToolTipManager.Hide();

        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Gimbal_Assembly").GetComponent<ConfigElementBehavior>().MouseEnterTooltip = () => ToolTipManager.Show("Gimbal Assembl");
        transform.Find("/Canvas/P_RobotConfiguration/Robot_Model/Gimbal_Assembly").GetComponent<ConfigElementBehavior>().MouseExitTooltip = () => ToolTipManager.Hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
