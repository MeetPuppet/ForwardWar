Version 1.0

My email is "kripto289@gmail.com"
Discord Kripto#6346
You can contact me for any questions.
My English is not very good, and if I have any translation errors, you can write me :)

Demo scenes include open source projects for volumetric light and volumetric clouds.
https://github.com/SlightlyMad/VolumetricLights
https://github.com/yangrc1234/VolumeCloud


DEMO SCENE CORRECT SETTINGS:
1) Use linear color space. Edit-> Project settings -> Player -> Other settings -> Color space -> Linear
If you use gamma space, then you need to change light intencity and water transparent/turbidity for better looking.

2) Import "cinemachine" (for camera motion) and "post processing"
Window -> Package Manager -> click button bellow "packages" tab -> select "All Packages" or "Packages: Unity registry" -> Cinemachine -> "Install"
Window -> Package Manager -> click button bellow "packages" tab -> select "All Packages" or "Packages: Unity registry" -> Post Processing-> "Install"
3) Restart unity (post processing required)


WATER FIRST STEPS:
1) Create gameobject
2) Add the script "WaterSystem"
3) See the video description of each setting. Just click a help box with symbol "?"
Each parameter include

USING FLOWMAP EDITOR:
1) Enable "flowmap" tab
2) Click "Edit mode" button
3) The pivot point of water gameobject should be in the area where you want to draw.
4) Use "world offset" parameter for moving flowmap area (relative to water pivot point)
5) Use "Area size" parameter for changing flowmap area size(in meters).
6) Use "texture resolution" for changing flowmap baked texture. This parameter relative to area size.
Final quality of flowmap used formula = texture resolution / area size. For example 1024 pixels / 100 meters area = ~10 pixels per meter.
More pixels -> better quality -> more memory using.
7) Use left mouse button click to draw on flowmap
8) Use ctrl button for erase on flowmap
9) Use mouse wheel for changing brush area size.
10) Save all changes
11) All changes will be saved in the folder "Assets/StreamingAssets/WaterSystemData/WaterID", so be care and don't remove it.

USING SHORELINE EDITOR:
1) Disable "selection outline" and "selection wire" in Gizmos (Scene tab -> Gizmos button).
Otherwise shoreline rendering will be slow when you select the water in hierarchy.
2) Enable "shoreline" tab
3) Click "Edit mode" button
4) The pivot point of water gameobject should be in the area where you want to create shoreline waves.
5) Use "Area size" parameter for changing shoreline area size(in meters).
6) Use "texture resolution" for changing shoreline baked texture. More resolution -> you can use smallest waves but more memory using
7) Click "Add Wave" or "Insert" button. Use "Delete" button for removing wave.
8) You can use move/rotate/scale tool. Be careful, wave boxes with the same colors should not be intersected.
Otherwise, the waves will be superimposed on each other.
9) Save all changes.
10) All changes will be saved in the folder "Assets/StreamingAssets/WaterSystemData/WaterID", so be care and don't remove it.






