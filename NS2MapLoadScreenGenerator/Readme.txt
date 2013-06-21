/*
	README
	
	This is a tool to generate map screenshots for the loading screen for maps in ns2.
	
	It is by Confused! 
	
	
	
	
	Usage
		 ns2MapLoadScreenGenerator.exe name_of_map [path to root]
		 
		 it looks at the following files:
		 	path/ns2/maps/name_of_map.level -parsed for entites
		 	path/ns2/maps/overviews/name_of_map.tga -used for minimap
		 	path/ns2/screens/name_of_map/ -created if doesnt exist used to store screenshots
		 	path/ns2/screens/name_of_map/src - if jpgs exist here used as the background image
	
	Known issues:
		The font is wrong. This casues weirdness on the labels on teh minimap
		no vignette effect on screenshots.
		
	ToDo:
		support randomizing the random map
		support executing the minimap generator if needed
		not have the worlds worst level parser.
		
	Thanks to:
	
		http://www.codeproject.com/Articles/31702/NET-Targa-Image-Reader
		https://github.com/DamienHauta


*/
