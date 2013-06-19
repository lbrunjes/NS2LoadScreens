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
generator.exe ns2_map_name ns2dir

TO DO:
show RT,TP, locations on minimap
";
		const string mapsDir = "ns2/maps/";
		const string overviewDir= "ns2/maps/overviews/";
		const string screensDir = "ns2/screens/";
		const string screenSrcDir = "/src/";

		static Font bigFont = new Font (FontFamily.GenericSansSerif, 96f);
//		static Font bigFont = new Font (FontFamily.GenericSansSerif, 94f);
		static Font minimapFont = new Font(FontFamily.GenericSansSerif,14f);

		static int minimapxoffset = 1120;
		static int minimapyoffset = 0;
		static int minimapsize =640;

		static Bitmap tp_icon = new Bitmap ("icon_techpoint.png");
		static Bitmap rt_icon;

		public static void Main (string[] args)
		{

			String map = "ns2_test";
			String path = "C:/Program Files (x86)/Steam/steamapps/common/Natural Selection 2/";
			Bitmap overview;

			//parse command line args
			if (args.Length > 0) {
				map= args[0];
			}

			if (args.Length > 1) {
				path= args[1];
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

			//TODO LOAD FONTS FROM ../core/fonts/
			//todo see if the font is intsalled
			PrivateFontCollection pfc = new PrivateFontCollection ();


			if (File.Exists (System.Environment.CurrentDirectory+"/fonts/AgencyFB-Bold.ttf")) {
				pfc.AddFontFile (System.Environment.CurrentDirectory+"/fonts/AgencyFB-Bold.ttf");
				bigFont = new Font (pfc.Families [0], 96f);
				minimapFont = new Font (pfc.Families [0], 36f);
			} else {
				Console.WriteLine ("Cannot find font, using defaults sorry");
			}
			//find the minimap file first
			if(File.Exists(path +overviewDir+ map+".tga")){
				overview = Paloma.TargaImage.LoadTargaImage(path +overviewDir+ map+".tga");
				overview.MakeTransparent (overview.GetPixel(1,1));
				AnnotateOverview (ref overview, path+mapsDir+map+".level");
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



			g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, outImg.Width, outImg.Height));
			g.DrawImage(img, new Rectangle(0,0, outImg.Width, outImg.Height));

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
		//	lvl.GetEntitiesWithName ("minimap_extents");
			Console.WriteLine (lvl.minimap_extents);
			Console.WriteLine (lvl.Textures.Length+" TEXTures");

			Console.WriteLine (lvl.TechPoints.Count+" TP");
			Console.WriteLine (lvl.Resources.Count+" RT");
			Console.WriteLine (lvl.Locations.Count+" loc");


			Graphics g = Graphics.FromImage (overview);

			//g.DrawLine (Pens.Red, 0, overview.Height / 2, overview.Width, overview.Height / 2);
			//g.DrawLine (Pens.Red, overview.Width/2, 0, overview.Width/2, overview.Height );

			g.TranslateTransform (overview.Width/2f , overview.Height/2f);
			g.RotateTransform (-90f);
			//g.ScaleTransform (overview.Width/(lvl.minimap_extents.Scale.X/2), overview.Width / (lvl.minimap_extents.Scale.Z/2));
			//g.TranslateTransform (-lvl.minimap_extents.Origin.X, -lvl.minimap_extents.Origin.Z);


			//g.DrawLine (Pens.Yellow, -overview.Width, 0, overview.Width, 0);
			//g.DrawLine (Pens.Yellow, 0, -overview.Height, 0, overview.Height );




			Console.WriteLine ("Scale:"+(overview.Width/lvl.minimap_extents.Scale.X));
			//g.TranslateTransform (, lvl.minimap_extents.Origin.Y);


			//g.FillEllipse (Brushes.Red, lvl.minimap_extents.Origin.X, lvl.minimap_extents.Origin.Z, 10f, 10f);
			//g.FillEllipse (Brushes.Peru, 0, 0, 10, 10);
			NS2.Tools.Vector3 vec;
			g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

			//Draw techpoints
			foreach (NS2.Tools.Entity e in lvl.TechPoints) {
				vec = lvl.minimapLocation (e.Origin, overview.Width);
	//			Console.WriteLine (e.Origin + " =>" + vec);
				//g.FillRectangle(Brushes.Gold, e.Origin.X, e.Origin.Z, 8f,8f);
	//			g.FillRectangle(Brushes.Blue, vec.X-16, vec.Z-16, 32f,32f);
				g.DrawImage (tp_icon, new RectangleF(vec.X-16f, vec.Z-16f, 32f, 32f));


			//	g.FillRectangle
			}

			//Draw Rts
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
		//		Console.WriteLine (e.Origin + " =>" + vec);
			//	Console.WriteLine (e.Text+ " "+ e.Origin);
				var size = g.MeasureString (e.Text,minimapFont);
				drawString (e.Text, g, minimapFont, vec.Z-size.Width/2 +32f, -vec.X-size.Height/2,1);
				//	g.FillRectangle
			}

			g.Flush ();
		}

		public static  void drawString(String text, Graphics g, Font f, float centerx, float centery, float margin){
			GraphicsPath gp = new GraphicsPath ();

			gp.AddString(text, 
			             f.FontFamily,
			             (int)FontStyle.Regular,
			             f.Size,
			             new PointF(centerx, centery),
			             new StringFormat()
			            );
			g.FillPath (Brushes.White, gp);
			g.DrawPath (new Pen(Color.Black, margin), gp);

		}
	}
}
