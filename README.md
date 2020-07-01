# Project Cinebot Alpha 

This is the documentation for my Robotics Course Project created during my second year, second semester in FMI for the course Practical Robotics and Smart "Things". 

## *Acknowledgements*
Before I begin, I would like to acknowledge my parents, for their constant love and encouragement, not only for this project, but for all of my previous and current envdeavours. Without them this project would not be the same.
I would also like to thank the teachers the teachers that guided me though the skills I needed to learn in order to make this project a reality. 
In no particular order I'd like to thank
Dr. Ivan Chavdarov, for teaching me the skills related to 3D modelling & printing, as well as giving me guidance on my structural design by telling me what could be improved and how such things are usually made/designed.
and
Trayan Iliev, for expanding my horizons on the subjects of IoT and the software side of robotics.

# Contents
-  [Introduction](#Introduction)
	* [Scope](#Scope)
	* [Key features](#Key-features)
	* [Initial design decisions](#Initial-design-decisions)
-  [Hardware](#Hardware)
	* [Design constraints](#Design-constraints)
		* [Actuators](#Actuators)
		* [Method of manufacturing](#Method-of-manufacturing)
			* [Manufacturing capabilities](#Manufacturing-capabilities)
			* [Material choice](#Material-choice)
		* [Assembly methods](#Assembly-methods)
		* [Other resource constraints regarding hardware development](#Other-resource-constraints-regarding-hardware-development)
	
	* [Hardware design decisions](#Hardware-design-decisions) 	
		*	[Structure overview](#Structure-overview) (part evolution and motivation behind the design)
			*	[Base & base drive train](#Base-&-base-drive-train)
			* [Arm sections](#Arm-sections)
			* [Reducer mechanisms](#Reducer-mechanisms) 
			* [Counter balance](#Counter-balance)
			* [Gimbal](#Gimbal)
			* [Limit switches](#Limit-switches)
			* [Electronics housing](#Electronics-housing) ***(design aspects)***
			
		* [Electrical system](#Electrical-system)
			* [Schematics](#Schematics)
			* [Used components](#Used-components)
			* [Electronics housing and connections to robot](#Electronics-housing-and-connections-to-robot)
			
		*  [Misc](#Misc)	
		
- ## [Software](#Software)
	- [Software Requirements](#Software-Requirements)
		- [Safety requirements](#Safety-requirements)
		- [Required modes of operation](#Required-modes-of-operation)

	- [Software Architecture](#Software-Architecture)
 	- [Choosing a suitable platform](#Choosing-a-suitable-platform)
	- [Programing language selection](#Programing-language-selection)

	- 
# ==== Disclaimer ====
This project is for ***educational purposes only***. This is my ***first*** robotics project and there are certainly bad practices/decisions that could lead to injury or damage to components. 
If you decide to recreate this please do so carefully. If any issues are discovered please put in an issue so I can fix it.


# Introduction
## Documentation structure
This project has two main sections hardware & software. The structure of this documentation begins with the hardware aspects of the project because that is the way the development of this project began. After that I will continue with the software side of things and at the end...**(TODO)**

 The goal of this documentation is to inform the reader about the project and give him/her a better understanding about the project, what the motivations are behind the various design decisions and how the many harware and software components interact with on another to create a complete robot as well. This document will also cover the many mistakes I made along the way, the challenges I faced as a complete beginner, how I over came them. I also describe the lessons I learned along the way and point out the improvements that can be made if someone attempts to recreate this project.

Since this is my first project and documentation of such scale I would like the reader to know that there may be some errors and things that can be done better. If you find such a thing, please make an issue in Github so I can get some constructive feedback and so I can improve this project for future readers.
 
## Scope
Since this is my first robotics project, the scope of this project is farily limited but quite broad for a beginner.  At the start of this project the scope was quite limited, but as the development progressed so did the scope because I discovered new things that would help me improve this project and teach me even more new things.

During the development of this project in no particular order I wanted to:

* Learn about the platforms that are available for robotics and that are suitable for beginners but at the same time have enough room for growth in the future
* Using 3D modelling for visualizing the robot and as a key component in the manufacturing phace.
* Exploreing different methods of creating the physical structure of the robot.
* How actuators are made/chosen and how they are controlled
* Learn more about key principles about robotics such as
	* Kinematics
	* How to deal with forces/balancing issues
	* Safety considerations when designing a robot
	* Motion smooth / using qinitic polynomial equation in order to manage forces on the robot cause by acceleration during movement.
* Learn how to make a basic multithreaded control software for the robot
* Facial tracking using openCV and implementing a PID controller to track a face by moving the robot.


## Key features

## Initial design decisions 
The first choice I had to make was what kind of robot would be suitable for this project. The two main choices where either a mobile robot or a stationary robot arm. I know there are more options but considering my limited knowledge at the beginning I wanted to attempt one of the two types. In the end I decided to make a stationary robot arm for the following reasons:

1. Work environment - Before and after the pandemic my work environemt was limited and setting up an environment for a mobile robot would have impractical. For a stationary robot it was much easier to designate one small spot for it,
2. Powering the robot - I only knew about lab power supplies and how they are used for such experiemtal project, based on the knowledge my only reasonable option was a stationary robot as a mobile one would required batteries in order to operate freely.

Once I decided what kind of robot I would make the size of the robot was the next decision. Since I have a limited work area and limited resources such as time,money and manufacturing capabilities I decied to make a fairly small robot arm that doesn't really have a practical use but is perfect for educational purposes.

Before beginning with the CAD design, I used Legos to create a crude model of the robot arm and the joints it would have and how they would move. I found this to be the best way to make the initial prototype as it cost nothing and gave a good enough visualization/feel.

!!== INSERT IMAGE ==!!

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
- Autodesk AutoCAD 2020 (Student edition)
- Wolfram Mathematica
- Github Desktop
- WinSCP
- .NET Core IoT Libraries
- Flashprint (Slicer)

##### [Back to top](#Contents)

# Hardware

## Design constraints
This section covers the design contraints I had during the developent of the hardware and how the constraints affected the final design. 

### Method of manufacturing 
Since this project requires are higher accuracy of the parts and since I had limited time to actually make each part myself I decided that the best option would be to utilize 3D printing to make all of the parts for the robot. Since I also enrolled in a 3D modelling course in the same semester, the decision to use 3D printing would mean that I could apply the from one course in another.

#### Manufacturing capabilities
The printer I chose for this project as mentioned in the tools section is the Flashforge Creator Pro. Since it was my first printer I decided to go with this model because it has dual extruders and the ability to print in a variety of materials, giving me enough options for the future.

#### Material choice
Due to the fact that all of the printing would be happening in my room, I decided to go for **PLA** as it does not emit toxic odors and it is significantly easier to print with. The strength of the material is adequate and is suitable for relatively small forces involved.

### Actuators
For the primary actuators I decides to use two NEMA 14 35x28 stepper motors and for the gimbal actuators I use two micro [servos with metal gears]([https://erelement.com/servos/feetech-ft90m](https://erelement.com/servos/feetech-ft90m)) for the rotate and tilt axes and a [larger servo with plastic gears ](https://www.robotev.com/product_info.php?cPath=1_40_45&products_id=205(https://www.robotev.com/product_info.php?cPath=1_40_45&products_id=205)) for the pan axis.

*Due to lack of knowledge at one stage of the project I had to change the stepper motors from my initial NEMA 14 35x26 to the slightly more powerful 35x28. That said, I believe it is possible to make this project work with the smaller motors but better gearing would be required*



### Assembly methods
### Other resource constraints regarding hardware development
### Possible improvements
##### [Back to top](#Contents)
## Hardware design decisions
###	Structure overview (part evolution and motivation behind the design)
####	Base & base drive train
#### Arm sections
#### Reducer mechanisms 
#### Counter balance 
#### Gimbal
#### Limit switches
#### Electronics housing ***(design aspects)***
##### [Back to top](#Contents)

### Electrical system
#### Schematics
#### Used components
#### Electronics housing and connections to robot
		
###  Misc	
##### [Back to top](#Contents)
		
# Software 
## Software Requirements
	### Safety requirements
	### Required modes of operation 

## Software Architecture 
### Choosing a suitable platform
### Programing language selection
##### [Back to top](#Contents)
