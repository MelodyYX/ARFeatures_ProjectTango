# ARFeatures_ProjectTango
This is a small Augmented Reality app which works on Google Project Tango and was developed in Unity 5.4. 
There are four features: Freehand drawing, Measurement, Screenshot and Eraser.

## Features
* **Freehand drawing:** The annotations can be spatially attached to real world objects so that they remain fixed 
on the location of interest.

* **Measurement:** Obtain distance measurement by clicking target points on the screen.

* **Screenshot:** Save the current screen view as a picture. Shows the screenshot icon, 1 second later the icon will disappear.

* **Eraser:** Delete the annotations.

## Setup
* Google Project Tango
* Unity 3D 5.4
* Tango SDK https://developers.google.com/tango/downloads
* Experience using tango-examples-unity is necessary

## Build

1. Create a new project, import Tango SDK, Download all the objects into this repository.

2. Add "Tango Point Cloud", "Tango Manager", "Tango AR Camera" to Scenes, and setup the same as 
Tango-examples-Unity-AugmentedReality.

3. Create a 2D object "screenshotPic" to "Tango AR Camera".

4. Create an object named "GUIController", add script "FeaturesGUIController" to it. 

5. Create 2D object "Line", add Text "Distance" to "Line". Adjust their color and size.

6. Find your own icon pictures and add them to FeaturesGUIController.

7. Add "ARMeasureMarker" as a script component to "Measure Marker.prefeb". 
