using System;
using System.Collections;

namespace NS2MapLoadScreenGenerator
{
	public class Randomizer:IComparer
	{
		Random r= new Random();
		#region IComparer implementation

		int IComparer.Compare (object x, object y)
		{
			return r.Next (-5, 5);
		}

		#endregion


	}
}

