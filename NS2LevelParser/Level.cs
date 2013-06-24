using System;
using System.IO;
using System.Collections.Generic;

namespace NS2.Tools
{
	public class Level
	{
		public String[] Textures;

		public Entity minimap_extents;
		public List<Entity> Locations = new List<Entity> ();
		public List<Entity> Resources = new List<Entity> ();
		public List<Entity> TechPoints = new List<Entity> ();

		public int sectionEnds = 0xffffff;
		public byte[] headerStart ={0x4c,0x56,0x4c,0x09,0x02};
		public int headerLength =6;
		public int LevelSpecVersion = 0;

		public int internalSeparator= 0x000000;

		public Vector3 getLocationName(string name){
			int num = 0;
			Vector3 ret = new Vector3 (0,0,0);

			foreach (Entity e in Locations) {
				if (e.Text == name) {
					ret.X += e.Origin.X;
					ret.Y += e.Origin.Y;
					ret.Z += e.Origin.Z;
					num++;
				}

			}

			if (num > 1) {
				ret.X = ret.X / num;
				ret.Y = ret.Y / num;
				ret.Z = ret.Z / num;
			}

			return ret;


		}

		public Level(String FileName){
			byte[] tmp=File.ReadAllBytes (FileName);
			int idx = 0;;
				
			//read and verify the header
			
			bool okay = true;
			for(int i =0; i < headerStart.Length;i++){
				okay = (tmp[i]!=headerStart[i])? false:okay;
			}

			if (!okay) {
				throw new InvalidDataException("file not a valid ns2level");
			}

			//There is some data here i dont know what is
			idx = headerStart.Length + 17;


			//read texture strings
			List<String> tx = new List<String> ();
			String current = "";
		
			while (!(tmp[idx] == 0xff && tmp[idx+1] ==0xff && tmp[idx+2]==0xff && tmp[idx+3]==0xff && Char.IsLetter((char)tmp[idx+8]))) {
				if (tmp[idx] == 1 ||(int)tmp[idx] == 0) {
					if (current.Length > 0 && current.Contains("material")) {
						current = current.Trim ();
						tx.Add (current);
					//	Console.WriteLine (current);
					}
					current = "";
				}
				current +=  (char)tmp[idx];
				idx++;
			}

			this.Textures = tx.ToArray ();

			//we should be at a FF FF FF FF that starts a mystery block
			//probably shapes
	//		Console.WriteLine ("end texture "+idx.ToString("x"));
			idx += 4;

		//	while (!(tmp[idx] == 0xff && tmp[idx+1] ==0xff && tmp[idx+2]==0xff && tmp[idx+3]==0xff)) {
		//		Console.Write (tmp[idx]);
			//	Console.Write((char)tmp[idx]);
		//		idx++;
		//	}

		//	idx += 4;


	//		Console.WriteLine ("checking ents " +idx.ToString("x"));
			//Read entites in.
			List<byte> ent = new List<byte> ();
			while (!(tmp[idx] == 0x3c && tmp[idx+2] == 0x3f  && tmp[idx+4] == 0x78  && tmp[idx+6] == 0x6d && tmp[idx+8] == 0x6c)) {
				if (tmp[idx] == 0xff && tmp[idx+1] ==0xff && tmp[idx+2]==0xff && tmp[idx+3]==0xff) {
					idx+= 4;

					//save the stuff
					this.AddEntity(ent.ToArray());
					ent.Clear ();
			///		Console.WriteLine ("addedent");

				}

				ent.Add (tmp[idx]);
				idx++;

			}

			//at this point there ix xml data.
		//	Console.WriteLine ("XML Begins at " +idx.ToString("x"));

			//todo get this xml document

			//setup average points for all names
			List<Entity> loc = new List<Entity> ();
			List<String> names = new List<String> ();
			foreach (Entity e in this.Locations) {
				if(!names.Contains(e.Text)){
					e.Origin = getLocationName (e.Text);
					names.Add (e.Text);
					loc.Add (e);
				}
			}
			this.Locations = loc;

		}
		public Vector3 minimapLocation(Vector3 loc, int size){
			//to get this we add the
			if( this.minimap_extents == null || size ==0){
				return  loc;
			}

			loc.X -= minimap_extents.Origin.X;
			loc.Y -= minimap_extents.Origin.Y;
			loc.Z -= minimap_extents.Origin.Z;



		

			float scaleX = (float) size/(minimap_extents.Scale.X/2);
			float scaleZ = (float) size/(minimap_extents.Scale.Z/2) ;

	//		Console.WriteLine (scaleX);

			loc.X = loc.X * scaleX;//+ minimap_extents.Scale.X/2*scaleX;
			loc.Z = loc.Z * scaleZ;;// + minimap_extents.Scale.Z/2*scaleZ;



			return loc;

		}

		void AddEntity (byte[] @data)
		{
			try{

			string name= "";
			Vector3 origin;
			Vector3 angles;
			Vector3 scale;
			int i = 4;
		//	Console.WriteLine (@data.Length);
			while (data [i] != 0x02) {
				name += (char)@data [i];
				i++;
			}

			name = name.Trim ();
			//there are two bytes here that might mean something
			i += 30; //get to data

			byte[] threefloats = new byte[12];
			for(int j =0; j< threefloats.Length;j++){
				threefloats [j] = @data[i+j];
			}
			origin = new Vector3(threefloats);

			i+=threefloats.Length+30; //get to data

			for(int j =0;j< threefloats.Length;j++){
				threefloats [j] = @data[i+j];
			}
			angles = new Vector3(threefloats);

			i+=threefloats.Length+29; //get to data

			for(int j =0;j < threefloats.Length;j++){

				threefloats [j] = @data[i+j];
			}
			scale = new Vector3(threefloats);

			//if we are a location continue.
			string locname = "";
			if (name == "location") {

				i += 33;
				while (@data[i] !=0x02) {
					if (@data [i] != 0x00 && !char.IsControl((char)@data[i])) {
						locname += (char)@data [i];
					}

					i++;
				}
				locname = locname.Trim();
				//todo show in minimap
					//TODO this index is wrong
					//12 to the next tag.
					//13 in sho on minimap
					//12 to data (4 for num 4 for type 4th byte length
			
					i+=25;

					/*for(int k =0 ;k < 20;k++){
						Console.Write(@data[i+k].ToString("x")+"\t");
					}*/
					//Console.WriteLine(@data[i]-25);

				if (@data [i] == 0x00) {
					locname ="";
				}
			}
	//		Console.WriteLine (name);
			switch (name) {
			case "minimap_extents":
				this.minimap_extents = new Entity ("", origin, angles, scale);
				break;
			case "location":
				this.Locations.Add (new Entity (locname, origin, angles, scale));
				break;
			case "tech_point":
				this.TechPoints.Add (new Entity ("", origin, angles, scale));
				break;
			case "resource_point":
				this.Resources.Add (new Entity ("", origin, angles, scale));
				break;

			default:
				//Console.WriteLine ("unsupported entity "+name);
				break;


			}
			}
			catch(Exception ex){
				Console.WriteLine (ex.Message);
			}
		}
	}
}

