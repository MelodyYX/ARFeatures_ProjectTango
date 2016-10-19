# ARFeatures_ProjectTango
This is a small Augmented Reality app which works on Google Project Tango and was developed in Unity 5.4. 
There are four features: Freehand drawing, Measurement, Screenshot and Eraser.

## Features
* **Freehand drawing:** The annotations would spatially attached to real world objects so that they remain fixed 
at the location of interest.

* **Measurement:** Obtaining measurement by clicking target points on the screen.

* **Screenshot:** To save the current screen view as a picture. Showing the screen shot icon, 1 second later the
icon will desapear.

* **Eraser:** Delete the annotations.

## Setup
* Google Project Tango
* Unity 3D 5.4
* Tango SDK https://developers.google.com/tango/downloads
* Experience using tango-examples-unity is necessary

## Build
1. Creat a new project, import Tango SDK, Download all the objects in this repositories.
2. Add "Tango Point Cloud", "Tango Manager", "Tango AR Camera" to Scenes, and setup as same as 
Tango-exmaples-Unity-AugmentedReality .
3. Creat a 2D object "screenshotPic" to "Tango AR Camera".
4. Creat an object named "GUIController", add script "FeaturesGUIController" to it. 
5. Creat 2D object "Line", add Text"Distace" to "Line". Adjust their color and size.
6. Find your own icon pictures and add them to FeaturesGUIController.
7. Add "ARMeasureMarker" as a script component to "Measure Marker.prefeb". 
