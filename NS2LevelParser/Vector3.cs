using System;

namespace NS2.Tools
{
	public struct Vector3
	{
		public float X, Y, Z;

		public Vector3 (float x, float y, float z)
		{
			X= x;
			Y= y;
			Z=z;
		}
		public Vector3(Byte[] bytes){
			X = System.BitConverter.ToSingle (bytes, 0);;
			Y = System.BitConverter.ToSingle (bytes, 4);
			Z = System.BitConverter.ToSingle (bytes, 8);
		}
		public override String ToString(){
			return String.Format ("({0}, {1}, {2})",X,Y,Z);
		}
	}
}

