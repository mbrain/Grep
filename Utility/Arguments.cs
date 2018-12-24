using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace CommandLine.Utility{

	public class Arguments{

		private HybridDictionary Parameters;

		public Arguments(string[] Args){

			Parameters=new HybridDictionary();
			Regex Spliter=new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase|RegexOptions.Compiled);
			Regex Remover= new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase|RegexOptions.Compiled);
			string Parameter=null;
			string[] Parts;

			foreach(string Txt in Args){

				Parts=Spliter.Split(Txt,3);
				switch(Parts.Length){

					case 1:
						if(Parameter!=null){
							if(!Parameters.Contains(Parameter)){
								Parts[0]=Remover.Replace(Parts[0],"$1");
								Parameters.Add(Parameter,Parts[0]);
								}
							Parameter=null;
							}

						break;

					case 2:

						if(Parameter!=null){
							if(!Parameters.Contains(Parameter)) Parameters.Add(Parameter,"true");
							}
						Parameter=Parts[1];
						break;

					case 3:

						if(Parameter!=null){
							if(!Parameters.Contains(Parameter)) Parameters.Add(Parameter,"true");
							}
						Parameter=Parts[1];

						if(!Parameters.Contains(Parameter)){
							Parts[2]=Remover.Replace(Parts[2],"$1");
							Parameters.Add(Parameter,Parts[2]);
							}
						Parameter=null;
						break;
					}
				}

			if(Parameter!=null){
				if(!Parameters.Contains(Parameter)) Parameters.Add(Parameter,"true");
				}
			}

		public object this [string Param]{
			get{
				return (Parameters[Param]);
				}
			}
		}
	}
