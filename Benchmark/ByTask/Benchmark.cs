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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections.Specialized;
using System.Reflection;
using SimpleOX.Excel;

using Lucene.Net.Benchmark;
using Lucene.Net.Benchmark.ByTask;
using Lucene.Net.Benchmark.ByTask.Utils;

namespace Lucene.Net.Benchmark {

	/**
 * Run the benchmark algorithm.
 * <p>Usage: java Benchmark  algorithm-file
 * <ol>
 * <li>Read algorithm.</li>
 * <li> Run the algorithm.</li>
 * </ol>
 * Things to be added/fixed in "Benchmarking by tasks":
 * <ol>
 * <li>TODO - report into Excel and/or graphed view.</li>
 * <li>TODO - perf comparison between Lucene releases over the years.</li>
 * <li>TODO - perf report adequate to include in Lucene nightly build site? (so we can easily track performance changes.)</li>
 * <li>TODO - add overall time control for repeated execution (vs. current by-count only).</li>
 * <li>TODO - query maker that is based on index statistics.</li>
 * </ol>
 */
	public class Benchmark {

		private PerfRunData runData;
		public Algorithm algorithm;
		private bool executed;
		// Let's make an excel document here
		// TODO: make this something you can configure in the configuration file
		public static Document ExcelDoc; 
		public static int LogIndex;
		public static DataSheet LogSheet;
  
		System.IO.StreamWriter errWriter;

		public Benchmark (StreamReader algReader) {
			ExcelDoc = new Document();
			LogIndex = ExcelDoc.CreateDataSheet( "Log" );
			LogSheet = (DataSheet)Benchmark.ExcelDoc.Sheets[ Benchmark.LogIndex ];
			errWriter = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
			errWriter.AutoFlush = true;
			// prepare run data
			try {
				runData = new PerfRunData(new Config(algReader));
			} catch (Exception e) {
				errWriter.WriteLine(e.StackTrace );
				throw new Exception("Error: cannot init PerfRunData!",e);
			}
    
			// parse algorithm
			try {
				algorithm = new Algorithm(runData);
			} catch (Exception e) {
				throw new Exception("Error: cannot understand algorithm!",e);
			}
		}
		
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void  execute() {
			if (executed) {
				throw new InvalidOperationException("Benchmark was already executed");
			}
			executed = true;
			runData.setStartTimeMillis();
			algorithm.execute();
		}
  
		/**
		* Run the benchmark algorithm.
		* @param args benchmark config and algorithm files
		*/
		public static void Main( string[] args ) {
			System.IO.StreamWriter errWriter;
			errWriter = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
			errWriter.AutoFlush = true;

			Console.WindowWidth = 140;
			OrderedDictionary od = new OrderedDictionary();
			
			// verify command line args
			if (args.Length < 1) {
				errWriter.WriteLine("Usage: .Net Benchmark <algorithm file>");
				Console.WriteLine("Usage: .Net Benchmark <algorithm file>");
				Console.WriteLine("Press any key to exit");
				Console.ReadKey();
				return;
			}

			FileStream algFile;
			// verify input files 
			try
			{
				algFile = new FileStream(args[0], FileMode.Open);
			} catch (Exception e)
			{
					errWriter.WriteLine("cannot find/read algorithm file: " + args[0]);
					Console.WriteLine("cannot find/read algorithm file: " + args[0]);
					Console.WriteLine( e.Message );
					Console.WriteLine("Press any key to exit");
					Console.ReadKey();
					return;
			}


			Console.WriteLine("Running algorithm from: "+algFile.Name);
			Benchmark benchmark = null;
			try {
				benchmark = new Benchmark(new StreamReader(algFile));
			} catch (Exception e) {
				errWriter.WriteLine(e.StackTrace );
				Console.WriteLine(e.StackTrace);
				Console.WriteLine("Press any key to exit");
				Console.ReadKey();
			}

			
			Benchmark.LogSheet.AddRowAndCell( "Running algorithm from: " + algFile.Name );
			Benchmark.LogSheet.AddRowAndCell( "------------> algorithm:" );
			Console.WriteLine("------------> algorithm:");
			LogSheet.AddRowsAndCellsSplit( benchmark.getAlgorithm().toString(), "alg" );
			Console.WriteLine(benchmark.getAlgorithm().toString());

			// execute
			try {
				benchmark.execute();
			}  catch (Exception e) {
				errWriter.WriteLine( "Error: cannot execute the algorithm! " + e.Message );
				Console.WriteLine( "Error: cannot execute the algorithm! " + e.Message );
				//Console.WriteLine( e.InnerException.StackTrace );
				Console.WriteLine( e.Source );
				Console.WriteLine( e.StackTrace );
				Console.WriteLine( e.ToString() );
				foreach( var item in e.Data.Keys ) {
					LogSheet.AddRowsAndCellsSplit( (String)item + " --------> " + e.Data[ item ].ToString() );
					Console.WriteLine( (String)item + " --------> " + e.Data[ item ].ToString() );
				}
				Console.WriteLine( "Press any key to exit" );
				Console.ReadKey();
			}

			Console.WriteLine("####################");
			Console.WriteLine("###  D O N E !!! ###");
			Console.WriteLine("####################");
			LogSheet.AddRowAndCell( "####################" );
			LogSheet.AddRowAndCell( "###  D O N E !!! ###" );
			LogSheet.AddRowAndCell( "####################" );
			ExcelDoc.SaveDocument( "Stats.xlsx" );
			Console.ReadKey();
		}

		/**
		* @return Returns the algorithm.
		*/
		public Algorithm getAlgorithm() {
			return algorithm;
		}

		/**
		* @return Returns the runData.
		*/
		public PerfRunData getRunData() {
			return runData;
		}
	}
}
