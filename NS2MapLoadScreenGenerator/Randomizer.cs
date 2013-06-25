using System;
using System.Collections;
using System.Collections.Generic;

namespace NS2MapLoadScreenGenerator
{
	public class Randomizer
	{
		Random r= new Random();
		public string[] shuffle(string[] input){
			List<KeyValuePair<int,string>> items = new List<KeyValuePair<int,string>> ();
			foreach (string s in input) {
				items.Add(new KeyValuePair<int, string>(r.Next(input.Length), s));
			}

			items.Sort (new Comp());
			List<string> output = new List<string> ();
			foreach (KeyValuePair<int,string> k in items) {
				//Console.WriteLine (k.Key);
				output.Add (k.Value);
			}

			return output.ToArray ();
		}


		private class Comp:IComparer<KeyValuePair<int,string>>{
		
			public int Compare (KeyValuePair<int,string> x, KeyValuePair<int,string> y)
			{
				return(x).Key - (y).Key;

			}

		


		}
	

	}
}

