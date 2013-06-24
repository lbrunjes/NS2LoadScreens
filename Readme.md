
#N2 Load Screens

This is a tool to generate map screenshots for the loading screen for maps in ns2. That way every one wins.


it looks at the following files:
	core/fonts/font.ttf -if your font is notinstalled it looks here second
 	ns2/maps/name_of_map.level - parsed for entites
 	ns2/maps/overviews/name_of_map.tga - used for minimap data
 	ns2/screens/name_of_map/ - created if doesn't exist, used to store screenshots
 	ns2/screens/name_of_map/src - if jpgs exist here used as the background images in alphabetical order
 	overview.exe - if minimaps are to be refreshed


###Usage:
	ns2MapLoadScreenGenerator.exe name_of_map [font] [generate new minimap]
###Examples: 
	ns2MapLoadScreenGenerator.exe ns2_tram impact true
	ns2MapLoadScreenGenerator.exe ns2_descent (implies the default font and to generate a new minimap if needed)
	 

###Known issues:
	????

###ToDo:
	support randomizing the random generic screenshoot
	not have the worlds worst level parse
	
###Thanks to:

	http://www.codeproject.com/Articles/31702/NET-Targa-Image-Reader
	
	https://github.com/DamienHauta for having a signinficantly better level parser than me.
