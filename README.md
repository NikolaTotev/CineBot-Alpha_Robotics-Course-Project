

# Project Cinebot Alpha

This is the documentation for my Robotics Course Project created during my second year, second semester in FMI for the course Practical Robotics and Smart "Things".

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/Photos/Robot_Full.jpg?raw=true" width="562.66" height="660.16" alt="Robot_Full"/>
</p>

## *Acknowledgements*
Before I begin, I would like to acknowledge my parents, for their constant love and encouragement, not only for this project but for all of my previous and current endeavors. Without them, this project would not be the same.
I would also like to thank the teachers of the teachers that guided me
through the skills I needed to learn to make this project a reality. In no particular order I'd like to thank:

* Dr. Ivan Chavdarov, for teaching me the skills related to 3D modeling & printing, as well as giving me guidance on my structural design by telling me what could be improved and how such things are usually made/designed.

* Trayan Iliev, for expanding my horizons on the subjects of IoT and the software side of robotics.

# Contents
-  [Introduction](#Introduction)
	* [Scope](#Scope)
	* [Key features](#Key-features)
	* [Initial design decisions](#Initial-design-decisions)
	* [Interesting Statistics](#Project-Statistics)
	
* [Getting started ](#Getting-started)   *(click here if you just want to setup the robot and learn how to use it)*
	* [How to run client](#How-to-run-client)
	* [How to run  server (robot)](#How-to-run-the-robot)
		* [Publishing from Visual Studio](#Publishing-from-Visual-Studio)
		* [Uploading via WinSCP](#Uploading-via-WinSCP)
		* [Running Server](#Running-server)
	* [Trouble shooting](#Troubleshooting)

-  [Hardware](#Hardware)
	* [Design constraints](#Design-constraints)
		* [Actuators](#Actuators)
		* [Method of manufacturing](#Method-of-manufacturing)
			* [Manufacturing capabilities](#Manufacturing-capabilities)
			* [Material choice](#Material-choice)
		* [Assembly methods](#Assembly-methods)

	* [Hardware design decisions](#Hardware-design-decisions) 	
		*	[Structure overview](#Structure-overview) (part evolution and motivation behind the design)
			*	[Base & base drive train](#Base-and-base-drive-train)
			* [Arm section](#Arm-section)
			* [Arm joint](#Arm-Joint) 
			* [Counter balance](#Counter-balance)
			* [Gimbal](#Gimbal)
			* [Limit switches](#Limit-switches)
			* [Electronics housing](#Electronics-housing) ***(design aspects)***
			
		* [Electrical system](#Electrical-system)
			* [Schematics](#Schematics)
			* [Used components](#Used-components)
			* [Electronics housing and connections to robot](#Electronics-housing-and-connections-to-robot)
					
-  [Software](#Software)
	- [Software Requirements](#Software-Requirements)
		- [Safety requirements](#Safety-requirements)
		- [Required modes of operation](#Required-modes-of-operation)

	- [Software Architecture](#Software-Architecture)
		- [Unix Domain Sockets](#Unix-Domain-Sockets)
		- [Using TCP](#Using-TCP)
 	- [Choosing a suitable platform](#Choosing-a-suitable-platform)
	- [Programing language selection](#Programing-language-selection)
	- [Software motor control](#Software-motor-control)
	- [Diagrams](#Diagrams)
		- [System Modules](#System-Modules)
		- [Deployment Structure](#Deployment-Structure)

---
# ==== Disclaimer ====
This project is for ***educational purposes only***. This is my ***first*** robotics project and there are certainly bad practices/decisions that could lead to injury or damage to components.
If you decide to recreate this please do so carefully. If any issues are discovered please put in an issue so I can fix it.

_Also please note, this project is made by a beginner, I have never assembled, designed or programmed a robot before. When giving feedback or grading this please consider this._

---

# Introduction
## Documentation structure
This project has two main sections hardware & software. The structure of this documentation begins with the hardware aspects of the project because that is the way the development of this project began. After that, I will continue with the software side of this project.

 The goal of this documentation is to inform the reader about the project and give him/her a better understanding of the project, what the motivations are behind the various design decisions and how the many hardware and software components interact with one another to create a complete robot as well. This document will also cover the many mistakes I made along the way, the challenges I faced as a complete beginner, how I overcame them. I also describe the lessons I learned along the way and point out the improvements that can be made if someone attempts to recreate this project.

Since this is my first project and documentation of such a scale I would like the reader to know that there may be some errors and things that can be done better. If you find such a thing, please make an issue in Github so I can get some constructive feedback and so I can improve this project for future readers.

For the reader's convenience, I have added “Back to top” links throughout the documentation, they will lead you to the beginning of the contents page if you wish to read another part of the documentation.

## Scope
Since this is my first robotics project, the scope of this project is fairly limited if compared to the current state of robotics, but quite broad for a complete beginner.  At the start of this project the scope was quite limited, but as the development progressed so did the scope because I discovered new things that would help me improve this project and teach me even more new things. I have more things I’d like to add in the future, I would also be very happy if this could be used as a beginners project for students or just people who want to get into robotics. That is why this will be a 100% open-source project so anyone can contribute.

During the development of this project in no particular order I wanted to:

* Learn about the platforms that are available for robotics and that are suitable for beginners but at the same time have enough room for growth in the future
* Using 3D modeling for visualizing the robot and as a key component in the manufacturing phase.
* Exploring different methods of creating the physical structure of the robot.
* How actuators are made/chosen and how they are controlled
* Learn more about key principles about robotics such as
               * Kinematics
               * How to deal with forces/balancing issues
               * Safety considerations when designing a robot
               * Motion smooth / using the quintic polynomial equation to manage forces on the robot caused by acceleration during movement.
* Learn how to make a basic multithreaded control software for the robot
* Facial tracking using OpenCV and implementing a PID controller to track a face by moving the robot.


## Key features
* __Jog Mode__
This is manual control done via the encoders on the front of the electronics case. This is used for diagnostics and testing. This mode also shows off the multithread stepper motor control.

* __Program Path__
Similar to jog mode when it comes to controlling the robot, but there is an option for entering a node for a given position of the robot (both for gimbal and stepper motors). Once the wanted path is done, it is saved to a file and it can be replayed later using replay path mode.

* __Replay Path__  
Reads instructions from the selected file and replays them. A picture is taken at each node.

* __Face detection & tracking__
This mode is still a bit experimental, but it does detect a face using OpenCV with Haar Cascade. Then with a PID Controller, it tracks a face attempting to keep it centered in the frame. I followed [this tutorial](https://www.pyimagesearch.com/2019/04/01/pan-tilt-face-tracking-with-a-raspberry-pi-and-opencv/), but I had to make a couple of changes to make it work with the C# code. More information can be found in the [Unix Domain Sockets](#Unix-Domain-Sockets) section.


## Initial design decisions
The first choice I had to make was what kind of robot would be suitable for this project. The two main choices where either a mobile robot or a stationary robot arm. I know there are more options but considering my limited knowledge at the beginning I wanted to attempt one of the two types. In the end, I decided to make a stationary robot arm for the following reasons:

1. Work environment - Before and after the pandemic my work environment was limited and setting up an environment for a mobile robot would have impractical. For a stationary robot, it was much easier to designate one small spot for it,
2. Powering the robot - I only knew about lab power supplies and how they are used for such experimental projects, based on the knowledge my only reasonable option was a stationary robot as a mobile one would required batteries to operate freely.

Once I decided what kind of robot I would make the size of the robot was the next decision. Since I have a limited work area and limited resources such as time, money, and manufacturing capabilities I decided to make a fairly small robot arm that doesn't have practical use but is perfect for educational purposes.

Before beginning with the CAD design, I used Legos to create a crude model of the robot arm and the joints it would have and how they would move. I found this to be the best way to make the initial prototype as it cost nothing and gave a good enough visualization/feel.

## Project Statistics
* Work time - about 600-650 hours - aprox. 25-27days
* Print Time - 805h 8min - aprox. 33.5 days
* Total Project - 5.61GB

## Tools & Software
As this project has both software and hardware components I feel that it is necessary to list all of the tools I used during development

Hardware tools
- 3D Printer - Flashforge Creator Pro
- X-ACTO Knives
- Various screwdrivers
- Various pliers
- Clamps
- Soldering Iron
- Digital Caliper
- Wire-stripper

Software tools
- Visual Studio 2019
- Autodesk AutoCAD 2020 (Student Edition)
- Wolfram Mathematica
- Github Desktop
- WinSCP
- .NET Core IoT Libraries
- Flashprint (Slicer)

##### [Back to top](#Contents)

# Getting started
If you intend to build this robot, or even better, have already built it, this section is the one for you! The setup process isn't as straightforward as I would like, but considering the limited time, I think this is pretty good.
The system is divided into two parts
* Client
* Server
The server runs on the Raspberry Pi and the Client *(console version)* runs on any Windows or Linux machine.

## Prerequisites
Here are a couple of things you will need to get started:
* [Visual Studio 2019](https://visualstudio.microsoft.com/vs/community/)
* [WinSCP](https://winscp.net/eng/download.php)
* Basic Linux knowledge & ability to work in a terminal
* Command prompt or an SSH client such as [Putty](https://www.putty.org/) or [Solar Putty](https://www.solarwinds.com/free-tools/solar-putty) if you want a better GUI
* A raspberry pi with the latest version of [Raspbian](https://www.raspberrypi.org/downloads/raspberry-pi-os/) (or as it is now called "Raspberry  Pi OS)*
* [ .NET Core](https://github.com/dotnet/iot) needs to be installed on your raspberry
* You will also need to install [OpenCV](https://qengineering.eu/install-opencv-4.3-on-raspberry-pi-4.html)
#### For the builders
* If you are starting to build this, I suggest looking at the [Fritzing connection schematics](https://github.com/NikolaTotev/Robotics-Course-Project/tree/master/Documentation/Schematics)
* The [parts list](#used-components) is also something useful as well as the [STL Files](https://github.com/NikolaTotev/Robotics-Course-Project/tree/master/STL%20Files) may be useful for 3D printing.

## How to run client
I will describe how to run the client on windows, as that is the platform I have tested it on, but if you use the same steps used for [publishing the server](#publishing-code-from-visual-studio) you will be up and running in no time!

1. Once you have cloned or downloaded the repo, navigate to
` Robotics-Course-Project > Robot Code > Robot_Code`
and locate ` Robot_Code.sln` and launch it in Visual Studio.
2. When everything is loaded:
               * if you want to use the `GUI Client`,  find `WindowsClientGUI` in the *solution explorer* and run it
               * if you want to use the `Console client` locate the ConsoleClient in the *solution explorer*
3. After that just press `Ctrl+F5` to run

***[Helpful tip ]:*** 
*If you are new to Visual Studio, if the solution explorer is not visible you can search for it in the "Search" bar at the top of the IDE*


## How to run the robot

### Publishing from Visual Studio

***1. Locate the `Central_Control` project in the *solution explorer****

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/1_Select_Central.png?raw=true">

***2. Right-click and find the `Publish` option.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/2_Publish_Btn.png?raw=true">

***3. Once you press it, you will see this screen. Select the `Folder` option. Press `Next`***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/3_Initial_Pub_Screen.png?raw=true">

***4. Select the desired publish path. Make sure it is easy to find since you will use it when [uploading via WinSCP](#Uploading-via-WinSCP). Press `Finish`***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/4_Pub_Path_Sel.png?raw=true">

***5. You will now see this screen. Before publishing the configuration needs to be edited. Press the edit button.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/5_Edit_Pub_Config.png?raw=true">

***6. This window will open and here you need to change the `Deployment Mode` and `Target Runtime` if you don't, it will not run on the Raspberry Pi.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/6_Pub_Config_Edits.png?raw=true">

***7. `Deployment Mode` must be set to `Self-Contained`***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/7_Depl_Mode_Sel.png?raw=true">

***8. `Target Mode` must be set to `Linux-arm`***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/8_Targ_RT_Sel.png?raw=true">

***9. The configuration window should look like this once you are done.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/9_Final_Pub_Settings.png?raw=true">

***10. After that, you are ready to publish by pressing the `Publish` button.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/10_Pub_Btn.png?raw=true">

***11. This is how the output should look like if everything has been published correctly.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/Publish%20Instructions/11_Normal_Output.png?raw=true">

##### [Back to top](#Contents)

---
### Uploading via WinSCP
This section will not cover how to setup WinSCP, but it will show the basic upload procedure.

***1. On the RPi create a directory mine is `/home/pi/Documents/Robot_Code/Central_Control` In WinSCP navigate to that directory (the right section of the UI). 
On the left side navigate to the publish directory you set in the [publish guide](#Publishing-from-Visual-Studio).***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/1_WinSCP_StartScreen.png?raw=true">

***2. Select the publish folder and locate the `Upload` button. Then press it.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/Upload_Btn.png?raw=true">

***3. You will see the following screen, just press `Ok`***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/Conf_Window.png?raw=true">

***4. Then you will see this window, I suggest setting the upload speed to `Unlimited` if it isn't already.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/Speed_Setting.png?raw=true">

***5. Once uploaded the UI should look like this.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/5_After_Transfer.png?raw=true">

***6. Now it is time to set the correct permissions. Open the publish folder and locate `Central_Control`***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/6_R_Cl_Central_Ctrl.png?raw=true">

***7. Right-click and press the `Properties` option.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/7_Prop_Sel.png?raw=true">

***8. For simplicity, just check all of these boxes.***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/8_Set_Prop.png?raw=true">

***9. Press Ok and you should be all set to run the server!***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/WinSCP%20Instructions/9_Perms_Set.png?raw=true">

##### [Back to top](#Contents)

---
### Running Server
***1. Login to the Pi via SSH (you can also use a monitor or keyboard, SSH is just a bit more convenient)***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/SSH%20Instructions/1_SSH_Cmd.png?raw=true">


***2. Enter your password***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/SSH%20Instructions/2_SSH_Pass.png?raw=true">

***3. Navigate to the directory containing the program***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/SSH%20Instructions/3_SSH_Logged_In.png?raw=true">

***4. Start the program***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/SSH%20Instructions/4_Run_Server.png?raw=true">

***5. If everything has started correctly, you should see this message:***

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/SSH%20Instructions/5_Server_Started.png?raw=true">

 and you should see the notification LED's blinking like this:

![Alt Text](https://media.giphy.com/media/Sr8YixXuJKgU7s2pBT/giphy.gif)

### Troubleshooting
Some known errors may occur during the starting of either the server or client.
1. Arduino is not plugged in. This error looks like this and can easily be fixed by ensuring the Arduino is plugged in.

<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Getting%20Started%20Images/SSH%20Instructions/ERROR1_Arduino_Not_Connected.png?raw=true">


##### [Back to top](#Contents)

---

# Hardware

## Design constraints
This section covers the design constraints I had during the development of the hardware and how the constraints affected the final design.

### Method of manufacturing
Since this project requires are higher accuracy of the parts and since I had limited time to make each part myself I decided that the best option would be to utilize 3D printing to make all of the parts for the robot. Since I also enrolled in a 3D modeling course in the same semester, the decision to use 3D printing would mean that I could apply the from one course in another.

#### Manufacturing capabilities
The printer I chose for this project as mentioned in the tools section is the Flashforge Creator Pro. Since it was my first printer I decided to go with this model because it has dual extruders and the ability to print in a variety of materials, giving me enough options for the future.

#### Material choice
Because all of the printing would be happening in my room, I decided to go for **PLA** as it does not emit toxic odors and it is significantly easier to print with. The strength of the material is adequate and is suitable for relatively small forces involved.

---

### Actuators
For the primary actuators, I decided to use two NEMA 14 35x28 stepper motors and for the gimbal actuators, I use two micro [servos with metal gears]([https://erelement.com/servos/feetech-ft90m](https://erelement.com/servos/feetech-ft90m)) for the rotate and tilt axes and a [larger servo with plastic gears ](https://www.robotev.com/product_info.php?cPath=1_40_45&products_id=205(https://www.robotev.com/product_info.php?cPath=1_40_45&products_id=205)) for the pan axis.

*Due to lack of knowledge at one stage of the project I had to change the stepper motors from my initial NEMA 14 35x26 to the slightly more powerful 35x28. That said, I believe it is possible to make this project work with the smaller motors but better gearing would be required*

Since these motors lack the holding torque to hold the arm of the robot horizontal or to even lift it, [reducer mechanisms](#Reducer-mechanisms)  had to be implemented

---

### Assembly methods
As I am using 3D printing for making all of the parts I need to take into account the weaknesses and strengths of additive manufacturing to print good parts.
Most of the parts are printed in a way that avoids overhangs as much as possible. Layer orientation is taken into consideration when possible.

To assemble all of the parts I use two main techniques:
- M3 & M2.5 machine screws of various lengths.
- Trapezoid wedges that slide into each other

The combination of these two methods allows for a good balance between the solid assembly of large modules and allows those modules to be connected using a universal attachment interface.

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/CAD%20Models/Universal_Connector.png?raw=true" width="800" height="400" alt="Base_Gearbox_Evolution"/>            
</p>

***Note:*** Initially I used two other methods of assembling the robot:
1. Friction fit connections with a square or hexagonal shape to allow for easy assembly/disassembly.
2. Screws for all of the connections.

Both of these methods had significant downsides. The friction fit relied on tight tolerances, something that is not consistent when 3D printing parts.
For the screw method, once the larger parts were connected, if something needed to be replaced, a significant amount of work needed to be put in to change a single part.

##### [Back to top](#Contents)
## Hardware design decisions
### Structure overview
The structure of the robot is fairly simple. In the following points, I'll describe each part and all of the iterations I had to go through. The general goals for the hardware can be divided into two sections:

* Robot structure
	1. It has to be printable
	2. The parts need to be able to support the expected forces and use as little material as possible
	3. The reducer mechanisms need to move smoothly
	4. As mentioned above assembly needs to be pretty simple
	5. Parts must be modular and easy to swap out
	6. Built-in cable management.
	7. It needs to look good (this has a lower priority, but still an important aspect)
               
* Electronics housing
	1. There must be enough room to work inside and fit the electronics
	2. Cooling needs to be integrated into the case to help with performance
	3. Notification LEDs need to be in a good position for the user
	4. Any controls such as rotary encoders also need to be in an easily accessible spot.
	5. All ports that are used for connection to the robot need to be in the back
	6. As with the robot, it has to look good (again lower priority)

*Some of the things mentioned in the electronics housing are described in more detail in the [electrical system](#Electrical-system) section.*
####  A little bit about iterations
I have decided to go through all of the iterations because I think it is important for me to show the whole process. That way I can describe the things I've learned along the way and it is a great section for beginners to see the mistakes I've made so they don't make them as well.
Since this is quite a lengthy section I will provide skip links so that you can only see the final design or so you can skip directly to the [software part](#Software) of the project.

---

####    Base and base drive train
Since this is a stationary robot, the base is a key part of the design and a good base is essential for the overall stability of the system. This section also experiences the most loading and has to be designed with that in mind. I also want to cover the different adapters that are used to connect the arm sections to the base subassembly.
* ___Base & base drive train iterations___

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/CAD%20Models/Base_Gearbox_Evolution.png?raw=true" width="800" height="400" alt="Base_Gearbox_Evolution"/>            
</p>

* __Iteraton 1__
Initially, I started with this design. As you can see it is a simple and small gearbox and the larger gear has a hexagonal shaft. and it sits on a stand. The idea behind this was to have the rest of the robot be friction fit onto it, however, there were many tolerance issues. Another significant issue with this base, it had a small footprint and once the robot was friction fit ontop  there was a lot of wobbling around and I abandoned the idea.

* __Iteraton 2__
This iteration was inspired by the first version of the arm joint planetary gears and then a smaller version of this base gearbox was used in the final iteration of the arm joint. In theory, this setup should've been much more stable, but the way I chose to attach the base adapter created a lot of problems. This gearbox also had some binding issues in some spots, which resulted in irregular motion and the stepper motor losing steps. The problems mentioned along with the arm section issues at the base of the robot made me rework the entire base.

* __Iteraton 3__
This is the final iteration. As you can see it quite different and uses a much simpler and adjustable drive train. In the base, there are 2 75 mm *(outer diameter)* bearings that provide the structural stability and smooth rotational movement. This base fixes all of the issues that the previous ones had and helped solve the issues with the base arm section

**Thigs I learned** - Its best to keep reducer mechanisms as simple as possible. It is important to consider the forces that will be exerted on a part, especially on the base of the robot. Bearings are something that should be used more often, they significantly improve the final result and are a great addition to 3D printed mechanisms.
               
##### [Back to top](#Contents) 

---          
               
* ___Base Adapter iterations___
   This will just quickly cover the different adapters I made.

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/CAD%20Models/Base_Adapter_Evolution.png?raw=true" width="800" height="400" alt="Base_Gearbox_Evolution"/>            
</p>

* __Iteraton 1__
 Uses hexagonal friction fit on one side and screws for mounting the arm section.
Cons -
	* No cable management
	* Tolerance issues
	* Time-consuming to mount/unmount arm sections
	* Too thin for the loads it would experience

	Pros -
	* simple design
	* quick to print.

*  __Iteraton 2__
               This version as you can see it a bit better, but it still has the major issues of the previous one. 
               
      Pros -
      * fixes thinness issue
      * adds some stability due to wider footprint
      
      Cons
      * still time-consuming to mount arm sections
      * still has tolerance issues
                
* __Iteraton 3__             
               This one solves all of the issues of the previous ones but introduces some new ones. As you can see it is connected with screws to the gearbox, but those screws also hold the planet gears and carrier in alignment. This means that they can't be tightened too much as it will cause the gearbox to bind. Despite those problems, the easy swap functionality that comes from the use of a universal trapezoid mount allows for easy mounting of the other part of the robot. This base also has cable management that feeds the cables out of the back.
               
* __Iteration 4__
The final iteration of this part is removing it altogether. See the last arm iteration to understand the whole idea.

##### [Back to top](#Contents)
---

#### Arm section
This is the most important part of the robot. It makes up the majority of the assembly and without it, the robot would not work. This part also transmits the most load and has to be made both light and strong. Many iterations were needed to get to the final version.

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/CAD%20Models/Arm_Evolution_Diagram.png?raw=true" width="800" height="400" alt="Arm_Evolution"/>               
</p>     

* __Iteraton 1__
This was not thought through very well. As the previous first iterations, it relies on friction fits and too many screws to be practical.

* __Iteraton 2__
 This iteration is also poorly designed. While the shape is cool, it is too thin, too small and again relies on a friction fit.
   
* __Iteraton 3__
This version is a bit better, much stronger but still too small and once again it uses too many screws.

* __Iteraton 4__
This iteration is where better design choices emerge. There is proper cable management, it is light yet strong. Unfortunately, it is too complex and takes too long to print. It also uses too many screws.
                 
* __Iteraton 5__
This is the first iteration where a universal connector is used. The shape is simplified and there is better cable management. This design goes through 2 updates to make it stronger because it experienced a structural failure once. These updates just make the walls thicker.

* __Iteraton 6__
For this iteration, the top arm section is the same, but the base arm section has been reworked due to the structural failure shown below. This change also comes from the redesign of the base drive train. As you can see, it still keeps the universal connector, it is also much more stable, compared to the previous arm sections.

##### [Back to top](#Contents)
---          

#### Arm Joint
This part is the one and only "joint". I call it a joint because it connects two arm sections in a mobile way. This part of the robot had a couple of requirements to meet:
1. It needs to be strong enough (structurally) to lift the weight of the arm.
2. Since the stepper motor can't lift the arm on its own the joint needs to be some kind of reducer with an appropriate gear ratio. The gears must also operate smoothly.
3. It needs to implement a universal mounting system.

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/CAD%20Models/Joint_Evolution.png?raw=true" width="800" height="400" alt="Arm_Joint_Evolution"/>                               
</p>

* __Iteraton 1__
 In the very beginning since I had never worked with stepper motors, I didn't know that they can't lift much on their own, and they require some kind of reduction mechanism.
This is why the first iteration has no gears whatsoever.
                 
 * __Iteraton 2__
This iteration uses a very simple planetary gearbox with a 7:1 reduction ratio. Was able to move the arm, but as it relies on a friction fit it is not stable. Along with those issues, after the gimbal iterations, it proved to be too weak.
                
* __Iteraton 3__
In this version, I decided to try to make a worm drive, but since it is a much harder reducer to make it had internal friction issues that caused it to be too weak.

* __Iteraton 4__
This is the final iteration and it is a smaller version of the second iteration of the base gearbox.  It shares its flaws, but they were not as pronounced due to the smaller forces exerted on it. Using this new gearbox allowed me to finally add the universal mounting bracket to it.

##### [Back to top](#Contents)
----

#### Joint to arm section interfaces/adapters
* __Iteraton 1__
               As you can see there is more than one version, but I'm grouping them as the first iteration as they do not include any counterbalance. These are simple, they all do their job, but due to the weight of the gimbal, a counterbalance is required.
               
* __Iteraton 2__
As you can see in this version, at the back there is a mounting point for the counterbalance mentioned above. This makes the job of the reducer much easier by balancing the forces.

##### [Back to top](#Contents)
---

#### Counter balance
* __Iteraton 1__
This part is pretty simple. Since I needed something to balance the gimbal, and an Arduino is needed to control the motors, I decided to combine the two and this is the result. The back has a tray for some metal weights and on the top, there are mounting holes for the Arduino. As mentioned above it connects to the second iteration of the arm joint adapter

##### [Back to top](#Contents)
---

#### Gimbal
<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/Photos/Gimbal_Full_Left.jpg?raw=true" width="486.33" height="597.66" alt="Gimbal_Full_Left"/>
</p>

This part also has a lot of iterations, as initially, I didn't know what the best approach was. All of the axes are controlled via servo motors.

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/CAD%20Models/Gimbal_Evolution.png?raw=true" width="800" height="400" alt="Gimbal_Evolution"/>
</p>

* __Iteraton 1__
 This looks like a standard gimbal, the rotation motor housing is quite big and it is really heavy. Along with that, the camera mount is weak and the tilt and pan arms are also flimsy.

* __Iteraton 2__
The second version of the gimbal is very complex and rather large. Due to this fact, it is very unstable and I quickly abandoned it.

* __Iteraton 3__
The third version is inspired by the second one but attempts to minimize it by using smaller servos and smaller pieces. This design has major alignment issues. It is also very difficult to assemble and easy to break.

* __Iteraton 4__
This final design is inspired by [a project I found on youtube](https://www.youtube.com/watch?v=wQypj7ti7Vw). I was already behind schedule and I thought that it's time to look for some help. I had to make the models from scratch since I am using servo motors and the person in the video is using brushless gimbal motors.

##### [Back to top](#Contents)
---

#### Limit switches
<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/Photos/Limit_Switch.jpg?raw=true" width="717" height="422" alt="Limit_Switch"/>
</p>

These don't have any iterations. I just bought a simple switch from the store and mounted it to the robot with two simple mounts. Despite their simplicity, these are critical parts as the allow the robot to "know" what position it is in. Please read [the improvements section](#Possible-improvements) for more insight on the improvements I would make in the area of positional awareness.

##### [Back to top](#Contents)
---

#### Electronics housing

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/Photos/Housing_Wide.jpg?raw=true" width="646.16" height="509.66" alt="Electronics_Housing_Wide"/>
</p>

This is a pretty simple part of the whole project. Its main goal is to make the project a bit more complete and polished. It also ensures, that once all of the cables are connected in working that they stay that way. The case also provides active cooling because it has two 95mm fans on one side.

***Possible improvements***
* This housing it a bit large for the number of components inside, but since it was my first attempt at such a thing, I decided to leave enough space to work inside.

* It also tends to tip forward as it isn't well balanced, so some legs or a flatter bottom might be a nice improvement.
* Internal cable management can be improved

* External cable management, such as ports for USB cables, ethernet, and so on. At the moment, all of those cables go directly out through the back opening. Ideally, it would have some sort of panel with the right kind of holes.
##### [Back to top](#Contents)

---

### Electrical system
#### Schematics
The schematics are very useful for anyone trying to make this exact robot. Please make sure to safely handle components and double-check connections before powering on the system. ***NEVER plug or unplug anything from the  RPi or Arduino while they are working. ***

* ___Stepper motor connection schematic___ [_alternate link_](https://github.com/NikolaTotev/Robotics-Course-Project/tree/master/Documentation/Images/Electronic%20Schematics)
<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Electronic%20Schematics/Stepper%20Motor%20Connection%20Schematic.png?raw=true" width="707.5" height="667.5">
</p>

---

* ___Encoder & switch connection schematic___ [_alternate link_](https://github.com/NikolaTotev/Robotics-Course-Project/tree/master/Documentation/Images/Electronic%20Schematics)

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Electronic%20Schematics/Button%20and%20Encoder%20Connections%20Schematic.png?raw=true" width="707.5" height="667.5">
</p>

---

#### Used components
* __Stepper motor & stepper motor drivers__

     * [A4988 Stepper Motor Driver Carrier](https://www.pololu.com/product/1182)
     * [NEMA 14 Stepper motor](https://www.pololu.com/product/1208)
* __Encoders__

     * [Waveshare rotary encoder](https://www.waveshare.com/rotation-sensor.htm)
     
* __Single-board computer & microcontroller__
    * [Raspberry Pi 4 2GB](https://erelement.com/raspberry-pi-4-2gb) is used as the primary computer.
    * [Arduino UNO](https://store.arduino.cc/arduino-uno-rev3) is used for servo control.

---

#### Electronics housing and connections to robot
The electronics housing is not an essential part of the project, but it makes for a more complete and polished project. It also hides the mess of cables and provides an interface for the robot to connect to via standard ports.

**There are 4 ports:**
1.  Stepper motor A
2. Stepper motor B
3. Servo power & limit switches for stepper A
4. I2C connection & limit switches for stepper B _(Unfortunately I2C is never used)_

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/Photos/Robot_Ports.jpg?raw=true" width="50%" height="50%">
</p>

---

### Possible improvements
* Putting everything on one PCB
* Increasing modularity by using even more universal plugs for other components.
* Adding microstepping control to the stepper drivers
* Utilizing the available I2C connection.
##### [Back to top](#Contents)
---                          
# Software
The software is the other big component of this process. It is responsible for the safe control of the robot. It is also the bridge with which the user interacts with the system. This is why I want to begin with some of the requirements that I decided are important for this project.

---
## Software Requirements
### Safety requirements
Since this is a simple robot, with limited functionality, I decided to make safety features as part of the functionality of the robot. Two main safety features are noteworthy:

1. There is a physical emergency stop button that is monitored by processes that control the stepper motors or send commands to the Arduino via the serial port. If the button is switched on those processes immediately stop the movement and exit to a safe program state. This check is performed before any movement code is executed.

2.  There are physical limit switches that limit the motion of the robot and protect it from entering into dangerous positions. These are also monitored by the processes that perform movements. The difference is, that if a limit is hit, the robot can move back into a safe position if such a command is given.

These two safety-critical checks run on a separate thread and for each motor at the same time. All of that said, it is important to consider the [platform](#Choosing-a-suitable-platform) and [programing language](#Programing-language-selection) being used, and the pros and cons associated with those decisions.

##### [Back to top](#Contents)

---

### Required modes of operation
Along with the safety features, the robot has a couple of different modes of operation:
1. Simple jog mode - control one stepper motor at a time.
2. Multithread jog mode - control both stepper motors at the same time. 
3. Gimbal jog mode - control the gimbal - all axes at the same time.
4. Record path - records a path, multithread stepper motor control, and an option to switch to gimbal control. Path nodes are set by pressing the correct buttons for each mode. More info can be found in the [how to use section]()
5. Replay path - Reads a saved file and replays the motion that was recorded.
6. Face detection & tracking - track a detected face by attempting to keep it in the center of the frame  ***(only works for gimbal)***

##### [Back to top](#Contents)

---

### User interface
This is an important category, as it is the way for the user to know what mode the robot is in.
This is done via 3 LED's mounted on the front of the electronics housing. The main way the user can interact with the system with via the client application which has both a GUI and console interface.

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/Photos/Housing_Close.jpg?raw=true" width="50%" height="50%">
</p>

---

## Software Architecture

### Unix Domain Sockets
As mentioned in the [Key Features](#Key-Features) section, there is a mode for facial tracking. The problem is that is written in Python, but all of my other code is in C#. This means that I need some way for the two processes to communicate with each other. Unix Domain Sockets to the rescue! Since everything is a "file" in Unix, I can just create a simple server-client set up to allow the Python program to communicate with the C# cod that controls the servos.

### Using TCP
The robot communicatew with the client via TCP. The robot is the TCP server.

## Choosing a suitable platform
Due to my limited budget and experience, the most reasonable platform was the Raspberry Pi and the Arduino UNO. Considering my fairly light requirements these two devices are more than capable to meet the requirements and perform well.

## Programing language selection
My initial choice was Python, but it was lacking TCP pending functionality that prevents blocking of the main thread while the server awaits a connection. I am sure some options allow for this functionality, but I was not able to get something to work. This issue combined with my lack of Python experience made me look for alternatives.
Since I have previous experience with C# and since the Raspberry Pi is essentially running Linux my dad suggested that I look into .NET Core which can be run on Linux. After a bit of research, I discovered the .NET IoT Core library by Microsoft. It was easy to set up and quickly test and that is why the majority of the software is written in C#.

***Note:***
*1.  I am aware that C# is not the best language for a robotics project, but since I had so many other things to figure out, I decided that it was best to have at least one thing that I shouldn't worry about.*

*2. I am also aware of [ROS](https://www.ros.org/), but I found out about it too late into the project for it to be possible to switch.*
*3. Despite the cons, for this current project, C# is more than capable of delivering good performance. This has been determined by simply working with and on the system for 5 months*

##### [Back to top](#Contents)
---

## Software motor control
This is a separate section because there is more to it than turning the motor on and off.
The standard way of moving a stepper motor with the `A4988` driver is by pulsing the step pin at a given speed. For simple applications, this is more than enough, but since this is a robot and since there is a considerable amount of mass that needs to be moved, simply moving the motor from 0 to full speed is not possible. There are a couple of  unwanted side effects of such instant acceleration:

* The stepper motor losing steps
* Extreme stresses that can lead to structural failures like this:
<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Robot%20Images/Photos/Struc_Faliure.jpg?raw=true" width="270" height="480">
</p>

* Unnecessary wear on components.

A common approach that is used in larger robots is by using a [polynomial trajectory](https://www.youtube.com/watch?time_continue=8&v=HqQBL6xcj4w&feature=emb_logo) to ensure that the robot moves from one position to another smoothly over time. By using such a trajectory we can imagine the position, velocity and acceleration curves looking something like this:
<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Polynomial%20Func/SuperSmooth.png?raw=true" width="90%" height="90%">
</p>

This is done by using this kind of a quintic polynomial equation:

where the function itself gives us the change in position, the first derivative gives us the velocity curve and the third derivative gives us the acceleration curve. All of these curves are given for a set time period.

Let's imagine we want to rotate the base of the robot from 0 to 42 degrees. There are a couple of options:

If it takes ~ 0 seconds to move from 0 to 42 degrees, the curves would look something like this:
<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Polynomial%20Func/InstantAcceleration.png?raw=true" width="80%" height="80%">
</p>

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Polynomial%20Func/InstantVelocity.png?raw=true" width="80%" height="80%">
</p>

***Note: The Y-axis values are the value from the function and do not represent actual velocity or acceleration metrics***

As you can see, both acceleration and velocity almost instantly shoot up, this causes a lot of stress on the structure and the motor.

**Note: The curves are shown separately, to better illustrate the point.**

On the other hand, if we use a polynomial trajectory, the curves look something like this:

<p align="center">
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Polynomial%20Func/Poly_10sec.png?raw=true" width="80%" height="80%">

</p>
As you can see here, the acceleration is less and the curve is much more gentle.
Of course, if you reduce the time a motion needs to be executed in a smaller time frame even with the polynomial trajectory you can have high accelerations, in those cases, you can either use other trajectories like trapezoidal or design the robot to handle higher stresses.


After doing some research on the topic, I decided to try to implement this. Luckily for me, such a project has already been attempted and I am very grateful to Joseph Q. Oberhauser for the work he has done on his [master thesis ](https://etd.ohiolink.edu/!etd.send_file?accession=ohiou1460045979&disposition=inline) which has a part that covers this exact topic. I am also grateful that he has uploaded his C# code to his [dropbox](https://www.dropbox.com/sh/l5rgy7s7osya5vz/AABrLWKVYxZhbu4hxOwEMSMZa?dl=0) and still has it available at the time of writing this documentation.
Even though it doesn't cover everything I needed, with the help of his work I was able to implement a working polynomial trajectory for my robot.
It is not the smoothest operation, but that is mostly since I am not using micro-stepping and because my stepper motors only have 200 steps per revolution. Those two things result in quite a bit of noise, but overall everything works as intended and there is a visible improvement in performance as the stepper motor doesn't skip steps and there is less force exerted on the structure.


##### [Back to top](#Contents)

---
### Diagrams
Due to the relative complexity of the system, I will not write too much text to explain it. Instead, I will use the things I learned in my Software Architectures course this semester to create diagrams for my system.

---

### System modules


<p align="center">
<img src="https://raw.githubusercontent.com/NikolaTotev/Robotics-Course-Project/2a61a2f3e2c6e9b4ed6ecc0658b16b2103fae912/Documentation/Software%20Diagrams/Client_Modules.svg?token=AGMIB7MHU3PP2RZ6VG46N427BS6PE" width="50%" height="50%" >
</p>

<p align="center">
<img src="https://raw.githubusercontent.com/NikolaTotev/Robotics-Course-Project/2a61a2f3e2c6e9b4ed6ecc0658b16b2103fae912/Documentation/Software%20Diagrams/Server_Modules.svg?token=AGMIB7OJK5R7QLKEOSEVWOK7BS6QS" width="80%" height="80%">
</p>

##### [Back to top](#Contents)

---
### Deployment Structure
<p align="center">
<img src="https://raw.githubusercontent.com/NikolaTotev/Robotics-Course-Project/2a61a2f3e2c6e9b4ed6ecc0658b16b2103fae912/Documentation/Software%20Diagrams/Deployment_Diagram.svg?token=AGMIB7NAEJ43EJ5H462Q2WK7BS6RS" width="72%" height="70%">
</p>


##### [Back to top](#Contents)

---
