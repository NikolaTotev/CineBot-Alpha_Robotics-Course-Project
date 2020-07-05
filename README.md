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
		
-  [Software](#Software)
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

Since these motors lack the holding torque to hold the arm of the robot horizontal or to even lift it, [reducer mechanisms](#Reducer-mechanisms)  had to be implemented


### Assembly methods
As I am using 3D printing for the making all of the parts I need to take into account the weaknesses and strengths of additive manufacturing in order to print good parts. 
Most of the parts are printed in a way that avoids overhangs as much as possible. Layer orientation is taken into consideration when possible. 

In order to asseble all of the parts I use two main techniques:
- M3 & M2.5 machine screws of various lengths. (more information can be found in the [assembly booklet](#Assembly-Information)
- Trapezoid wedges that slide into each other 

The combination between these two methods allows for a good balance between solid assembly of large modules and allows those modules to be connected using a universal attachement interface.

===	INSERT IMAGE ===

***Note:*** Initially I used two other methods of assembling the robot:
1. Friction fit connections with square or hexagonal shape to allow for easy assembly/disassembly.
2. Screws for all of the connections.

Both of these method had significant downsides. The friction fit relied on tight tollerances, something that is not consistent when 3D printing parts.
For the screw method, once the larger parts were connected, if something needed to be replaced, a siginificant amount of work needed to be put in to change a singlep part.

### Other resource constraints regarding hardware development

##### [Back to top](#Contents)
## Hardware design decisions
### Structure overview (part evolution and motivation behind the design)
The structure of the robot is fairly simple. In the following points I'll describe each part and all of the itterations I had to go though. The general goals for the hardware can be divided into two sections:
* Robot structure

	1. It has to be printable 
	2. The parts need to be able to support the expected forces and use as little material as possible
	3. The reducer mechanisms need to move smoothly
	4. As mentioned above assembly needs to be pretty simple
	5. Parts must be modular and easy to swap out
	6. Built-in cable management.
	7. It needs to look good (this has s lower priority, but still an important aspect)
	
* Electronics housing 
	1. There must be enough room to work in side and fit the electronics
	2. Cooling needs to be integrated into the case to help with performance
	3. Notification LEDs need to be in a good position for the user
	4. Any controls such as rotary encoders also need to be in a easily accesible spot.
	5. All ports that are used for connection to the robot need to be in the back
	6. As with the robot it has to look good (again lower priority)

*Some of the things mentioned in the electronics housing are described in more detail in the [electrical system](#Electrical-system) section.*
####  A little bit about iterations
I have decided to go through all of the itterations because I think it is important for me to show the whole process. That away I can describe the things I've learned along the way and it is a great section for beginners to see the mistakes I've made so they don't make them as well. 
Since this is quite a lengthy section I will provide skip links so that you can only see the final design or so you can skip directly to the [software part](#Software) of the project. 

####	Base & base drive train
Since this is a stationary robot, the base is a key part of the design and a good base is essential for the overall stability of the system. This section also also experiences the most loading and has to be designed with that in mind. I also want to cover the different adapters that are used to connect the arm sections to the base subassembly.
* ___Base & base drive train iterations___
	* __Iteraton 1__
		INSERT IMAGE
		Initially I started off with this design. As you can see it is a simple and small gearbox and the larger gear has a hexagonal shaft. and it sits in a stand. The idea behind this was to have the rest of the robot be friction fit onto it, however there were many tolerance issues. Another significant issue with this base, it had a small footprint and once the robot was friction fit ontop  there was a lot of wobbling around and I abandoned the idea.
	* __Iteraton 2__
	INSERT IMAGE
	This iteration was inspired by the  first version of the arm joint planetary gears and then a smaller version of this base gear box was used in the final iteration of the arm joint. In theory this stetup should've been much more stable, but the way I chose to attach the base adapter created a lot of problems. This gearbox also had some binding issues in some spots,which resulted in irregular motion and the stepper motor loosing steps. The problems mentioned along with the arm section issues at the base of the robot made me rework the entire base.
	
	* __Iteraton 3__
	INSERT IMAGE
	This is the final iteration. As you can see it quite different and uses a much simpler and adjustable drive train. In the base there are 2 75 mm *(outer diameter)* bearings that provide the structural stabilitiy and smooth rotational movement. This base fixes all of the issues that the previous ones had and helped solve the issues with the base arm section

**Thigs I learned** - Its best to keep reducer mechanisms as simple as possible. It is important to consider the forces that will be exerted on a part, especially on the base of the robot. Bearings are something that should be used more often, they significantlly improve the final result and are a great addition to 3D printed mechanisms. 
	
* ___Base Adapter iterations___
   This will just quickly cover the different adapters I made.
	* __Iteraton 1__
		INSERT IMAGE
	Uses hexagonal friction fit on one side and screws for mounting the arm section. 
		Cons - 
		*	No cable management
		*	Tolerance issues
		*	Time-consuming to mount/unmount arm sections
		*	Too thin for the loads it would experience
	
		Pros - 
		* simple design
		* quick to print.
		
	*  __Iteraton 2__
== INSERT IMAGE==
		This  version as you can see it a bit better, but it still has the major issues of the previous one.
	
		 Pros - 
		 *fixes thinness issue
		 * adds some stability due to wider footprint
		 
		 Cons
		 * still time-consuming to mount arm sections
		 * still has tolerance issues
		 
	* __Iteraton 3__
	* == INSERT IMAGE==

		This one solves all of the issues of the previous ones, but introduces some new ones. As you can see it is connected with screws to the gearbox, but those screws also hold the planet gears and carrier in alignment. This means that they can't be tightened too much as it will cause the gearbox to bind. Despite those problems, the easy swap functionality that comes from the use of a universal trapezoid mount allows for easy mounting of the other part of the robot. This base also has cable management that feeds the cables out of the back.
		
* Iteration 3.1
	The final iteration of this part is actually removing it altogether. See the last arm iteration to understand the whole idea.

#### Arm section
This is the most important part of the robot. It make up the majority of the assembly and without it the robot would not work. This part also transmits the most load and has to be made both light and strong. Many iterations were needed to get to the final version.
* Iteraton 1
==INSERT IMAGE==
This was not thought trough very well. As previous first iterations it relies on friction fits and too many screws to be practical.

* Iteraton 2
==INSERT IMAGE==
   This iteration is also poorly designed. While the shape is cool, it is too thin, too small and again relies on a friction fit.
   
* Iteraton 3
==INSERT IMAGE==
This version is a bit better, much stronger but still too small and once again it uses too many screws.

* Iteraton 4
 ==INSERT IMAGE==
	  This iteration is where better design choices emerge. There is proper cabel management, it is light yet strong. Unfortunately, it is too complex and takes too long to print. It also uses too many screws.
* Iteraton 5
 ==INSERT IMAGE==
	This is the first iteration where a the universal connector is used. The shape is simplified and there is better cable management. This design goes through 2 updates to make it stronger it experienced a structural faliure once.
* Iteration 5.1
* Iteration 5.2
* Iteraton 6
==INSERT IMAGE==
	For this iteration, the top arm section is the same, but the base arm section has been reworked due to the structural falire shown below. This change also comes from the redesign of the base drive train. As you can see, it still keeps the universal connector, it is also much more stable, compared to the previous arm sections.
	
#### Arm Joint
This part is the one and only "joint". I call it a joint because it connects two arm sections in a mobile way. This part of the robot had a couple of requiredments to meet:
1. It needs to be strong enough (structurally) to lift the weight of the arm.
2. Since the stepper motor can't lift the arm on its own the joint needs to be some kind of reducer with an appropriate gear ratio. The gears must also operat smoothly.
3. It needs to implement a universal mounting system.
		
* Iteraton 1
		== INSERT IMAGE ==
	  In the very beginning since I had never worked with stepper motors I didn't know that they can't lift much on their own, and they require some kind of reduction mechanism.
	  This is why the first iteration has no gears whatsoever.
	  
 * Iteraton 2
==INSERT IMAGE==
	 This iteration uses a very simple planetary gearbox with a 7:1 reduction ratio. Was able to move the arm, but as it relies on a friction fit it is not stable. Along with those issues, after the gimbal iterations it proved to be too weak.
* Iteraton 3
==INSERT IMAGE==
In this version, I deicded to try to make a worm drive, but since it is a much harder reducer to make it had internal friction issues that caused it to be too weak.

* Iteraton 4
==INSERT IMAGE==
This is the final iteration and it is a smaller version of second iteration of the base gearbox.  It shares it's flaws, but they were not as pronounced due to the smaller foces exerted on it. Using this new gearbox allowed me to finally add the universal mounting bracket to it.

#### Joint to arm section interfaces/adapters
* Iteraton 1
	==INSERT IMAGE==
	As you can see there is more than one version, but I'm grouping them as the first iteration as they do not include any counterbalance. These are simple, they all do their job, but due to the weight of the gimbal, a counterbalance is requried.
* Iteraton 2
==INSERT IMAGE==
As you can see on this version, at the back there is a mounting point for the counter balance mentioned above. This makes the job of the reducer much easier by balancing the forces.

#### Counter balance 
* Iteraton 1
This part is pretty simple. Since I neede something to balnce the gimbal, and an arduino is needed to control the motors, I decied to combine the two and this is the result. The back has a tray for some metal weights and on the top there are mounting holes for the arduino. As mentioned above it connects to the second iteration of the arm joint adapter

#### Gimbal
This part also has a lot of iterations, as initially I didn't know what the best approach was. All of the axes are controled via servo motors.
* Iteraton 1
==INSERT IMAGE==
 This looks like a standard gimbal, the rotation motor housing is quite big and it is really heavy. Along with that, the camera mount is weak and the tilt and pan arms are also flimsy.
* Iteraton 2
==INSERT IMAGE==
The second version of the gimbal is very complex, and rather large. Due to this fact it is very unstable and I quickly abanoned it. 
* Iteraton 3
The third version is inspired by the second one, but attempts to minimize it by using smaller servos and smaller pieces. This design has major alignment issues. It is also very difficult to assemble and easy to break.
* Iteraton 4
==INSERT IMAGE==
This final design is inspired by [a project I found on youtube](https://www.youtube.com/watch?v=wQypj7ti7Vw). I was already behind schedule and I thought that it's time to look for some help. I had to make the models from scratch, since I am using servo motors and the person in the video is using brushless gimbal motors.

#### Limit switches
These don't really have any iterations. I just bought a simple switch from the store and mounted it to the robot with two simple mounts. Despite their simplicity, these are critical parts as the allow the robot to "know" what position it is in. Please read [the improvemens section](#Possible-improvements) for more insight on the improvements I would make in the area of positional awareness. 





#### Electronics housing ***(design aspects)***
##### [Back to top](#Contents)

### Electrical system
#### Schematics
* ___Stepper motor connection schematic___ [_alternate link_](https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Stepper%20Motor%20Connection%20Schematic.png)
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Stepper%20Motor%20Connection%20Schematic.png?raw=true=" width="707.5" height="667.5">

* ___Encoder & switch connection schematic___ [_alternate link_](https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Button%20and%20Encoder%20Connections%20Schematic.png) 
<img src="https://github.com/NikolaTotev/Robotics-Course-Project/blob/master/Documentation/Images/Button%20and%20Encoder%20Connections%20Schematic.png?raw=true" width="707.5" height="667.5">

#### Used components
#### Electronics housing and connections to robot

### Possible improvements
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
##Assembly Information
