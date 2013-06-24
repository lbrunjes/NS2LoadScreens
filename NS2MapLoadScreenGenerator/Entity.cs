using System;

namespace NS2.Tools
{
	public class Entity
	{
		public Vector3 Origin,Angle,Scale;
		public string Text="";
		public Entity (String text, Vector3 origin, Vector3 angle, Vector3 scale)
		{
			Text = text;
			Origin = origin;
			Angle = angle;
			Scale = scale;
		}
		public override String ToString(){
			return String.Format ("{0} o:{1}, a:{2}, s:{3}",Text, Origin,Angle,Scale);
		}
	}
}

