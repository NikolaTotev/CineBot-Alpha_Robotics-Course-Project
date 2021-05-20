
# Story House
HMI (Human Machine Interface) Course Project
[Link to the published app](https://fmi-storytellers.github.io/StoryHouse/#/)
# UI Library Updates
### Header usage & location

#### Where to find it 

    📁frontend > 📁storyhouseui > 📃ShHeader.dart

#### How to use:
##### Import

import 'package:storyhouse/frontend/storyhouseui/ShHeader.dart';
##### Usage
     Parameters
     currentUserType: UserTypes{user, admin, author, libAdmin, guest}
     currentPate: PageTypes{home, library,favs, dashboard, profile, users, adminlibraries, books, NA } 
     
     ShHeader(currentUserUserType: UserTypes.user, currentPage: PageTypes.home,)
---
### Added ShBasePage
#### Where to find it 

    📁frontend > 📁storyhouseui > 📃ShBasePage.dart

#### How to use:	
##### Import

    import 'package:storyhouse/frontend/storyhouseui/ShBasePage.dart';

##### Usage
     Parameters
     pageContents: List<Widget>
	 header: SheHeader()
	 
     ShBasePage(pageContents: [], header:  ShHeader(currentUserUserType: UserTypes.author, currentPage: PageTypes.home,) )

# UI Library Documentation
## Index
* [Header](#header)
* [Buttons](#buttons)
* [Images](#images)
* [Input](#input)
* [Text](#text)



## Colors
- ![#E38346](https://via.placeholder.com/15/E38346/000000?text=+) `#f03c15` sh_AccentOrange
-  ![#FFB383](https://via.placeholder.com/15/FFB383/000000?text=+) `#f03c15` sh_SoftOrange
- ![#FFEEDE](https://via.placeholder.com/15/FFEEDE/000000?text=+) `#f03c15` sh_WhiteOrange/sh_AccentButtonText
-  ![#FFDE6D](https://via.placeholder.com/15/FFDE6D/000000?text=+) `#f03c15` sh_RatingYellow
- ![#FFFFFF](https://via.placeholder.com/15/FFFFFF/000000?text=+) `#f03c15` sh_White
-  ![#757575](https://via.placeholder.com/15/757575/000000?text=+) `#f03c15` sh_ShadowBlack
-  ![#E0E0E0](https://via.placeholder.com/15/E0E0E0/000000?text=+) `#f03c15` sh_DividerGrey
- ![#424242](https://via.placeholder.com/15/424242/000000?text=+) `#f03c15` sh_TextBlack



## Header
#### Where to find
    📁frontend > 📁storyhouseui > 📃ShHeader.dart
#### How to import

    import 'package:storyhouse/frontend/storyhouseui/ShBasePage.dart';


#### Usage

     // Parameters
     currentUserType: UserTypes{user, admin, author, libAdmin, guest}
     currentPate: PageTypes{home, library,favs, dashboard, profile, users, adminlibraries, books, NA } 
     
     // Example
     ShHeader(currentUserUserType: UserTypes.user, currentPage: PageTypes.home,)

## Buttons

#### Where to find
    📁frontend > 📁storyhouseui > 📃Buttons.dart
#### How to import

    import 'package:storyhouse/frontend/storyhouseui/ShButtons.dart';


#### Usage

##### Text Button
     // Parameters
     buttonText: String
     callback: VoidCallback, (){}  
     textColor: String, ColorMaster.<Select String>
     backgroundColor: String, ColorMaster.<Select String> 
     
     textSize: double
     font: FontType.<Select from enum>
     fontWeight: FontWeight.<Select from enum>

     // Button Padding
     unformPadding: bool     
     buttonPadding: double, default value = 10
     l_buttonPadding: double, default value = 10
     r_buttonPadding: double, default value = 10
     t_buttonPadding: double, default value = 10
     b_buttonPadding: double, default value = 10
     
     // Text Padding
     uniformTextPadding: bool
     textPadding: double, default value = 16
     l_textPadding: double, default value = 16
     r_textPadding: double, default value = 16
     t_textPadding: double, default value = 16
     b_textPadding: double, default value = 16

	  
     
     // Example
      ShTextButton(  
		  "I'm a Text button",  
		  () {  
			  Clipboard.setData(ClipboardData(text: StringMaster.ButtonType1CODE));  
		  },  
		  ColorMaster.sh_White,  	  //Text Color  
		  ColorMaster.sh_AccentOrange,  	  //Background color
		  16,  // Font ize  
		  FontType.mont,  
		  FontWeight.w600,  
		  uniformPadding: true,  
		  buttonPadding: 10,  
		  textPadding: 10,  
		),
##### Icon Button

    // Parameters
     buttonIcon: IconData
     callback: VoidCallback, (){}  
     textColor: String, ColorMaster.<Select String>
     backgroundColor: String, ColorMaster.<Select String> 
     
     iconSize: double
    

     // Button Padding
     unformPadding: bool     
     buttonPadding: double, default value = 10
     l_buttonPadding: double, default value = 10
     r_buttonPadding: double, default value = 10
     t_buttonPadding: double, default value = 10
     b_buttonPadding: double, default value = 10
     
     // Icon Padding
     iconPadding: double, default value = 16
   

	  
     
     // Example
      ShIconeButton(  
		  Icons.star,,  
		  () {  
			  Clipboard.setData(ClipboardData(text: StringMaster.ButtonType1CODE));  
		  },  
		  ColorMaster.sh_TextBlack, //Icon Color  
		  ColorMaster.sh_WhiteOrange, //Background Color
		  30,  // Icon size  
		  uniformPadding: true,  
		  buttonPadding: 10,  
		  iconPadding: 10,
		  borderRadius  
		),

## Images
#### How to import
#### Usage
#### Example
## Input
#### How to import
#### Usage
#### Example
## Lists
#### How to import
#### Usage
#### Example
## Text 
#### How to import
#### Usage
#### Example


# Other useful resources

### Intro tutorials
* [Flutter basics (Building your first app)](https://www.youtube.com/watch?v=xWV71C2kp38)
* [Flutter state (The basics of state in flutter)](https://www.youtube.com/watch?v=QlwiL_yLh6E)
* [Widget intro](https://flutter.dev/docs/development/ui/widgets-intro)
### Other tutorials
* [Creating stateful widgets](https://www.youtube.com/watch?v=wE7khGHVkYY)
* [How stateful widgets are best used](https://www.youtube.com/watch?v=AqCMFXEmf3w)
* [Info about textbox decorations](https://medium.com/flutter-community/a-visual-guide-to-input-decorations-for-flutter-textfield-706cf1877e25)


### Layout
* [General Layout Documentation](https://flutter.dev/docs/development/ui/layout)
* [Layout tutorial](https://flutter.dev/docs/development/ui/layout/tutorial)
* [List of layout widgets](https://flutter.dev/docs/development/ui/widgets/layout)
* [Information about constraints](https://flutter.dev/docs/development/ui/layout/constraints)

The ones we are most likely to use: 

* [Columns](https://api.flutter.dev/flutter/widgets/Column-class.html)

* [Rows](https://api.flutter.dev/flutter/widgets/Row-class.html)

* [Center](https://api.flutter.dev/flutter/widgets/Center-class.html)

* [Alignment](https://api.flutter.dev/flutter/widgets/Align-class.html)

* [Padding](https://api.flutter.dev/flutter/widgets/Padding-class.html)

* [Expanded](https://api.flutter.dev/flutter/widgets/Expanded-class.html)

* [SizedBox](https://api.flutter.dev/flutter/widgets/SizedBox-class.html)

* [Container](https://api.flutter.dev/flutter/widgets/Container-class.html)

* [Stack](https://api.flutter.dev/flutter/widgets/Stack-class.html)

* [SingleChildScrollView](https://api.flutter.dev/flutter/widgets/SingleChildScrollView-class.html)
### State Management
* [More details about state management](https://flutter.dev/docs/development/data-and-backend/state-mgmt/simple) 

###  Packages being used for the project
* [Provider](https://pub.dev/packages/provider) For state management
* [HexColor](https://pub.dev/packages/hexcolor/example) For working easier with hex color codes
* [auto_aoute](https://pub.dev/packages/auto_route) For navigation
	* [auto_route_generator](https://pub.dev/packages/auto_route_generator) Helper dev plugin to auto_route package for automatically generating navigation files
	* [build_runner](https://pub.dev/packages/build_runner) Helper dev plugin used for automatically generating navigation files
* [Google Fonts](https://pub.dev/packages/google_fonts) For working easier with fonts
	


