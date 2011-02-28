/**
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Kajabity.Tools.Java;
using SimpleOX.Excel;


// TODO: think about rewriting the "Java tools" so that it doesn't have to 
// be distributed with the software
namespace Lucene.Net.Benchmark.ByTask.Utils {
	/**
	 * Perf run configuration properties.
	 * <p>
	 * Numeric property containing ":", e.g. "10:100:5" is interpreted 
	 * as array of numeric values. It is extracted once, on first use, and 
	 * maintain a round number to return the appropriate value.
	 * <p>
	 * The config property "work.dir" tells where is the root of 
	 * docs data dirs and indexes dirs. It is set to either of: <ul>
	 * <li>value supplied for it in the alg file;</li>
	 * <li>otherwise, value of System property "benchmark.work.dir";</li>
	 * <li>otherwise, "work".</li>
	 * </ul>
	 */
	public class Config {

		private static readonly String NEW_LINE = System.Environment.NewLine;
		
		private int roundNumber = 0;
		private JavaProperties props;
		private Hashtable valByRound = new Hashtable();
		private Hashtable colForValByRound = new Hashtable();
		private String algorithmText;

		/**
		* Read both algorithm and config properties.
		* @param algReader from where to read algorithm and config properties.
		* @throws IOException
		*/
		public Config (StreamReader algReader) {
			// read alg file to array of lines
			ArrayList lines = new ArrayList();
			TextReader tr = algReader;
			int lastConfigLine=0;
			for (String line = tr.ReadLine(); line!=null; line=tr.ReadLine()) {
				lines.Add(line);
				if (line.IndexOf('=')>0) {
					lastConfigLine = lines.Count;
				}
			}
			tr.Close();
			// copy props lines to string
			StringBuilder sb = new StringBuilder();
			for (int i=0; i<lastConfigLine; i++) {
				// as we're doing this appending, we want to make a couple
				// of strategic substitutions so we can hopefully read in
				// config files meant for the java version of Benchmark
				string line = (string)lines[ i ];
				line = Regex.Replace(line, "org\\.apache\\.lucene\\.analysis\\.standard.", "Lucene.Net.Analysis.Standard.");
				line = Regex.Replace( line, "org\\.apache\\.lucene\\.analysis.", "Lucene.Net.Analysis." );
				line = Regex.Replace( line, "org\\.apache\\.lucene\\.benchmark\\.byTask\\.feeds\\.", "Lucene.Net.Benchmark.ByTask.Feeds." );
				sb.Append(line);
				sb.Append(NEW_LINE);
			}
			// read props from string
			// this has changed to use the JavaProperties class - Jason Ramsey 1/27/11
			this.props = new JavaProperties();
			byte[] byteArray = Encoding.ASCII.GetBytes( sb.ToString() );
			MemoryStream stream = new MemoryStream( byteArray ); 
			props.Load(stream);

			// make sure work dir is set properly 
			 if (props.GetProperty("work.dir")==null) {
				props.SetProperty("work.dir", (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "work")));
			}

			if (bool.Parse(props.GetProperty("print.props","true"))) {
				printProps();
			}
    
			// copy algorithm lines
			sb = new StringBuilder();
			for (int i=lastConfigLine; i<lines.Count; i++) {
				sb.Append(lines[i]);
				sb.Append(NEW_LINE);
			}
			algorithmText = sb.ToString();
		}

		/**
		* Create config without algorithm - useful for a programmatic perf test.
		* @param props - configuration properties.
		* @throws IOException
		*/
		public Config (JavaProperties props) {
			this.props = props;
			if (bool.Parse(props.GetProperty("print.props","true"))) {
				printProps();
			}
		}

		private void printProps() {
			int sheetIndex = Benchmark.ExcelDoc.CreateDataSheet("Props");
			DataSheet ds = (DataSheet)Benchmark.ExcelDoc.Sheets[ sheetIndex ];
			ds.AddRowAndCell( "------------> config properties:" );
			Console.WriteLine("------------> config properties:");
			ArrayList propKeys = new ArrayList(props.Keys);
			propKeys.Sort();
			foreach (string propName in propKeys)
			{
				ds.AddRowAndCell( propName + " = " + props.GetProperty( propName ) );
				Console.WriteLine(propName + " = " + props.GetProperty(propName));
			}
			ds.AddRowAndCell( "-------------------------------" );
			Console.WriteLine("-------------------------------");
		}

		/**
		* Return a string property.
		* @param name name of property.
		* @param dflt default value.
		* @return a string property.
		*/
		public String get (String name, String dflt) {
			return props.GetProperty(name,dflt);
		}

		/**
		* Set a property.
		* Note: once a multiple values property is set, it can no longer be modified.
		* @param name name of property.
		* @param value either single or multiple property value (multiple values are separated by ":")
		* @throws Exception 
		*/
		public void Set (String name, String value) {
			if( valByRound[ name ] != null ) {
				throw new Exception( "Cannot modify a multi value property!" );
			}
			else
			{
				props.SetProperty(name, value);
			}
		}

		/**
		* Return an int property.
		* If the property contain ":", e.g. "10:100:5", it is interpreted 
		* as array of ints. It is extracted once, on first call
		* to get() it, and a by-round-value is returned. 
		* @param name name of property
		* @param dflt default value
		* @return a int property.
		*/
		public int get (String name, int dflt) {
			// use value by round if already parsed
			int[] vals = (int[]) valByRound[name];
			if (vals != null) {
				return vals[roundNumber % vals.Length];
			}
			// done if not by round 
			String sval = props.GetProperty(name,""+dflt);
			if (sval.IndexOf(":")<0) {
				return Int32.Parse( sval );
			}
			// first time this prop is extracted by round
			int k = sval.IndexOf(":");
			String colName = sval.Substring(0,k);
			sval = sval.Substring(k+1);
			colForValByRound[name] = colName;
			vals = propToIntArray(sval);
			valByRound[name] = vals;
			return vals[roundNumber % vals.Length];
		}
  
		/**
		* Return a double property.
		* If the property contain ":", e.g. "10:100:5", it is interpreted 
		* as array of doubles. It is extracted once, on first call
		* to get() it, and a by-round-value is returned. 
		* @param name name of property
		* @param dflt default value
		* @return a double property.
		*/
		public double get (String name, double dflt) {
			// use value by round if already parsed
			double[] vals = (double[]) valByRound[name];
			if (vals != null) {
				return vals[roundNumber % vals.Length];
			}
			// done if not by round 
			String sval = props.GetProperty(name,""+dflt);
			if (sval.IndexOf(":")<0) {
				return Double.Parse(sval);
			}
			// first time this prop is extracted by round
			int k = sval.IndexOf(":");
			String colName = sval.Substring(0,k);
			sval = sval.Substring(k+1);
			colForValByRound[name] = colName;
			vals = propToDoubleArray(sval);
			valByRound[name] = vals;
			return vals[roundNumber % vals.Length];
		}
  
		/**
		* Return a bool property.
		* If the property contain ":", e.g. "true.true.false", it is interpreted 
		* as array of booleans. It is extracted once, on first call
		* to get() it, and a by-round-value is returned. 
		* @param name name of property
		* @param dflt default value
		* @return a int property.
		*/
		public bool get (String name, bool dflt) {
			// use value by round if already parsed
			bool[] vals = (bool[]) valByRound[name];
			if (vals != null) {
				return vals[roundNumber % vals.Length];
			}
			// done if not by round 
			String sval = props.GetProperty(name,""+dflt);
			if (sval.IndexOf(":")<0) {
				return bool.Parse(sval);
			}
			// first time this prop is extracted by round 
			int k = sval.IndexOf(":");
			String colName = sval.Substring(0,k);
			sval = sval.Substring(k+1);
			colForValByRound[name] = colName;
			vals = propToBooleanArray(sval);
			valByRound[name] = vals;
			return vals[roundNumber % vals.Length];
		}
  
		/**
		* Increment the round number, for config values that are extracted by round number. 
		* @return the new round number.
		*/
		public int newRound () {
			roundNumber++;
    
			StringBuilder sb = new StringBuilder("--> Round ").Append(roundNumber-1).Append("-->").Append(roundNumber);

			// log changes in values
			if (valByRound.Count>0) {
				sb.Append(": ");
				foreach (string name in valByRound.Keys)
				{
					Object a = valByRound[name];
					if (a is int[]) {
						int[] ai = (int[]) a;
						int n1 = (roundNumber-1)%ai.Length;
						int n2 = roundNumber%ai.Length;
						sb.Append("  ").Append(name).Append(":").Append(ai[n1]).Append("-->").Append(ai[n2]);
					} else if (a is double[]){
						double[] ad = (double[]) a;
						int n1 = (roundNumber-1)%ad.Length;
						int n2 = roundNumber%ad.Length;
						sb.Append("  ").Append(name).Append(":").Append(ad[n1]).Append("-->").Append(ad[n2]);
					}
					else {
						bool[] ab = (bool[]) a;
						int n1 = (roundNumber-1)%ab.Length;
						int n2 = roundNumber%ab.Length;
						sb.Append("  ").Append(name).Append(":").Append(ab[n1]).Append("-->").Append(ab[n2]);
					}
				} // end of foreach
				
			} // end of if
			Console.WriteLine();
			Console.WriteLine(sb.ToString());
			Console.WriteLine();
    
			return roundNumber;
		}
  
		// extract properties to array, e.g. for "10:100:5" return int[]{10,100,5}. 
		private int[] propToIntArray (String s) {
			if (s.IndexOf(":")<0) {
				return new int [] { int.Parse(s) };
			}
    
			ArrayList a = new ArrayList();
			string[] st = s.Split(':');
			foreach (string token in st)
			{
				a.Add(int.Parse(token));
			}
			int[] res = new int[a.Count]; 
			for (int i=0; i<a.Count; i++) {
				res[i] = (int)a[i];
			}
			return res;
		}
    
		// extract properties to array, e.g. for "10.7:100.4:-2.3" return int[]{10.7,100.4,-2.3}. 
		private double[] propToDoubleArray (String s) {
			if (s.IndexOf(":")<0) {
				return new double [] { Double.Parse(s) };
			}
    
			ArrayList a = new ArrayList();
			string[] st = s.Split(':');
			foreach (string token in st)
			{
				a.Add(Double.Parse(token));
			}
			double[] res = new double[a.Count]; 
			for (int i=0; i<a.Count; i++) {
				res[i] = (Double) a[i];
			}
			return res;
		}
    
		// extract properties to array, e.g. for "true:true:false" return bool[]{true,false,false}. 
		private bool[] propToBooleanArray (String s) {
			if (s.IndexOf(":")<0) {
				return new bool [] { bool.Parse(s) };
			}
    
			ArrayList a = new ArrayList();
			string[] st = s.Split(':');
			foreach (string token in st)
			{
				a.Add(bool.Parse(token));
			}
			bool[] res = new bool[a.Count]; 
			for (int i=0; i<a.Count; i++) {
				res[i] = (bool)a[i];
			}
			return res;
		}

		/**
		* @return names of params set by round, for reports title
		*/
		public String getColsNamesForValsByRound() {
			if (colForValByRound.Count==0) {
				return "";
			}
			StringBuilder sb = new StringBuilder();
			foreach (string name in valByRound.Keys)
			{
				String colName = (String) colForValByRound[name];
				sb.Append(" ").Append(colName);
			}
			return sb.ToString();
		}

		/**
		* @return values of params set by round, for reports lines.
		*/
		public String getColsValuesForValsByRound(int roundNum) {
			if (colForValByRound.Count==0) {
				return "";
			}
			StringBuilder sb = new StringBuilder();
			foreach (string name in valByRound.Keys)
			{
				String colName = (String)colForValByRound[name];
				String template = " "+colName;
				if (roundNum<0) {
					// just append blanks
					sb.Append(Format.formatPaddLeft("-",template));
				} else {
				// append actual values, for that round
				Object a = valByRound[name];
				if (a is int[]) { 
					int[] ai = (int[])a;
					int n = roundNum % ai.Length;
					sb.Append(Format.format(ai[n],template));
				}
				else if (a is double[]) {
					double[] ad = (double[]) a;
					int n = roundNum % ad.Length;
					sb.Append(Format.format(2, ad[n],template));
				}
				else {
					bool[] ab = (bool[])a;
					int n = roundNum % ab.Length;
					sb.Append(Format.formatPaddLeft(""+ab[n],template));
				}
				}
			}
				

			return sb.ToString();
		}

		/**
		* @return the round number.
		*/
		public int getRoundNumber() {
			return roundNumber;
		}

		/**
		* @return Returns the algorithmText.
		*/
		public String getAlgorithmText() {
			return algorithmText;
		}
	}
}
