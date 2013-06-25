/*
 *  draws a basic mini map overview slide set ofr an ns 2 map
 *  Lee Brunjes 2013
 *  uses:
 *  http://www.codeproject.com/Articles/31702/NET-Targa-Image-Reader
 *  TODO:
 * 	draw techpoints/location names onto minimap.
 *  fonts
 */

using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace NS2MapLoadScreenGenerator
{
	class MainClass
	{
		#region varaibles
		const string configFile="loadscreens.ini";

		public static String instructions = @" 
Generates loading screens from and ns2 map.

use:
loadscreens.exe ns2_map_name [font] [refreshMinimap] [fontSize]

";
		static string fontDir = "core\\fonts\\";
		static string mapsDir = "ns2\\maps\\";
		static string overviewDir= "ns2\\maps/overviews\\";
		static string screensDir = "ns2\\screens\\";
		static string screenSrcDir = "\\src\\";
		static float bigFontSize = 72f;
		static float minimapFontSize = 24f;
		static Font bigFont = new Font (FontFamily.GenericSansSerif, bigFontSize);
		static Font minimapFont = new Font(FontFamily.GenericSansSerif,minimapFontSize);

		static int minimapxoffset = 1120;
		static int minimapyoffset = 0;
		static int minimapsize =640;
		static int bigTextX = 640;
		static int bigTextY = 64;

		static String tpIconPath="";
		static String rpIconPath="";
		static String defaultFont = "";
		static bool refreshMinimap = false;


		static Bitmap tp_icon;
		static Bitmap rt_icon;
		#endregion

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main (string[] args)
		{
			//Read config file
			ReadConfigFile ();


			String map = "ns2_test";
			String path = Environment.CurrentDirectory+"\\";
			String font = defaultFont;
			Bitmap overview;

			#region parse command line args & load things
			if (args.Length > 0) {
				map= args[0].Replace(".level","");
			}

			if (args.Length > 1) {
				font= args[1].ToLower();
			}

			if(args.Length > 2){
				refreshMinimap = args[2].ToLower() == "true" || args[2].ToLower()=="yes";
			}
			if( args.Length >3){
				float.TryParse(args[3], out minimapFontSize);
			}
			Console.WriteLine (String.Format(@"NS2 Map Load Screen generator
---------------------------------
 {0}

Level:{1}
font :{2}
update minimap:{3}
fontSize: {4}
---------------------------------",
			                                instructions, 
			                                 map,
			                                 font,
			                                 refreshMinimap,
			                                 minimapFontSize));
			// if we aint got a mp no point in going forward.
			if(map == ""){
				return;
			}
			if(File.Exists("icon_techpoint.png")){
				tp_icon = new Bitmap ("icon_techpoint.png");
			}
			else{
				Console.WriteLine ("WARNING: Cannot load techpoint Icon");
				tp_icon = new Bitmap(20,20);
				Graphics g = Graphics.FromImage(tp_icon);
				g.FillRectangle(Brushes.Orange, 0,0,19,19);

			}
			tp_icon.MakeTransparent ();
			#endregion

			#region check fonts
			bool fontFound = false;
			string installedFonts = "";
			InstalledFontCollection ifc = new InstalledFontCollection ();
			foreach (FontFamily fontfamily in ifc.Families) {
				if (fontfamily.Name.ToLower() == font) {
					bigFont = new Font (fontfamily, bigFontSize);
					minimapFont = new Font (fontfamily, minimapFontSize);
					fontFound = true;
				}
				installedFonts += fontfamily.Name+"\n";
			}
			if (!fontFound) {

				PrivateFontCollection pfc = new PrivateFontCollection ();


				if (File.Exists (path+fontDir+font+".ttf")) {
					pfc.AddFontFile (path+fontDir+font+".ttf");
					bigFont = new Font (pfc.Families [0], bigFontSize);
					minimapFont = new Font (pfc.Families [0], minimapFontSize);
				} else {
					Console.WriteLine ("Cannot find font, using default\nInstalled font list:");
					Console.WriteLine(installedFonts);
					Console.WriteLine();
				}
			}
			#endregion

			#region Find and generate anotated overview
			//find the minimap file first
			if(File.Exists(path +overviewDir+ map+".tga")){
				overview = Paloma.TargaImage.LoadTargaImage(path +overviewDir+ map+".tga");
				overview.MakeTransparent (overview.GetPixel(1,1));
				AnnotateOverview (ref overview, path+mapsDir+map+".level");

				//Check to make sure the map has not been saved since the minimap was created
				if(File.GetLastWriteTimeUtc(path +overviewDir+ map+".tga") < File.GetLastWriteTimeUtc(path+ mapsDir+map+".level")){
					Console.WriteLine ("WARNING: Level file updated more recently than overview");
					if (refreshMinimap && File.Exists("Overview.exe")) {
						Console.WriteLine("running: Overview.exe "+ path +mapsDir+map+".level "+path+overviewDir);
						System.Diagnostics.Process oProc =System.Diagnostics.Process.Start("Overview.exe", 
						                                                          string.Format("\"{0}\" \"{1}\"",
						              path+mapsDir+map+".level"
						              ,"ns2"));

						oProc.WaitForExit();

					}
					else{
							if(refreshMinimap){
						Console.WriteLine("Not in same directory as overview generator. Ignoring");
							}
					}
				}
			}
			else{
				Console.WriteLine("Cannot locate overview at:\n "+path +overviewDir+ map+".tga\n Are you sure it exists?");
				return;
			}
			#endregion

			//find the output dir
			if(!Directory.Exists(path+screensDir+map)){
				Directory.CreateDirectory(path+screensDir+map);
			}

			//list of files to read
			String[] toUse;

			if(!Directory.Exists(path+screensDir+map+screenSrcDir)){
				Console.WriteLine("Using generic screenshots");
				toUse = Directory.GetFiles(path+screensDir,"*.jpg");
				Randomizer r = new Randomizer ();
				toUse = r.shuffle (toUse);

			}
			else{
				toUse = Directory.GetFiles(path+screensDir+map+screenSrcDir,"*.jpg");
			}
			Console.WriteLine ("found "+toUse.Length);
			//draw the data you want
			if (toUse.Length > 0) {
				for (int i =0; i < 4; i++) {
					CreateAndSaveLoadScreen (i+1, 
					                        path + screensDir + map, 
					                       	ref overview,
					                        toUse [i%toUse.Length],
					                         map);
				}
			} else {
				Console.WriteLine ("no .jpgs found");
				return;
			}
			Console.WriteLine ("Complete.");
		}


		/// <summary>
		/// Creates a single loading slide
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="path">Path.</param>
		/// <param name="overview">Overview.</param>
		/// <param name="infile">Infile.</param>
		/// <param name="mapname">Mapname.</param>
		private static void  CreateAndSaveLoadScreen( int id, string path, ref Bitmap overview, string infile,String mapname){

			//Read the file in
			Bitmap tmp = new Bitmap (infile);
		
			//ensure that the file is big enough;
			if(tmp.Width < 1920){
			tmp = resizeImage (tmp,1920);
		
			}
			else {
				if(tmp.Width ==0){
				Console.WriteLine (infile+" Width was 0...");
				return;
				}
			}


			//show the overview
			Graphics g = Graphics.FromImage (tmp);
			g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
			g.DrawImage (overview, new Rectangle (minimapxoffset, minimapyoffset, minimapsize, minimapsize));

			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
		
			//draw the name of the map
			drawString (mapname.Replace("ns2_","").ToUpper(), g, bigFont, 320, 64,2.1f,6);

			//draw the hint box at teh bottom
			Color semitransparent = Color.FromArgb (128, 0, 0, 0);
			g.FillRectangle (new SolidBrush(semitransparent), new Rectangle (0, tmp.Height - 100, tmp.Width, 99));
		
			//draw teh vingette effect

			LinearGradientBrush gradientbrush1 = new LinearGradientBrush (
				new Rectangle (0, 0, 101, tmp.Height),
				Color.FromArgb (255, 0, 0, 0),
				Color.FromArgb (0, 0, 0, 0),
				LinearGradientMode.Horizontal);
			LinearGradientBrush gradientbrush2 = new LinearGradientBrush (
				new Rectangle (tmp.Width-150, 0, 150, tmp.Height),
				Color.FromArgb (0, 0, 0, 0),
				Color.FromArgb (255, 0, 0, 0),
				LinearGradientMode.Horizontal);
			LinearGradientBrush gradientbrush3 = new LinearGradientBrush (
				new Rectangle (0, 0, tmp.Width,51),
				Color.FromArgb (255, 0, 0, 0),
				Color.FromArgb (0, 0, 0, 0),
				LinearGradientMode.Vertical);
			g.FillRectangle (gradientbrush1, 0, 0, 100, tmp.Height);
			g.FillRectangle (gradientbrush2, tmp.Width-150, 0, 150, tmp.Height);
			g.FillRectangle (gradientbrush3, 0, 0, tmp.Width, 50);
			g.Flush ();

			g.Flush();
			//save the output.
			tmp.Save (String.Format("{0}/{1}.jpg",path,id));


			Console.Write (id +"...");
		}



		/// <summary>
		/// Resizes the image to match targeted sizes
		/// </summary>
		/// <returns>The image.</returns>
		/// <param name="img">Image.</param>
		/// <param name="width">Width.</param>
		public static Bitmap resizeImage(Bitmap img, int width){
			if (img.Width == 0) {
				return img;
			}
			double scale = 1/((double)img.Width / width);

			Bitmap outImg = new Bitmap(width, (int)(img.Height*scale),System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(outImg);


			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			g.SmoothingMode =  System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			//draw the tip window
			g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, outImg.Width, outImg.Height));
			g.DrawImage(img, new Rectangle(0,0, outImg.Width, outImg.Height));



			return outImg;
		}


		/// <summary>
		/// Adds things to overview
		/// </summary>
		/// <param name="overview">Overview.</param>
		/// <param name="file">File.</param>
		static void AnnotateOverview (ref Bitmap overview, string file)
		{
			List<RectangleF> textBlocks= new List<RectangleF>();

			//TODO
			/*
			 * to get this working we need to open the map file, get the minimap exetents, map that to the minimap.tga
			 * 
			 * iterate teh tech points,
			 * iterate the resource towers,
			 * then we need to iterate the locations and draw the names
			 */

			NS2.Tools.Level lvl = new NS2.Tools.Level (file);

			Console.WriteLine (lvl.minimap_extents);
			Console.WriteLine (lvl.Textures.Length+" TEXTures");

			Console.WriteLine (lvl.TechPoints.Count+" TP");
			Console.WriteLine (lvl.Resources.Count+" RT");
			Console.WriteLine (lvl.Locations.Count+" loc");


			Graphics g = Graphics.FromImage (overview);
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

			g.TranslateTransform (overview.Width/2f , overview.Height/2f);
			g.RotateTransform (-90f);

			Console.WriteLine ("Scale:"+(overview.Width/lvl.minimap_extents.Scale.X)+"\n"+overview.Height/lvl.minimap_extents.Scale.Z); 

			//ahhhh
			if(Math.Abs(lvl.minimap_extents.Scale.X - lvl.minimap_extents.Scale.Z) > 10){
				if(lvl.minimap_extents.Scale.Z >lvl.minimap_extents.Scale.X){
					g.ScaleTransform(lvl.minimap_extents.Scale.X/lvl.minimap_extents.Scale.Z,1);
				}
				else{
					g.ScaleTransform(1,lvl.minimap_extents.Scale.Z/lvl.minimap_extents.Scale.X);
				}
			}


			NS2.Tools.Vector3 vec;
			g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

			//Draw techpoints
			foreach (NS2.Tools.Entity e in lvl.TechPoints) {
				vec = lvl.minimapLocation (e.Origin, overview.Width);
	//			//g.FillRectangle(Brushes.Gold, e.Origin.X, e.Origin.Z, 8f,8f);
	//			g.FillRectangle(Brushes.Blue, vec.X-16, vec.Z-16, 32f,32f);
				g.DrawImage (tp_icon, new RectangleF(vec.X-16f, vec.Z-16f, 32f, 32f));
			}

			//Draw Rts
			//REMOVED FOR NOW.
		/*	foreach (NS2.Tools.Entity e in lvl.Resources) {
				vec = lvl.minimapLocation (e.Origin, overview.Width);
	//			Console.WriteLine (e.Origin + " =>" + vec);
				g.FillEllipse(Brushes.Red, vec.X-16, vec.Z-16, 32f,32f);
				//	g.FillRectangle
			}*/
			g.RotateTransform (90);
			//Draw location names
			foreach (NS2.Tools.Entity e in lvl.Locations) {
				vec = lvl.minimapLocation (e.Origin, overview.Width);
				var size = g.MeasureString (e.Text,minimapFont);

				int startX=(int)(vec.Z-size.Width/2);
				int startY=(int)( -vec.X - size.Height / 2);
				int lastAdjust = 0;
				RectangleF box = new RectangleF (startX,startY, size.Width, size.Height);

				for(int  i = 0; i <textBlocks.Count ;i++) {
					if (textBlocks[i].IntersectsWith (box)) {

						if (lastAdjust == 0) {
					
							box.Y -= 1;
						} else if (lastAdjust == 1) {
							box.Y += 1;
						} else if (lastAdjust == 2) {
							box.X -= 1;
						} else if (lastAdjust>=3){
							box.X+=1;
						}
						i = 0;
						if(lastAdjust == 0 && startY > box.Y + box.Height)
						{
							lastAdjust =1;
							box.Y = startY;
						}
						if(lastAdjust == 1 && startY < box.Y - box.Height)
						{
							lastAdjust =2;
							box.Y = startY;
						}
						if(lastAdjust == 2 && startX > box.X - box.Height)
						{
							lastAdjust =3;
							box.X = startY;
						}
						if(lastAdjust == 3 && startX < box.X - box.Height)
						{
							lastAdjust =4;
							i += textBlocks.Count;
							box.X=startX;
							box.Y= startY;
				
							Console.WriteLine("I hope "+e.Text+" is not overlapping");
						}

					}
				}
				//g.DrawRectangle (Pens.Red, box.X, box.Y, box.Width, box.Height);

				drawString (e.Text, g, minimapFont, box.X, box.Y,1,0);
				textBlocks.Add (box);
			}

			g.Flush ();
		}


		/// <summary>
		/// Draws the string using ana outline and supposts basic eeffects
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="g">The green component.</param>
		/// <param name="f">F.</param>
		/// <param name="centerx">Centerx.</param>
		/// <param name="centery">Centery.</param>
		/// <param name="margin">Margin.</param>
		/// <param name="glowWidth">Glow width.</param>
		public static  void drawString(String text, Graphics g, Font f, float centerx, float centery, float margin, float glowWidth){
			float scalingfortext = 395f / 300f;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			GraphicsPath gp = new GraphicsPath ();

			gp.AddString(text, f.FontFamily,
			             (int)FontStyle.Regular,
			             f.Size*scalingfortext,
			             new PointF(centerx, centery),
			             new StringFormat());

			if (glowWidth > 0) {
				for (int i = 0; i < glowWidth*2; i++) {
					g.DrawPath (new Pen (Color.FromArgb(16,255,255,255), i),gp);
				}

			}
			if (margin >= 1) {
				g.DrawPath (new Pen(Color.Black, margin*2), gp);
				g.FillPath (Brushes.Black, gp);
			}
			g.FillPath (Brushes.White, gp);

		
			/*g.DrawString (text, new Font(f.FontFamily, f.Size+margin), Brushes.Black, new PointF (centerx-margin, centery));

			g.DrawString (text, f, Brushes.White, new PointF (centerx-1, centery));*/


		}

		static void ReadConfigFile ()
		{
			string key = "";
			string data = "";
			String[] configData=null;
			int idx;
			try{
			 configData = File.ReadAllLines (configFile);
			}
			catch(Exception ex){
				Console.WriteLine ("WARNING: cannot load config, "+ex.Message+", using defaults");
				return;
			}
			foreach (String line in configData) {
				idx = line.IndexOf ("#");
				if ( idx>= 0) {
					key = line.Substring(0, idx);
				}
				idx = key.IndexOf ("=");
				if (key.Length > 3 & idx >= 0) {
					data = key.Substring (idx+1).Trim();

					key = key.Substring (0, idx).Trim().ToLower();
					ProcessConfig(key,data);
				}

			}
		}

		static void ProcessConfig(string key, string data){
			//TODO data validation?
			switch(key){
			case "fontdir":
				fontDir = data;
				break;
			case "mapsdir":
				mapsDir = data;
				break;
			case "overviewdir":
				overviewDir = data;
				break;
			case "screensdir":
				screensDir = data;
				break;
			case "screensrcdir":
				screenSrcDir = data;
				break;
			case "bigfontsize":
				float.TryParse (data, out bigFontSize);
				break;
			case "minimapfontsize":
				float.TryParse (data, out minimapFontSize);
				break;
			case "icon_tp":
				tpIconPath = data;
				break;
			case "icon_rp":
				rpIconPath = data;
				break;
			case "refreshminimap":
				refreshMinimap = data == "true";
				break;
			case "minimapxoffset":
				int.TryParse (data, out minimapxoffset);
				break;
			case "minimapyoffset":
				int.TryParse (data, out minimapyoffset);
				break;
			case "minimapsize":
				int.TryParse (data, out minimapsize);
				break;
			case "bigtexty":
				int.TryParse (data, out bigTextY);
				break;
			case "bigtextx":
				int.TryParse (data, out bigTextX);
				break;
			default:
				Console.WriteLine ("Invalid Config key: "+key);
				break;


			}

		}

	}
}
