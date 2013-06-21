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

namespace NS2MapLoadScreenGenerator
{
	class MainClass
	{
		public static String instructions = @" 
This take a map name, and a path it then loads the minimap for the
relvevant directory:ns2\maps\overviews. It attaches the minimap 
to screenshot jpgs found inns2\screens or ns2\screens\%map%\src
and outputs jpegs with the screen and the minimap to:
ns2\screens\%map%

use:
generator.exe ns2_map_name [font] [refreshMinimap]

TO DO:
show RT,TP, locations on minimap
";
		const string mapsDir = "ns2/maps/";
		const string overviewDir= "ns2/maps/overviews/";
		const string screensDir = "ns2/screens/";
		const string screenSrcDir = "/src/";
		const float bigFontSize = 96f;
		const float minimapFontSize = 32f;
		static Font bigFont = new Font (FontFamily.GenericSansSerif, bigFontSize);
//		static Font bigFont = new Font (FontFamily.GenericSansSerif, 94f);
		static Font minimapFont = new Font(FontFamily.GenericSansSerif,minimapFontSize);

		static int minimapxoffset = 1120;
		static int minimapyoffset = 0;
		static int minimapsize =640;

		static Bitmap tp_icon = new Bitmap ("icon_techpoint.png");
		static Bitmap rt_icon;


		public static void Main (string[] args)
		{

			bool refreshMinimap = false;

			String map = "ns2_test";
			String path = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Natural Selection 2\\";
			String font = "AgencyFB-Regular";
			Bitmap overview;

			#region parse command line args & load things
			if (args.Length > 0) {
				map= args[0].Replace(".level","");
			}

			if (args.Length > 1) {
				font= args[1];
			}

			if(args.Length > 2){
				refreshMinimap = args[2].ToLower() == "true" || args[2].ToLower()=="yes";
			}
			Console.WriteLine (String.Format(@"NS2 Map Load Screen generator
---------------------------------
 {0}

Level:{1}
Path :{2}
---------------------------------",
			                                instructions, 
			                                 map,
			                                 path));
			// if we aint got a mp no point in going forward.
			if(map == ""){
				return;
			}

			tp_icon.MakeTransparent ();
			#endregion

			#region check fonts
			bool fontFound = false;

			InstalledFontCollection ifc = new InstalledFontCollection ();
			foreach (FontFamily fontfamily in ifc.Families) {
				if (fontfamily.Name == font) {
					bigFont = new Font (fontfamily, 96f);
					minimapFont = new Font (fontfamily, 36f);
					fontFound = false;
				}
			}
			if (!fontFound) {
				PrivateFontCollection pfc = new PrivateFontCollection ();


				if (File.Exists (System.Environment.CurrentDirectory+"/fonts/AgencyFB-Bold.ttf")) {
					pfc.AddFontFile (System.Environment.CurrentDirectory+"/fonts/AgencyFB-Bold.ttf");
					bigFont = new Font (pfc.Families [0], bigFontSize);
					minimapFont = new Font (pfc.Families [0], minimapFontSize);
				} else {
					Console.WriteLine ("Cannot find font, using defaults sorry");
				}
			}
			#endregion

			//find the minimap file first
			if(File.Exists(path +overviewDir+ map+".tga")){
				overview = Paloma.TargaImage.LoadTargaImage(path +overviewDir+ map+".tga");
				overview.MakeTransparent (overview.GetPixel(1,1));
				AnnotateOverview (ref overview, path+mapsDir+map+".level");

				//Check to make sure the map has not been saved since the minimap was created
				if(File.GetLastWriteTimeUtc(path +overviewDir+ map+".tga") < File.GetLastWriteTimeUtc(path+ mapsDir+map+".level")){
					Console.WriteLine ("WARNING: Level file updated more recently than overview");
					if (refreshMinimap && File.Exists("minimap.exe")) {
						System.Diagnostics.Process.Start ("minimap.exe", map);
					}
				}
			}
			else{
				Console.WriteLine("Cannot locate overview at:\n "+path +overviewDir+ map+".tga\n Are you sure it exists?");
				return;
			}
		

			//find the output dir
			if(!Directory.Exists(path+screensDir+map)){
				Directory.CreateDirectory(path+screensDir+map);
			}

			//list of files to read
			String[] toUse;

			if(!Directory.Exists(path+screensDir+map+screenSrcDir)){
				Console.WriteLine("Using generic screenshots");
				toUse = Directory.GetFiles(path+screensDir,"*.jpg");
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
			drawString (mapname.Replace("ns2_","").ToUpper(), g, bigFont, 320, 64,3);

			//draw the hint box at teh bottom
			Color semitransparent = Color.FromArgb (128, 0, 0, 0);
			g.FillRectangle (new SolidBrush(semitransparent), new Rectangle (0, tmp.Height - 100, tmp.Width, 99));


			g.Flush();
			//save the output.
			tmp.Save (String.Format("{0}/{1}.jpg",path,id));


			Console.Write (id +"...");
		}




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

			//draw teh vingette effect
		
			LinearGradientBrush gradientbrush1 = new LinearGradientBrush (
				new Rectangle (0, 0, 100, outImg.Height - 100),
				Color.FromArgb (255, 0, 0, 0),
				Color.FromArgb (0, 0, 0, 0),
				LinearGradientMode.Horizontal);
			LinearGradientBrush gradientbrush2 = new LinearGradientBrush (
				new Rectangle (outImg.Width-150, 0, 150, outImg.Height - 100),
				Color.FromArgb (0, 0, 0, 0),
				Color.FromArgb (255, 0, 0, 0),
				LinearGradientMode.Horizontal);
			g.FillRectangle (gradientbrush1, 0, 0, 100, outImg.Height - 100);
			g.FillRectangle (gradientbrush2, outImg.Width-150, 0, 150, outImg.Height - 100);
			g.Flush ();

			return outImg;
		}



		static void AnnotateOverview (ref Bitmap overview, string file)
		{
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

			//Console.WriteLine ("Scale:"+(overview.Width/lvl.minimap_extents.Scale.X));

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
				drawString (e.Text, g, minimapFont, vec.Z-size.Width/2 +32f, -vec.X-size.Height/2,1);
				//	g.FillRectangle
			}

			g.Flush ();
		}


		/// <summary>
		/// Draws the string to teh graphics context with an outline.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="g">The green component.</param>
		/// <param name="f">F.</param>
		/// <param name="centerx">Centerx.</param>
		/// <param name="centery">Centery.</param>
		/// <param name="margin">Margin.</param>
		public static  void drawString(String text, Graphics g, Font f, float centerx, float centery, float margin){
			GraphicsPath gp = new GraphicsPath ();

			gp.AddString(text, f.FontFamily,
			             (int)FontStyle.Regular, f.Size,
			             new PointF(centerx, centery),
			             new StringFormat());

			g.FillPath (Brushes.White, gp);
			if (margin >= 1) {
				g.DrawPath (new Pen(Color.Black, margin), gp);
			}

		}
	}
}
