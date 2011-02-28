using System;
/**
 * Copyright 2005 The Apache Software Foundation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Management;
using System.Text;
using Lucene.Net.Analysis;

namespace Lucene.Net.Benchmark.stats {

	/**
	 * This class holds together all parameters related to a test. Single test is
	 * performed several times, and all results are averaged.
	 *
	 */
	public class TestData {

		public static int[] MAX_BUFFERED_DOCS_COUNTS = new int[]{10, 20, 50, 100, 200, 500};
		public static int[] MERGEFACTOR_COUNTS = new int[]{10, 20, 50, 100, 200, 500};

		/**
		 * ID of this test data.
		 */
		private String id;
		/**
		 * Heap size.
		 */
		private long heap;
		/**
		 * List of results for each test run with these parameters.
		 */
		private List<TestRunData> runData = new List<TestRunData>(); // TODO: double check this
		private int maxBufferedDocs, mergeFactor;
		/**
		 * Directory containing source files.
		 */
		private DirectoryInfo source;
		/**
		 * Lucene Directory implementation for creating an index.
		 */
		private Lucene.Net.Store.Directory directory;
		/**
		 * Analyzer to use when adding documents.
		 */
		private Analyzer analyzer;
		/**
		 * If true, use compound file format.
		 */
		private bool compound;
		/**
		 * If true, optimize index when finished adding documents.
		 */
		private bool optimize;
		/**
		 * Data for search benchmarks.
		 */
		private QueryData[] queries;

		private static NumberFormatInfo[] numFormat = new NumberFormatInfo[2] { new NumberFormatInfo(), new NumberFormatInfo()};
		private const String padd = "                                  ";

		/**
 * Get a textual summary of the benchmark results, average from all test runs.
 */
		const String ID =      "# testData id     ";
		const String OP =      "operation      ";
		const String RUNCNT =  "     runCnt";
		const String RECCNT =  "     recCnt";
		const String RECSEC =  "          rec/s";
		const String FREEMEM = "       avgFreeMem";
		const String TOTMEM =  "      avgTotalMem";
		readonly String[] COLS = {
				ID,
				OP,
				RUNCNT,
				RECCNT,
				RECSEC,
				FREEMEM,
				TOTMEM
			};

		public TestData()
		{
			ObjectQuery winQuery = new ObjectQuery( "SELECT * FROM Win32_ComputerSystem" );
			  long TotalPhysicalMemory = 0;
			  ManagementObjectSearcher searcher = new ManagementObjectSearcher( winQuery );
			  foreach( ManagementObject item in searcher.Get() ) {
				  TotalPhysicalMemory = long.Parse( item[ "TotalPhysicalMemory" ].ToString() );
			  }
			heap = TotalPhysicalMemory;

			numFormat[ 0 ].NumberDecimalDigits = 0;
			numFormat[ 1 ].NumberDecimalDigits = 1;
			
		}

		private class DCounter
		{
			public double total;
			public int count, recordCount;
		}

		private class LCounter
		{
			public long total;
			public int count;
		}

		private class LDCounter
		{
		  public double Dtotal;
		  public int Dcount, DrecordCount;
		  public long Ltotal0;
		  public int Lcount0;
		  public long Ltotal1;
		  public int Lcount1;
		}

		public String showRunData(String prefix)
		{
			if (runData.Count == 0)
			{
				return "# [NO RUN DATA]";
			}
			Dictionary<String, LDCounter> resByTask = new Dictionary<String, LDCounter>(); 
			StringBuilder sb = new StringBuilder();
			String lineSep = Environment.NewLine;
			sb.Append("warm = Warm Index Reader").Append(lineSep).Append("srch = Search Index").Append(lineSep).Append("trav = Traverse Hits list, optionally retrieving document").Append(lineSep).Append(lineSep);
			for (int i = 0; i < COLS.Length; i++) {
			  sb.Append(COLS[i]);
			}
			sb.Append("\n");
			OrderedDictionary mapMem = new OrderedDictionary();
			OrderedDictionary mapSpeed = new OrderedDictionary();
			for (int i = 0; i < runData.Count; i++)
			{
				TestRunData trd = (TestRunData) runData[i];
				String[] labels = trd.getLabels();
				foreach (String label in labels)
				{
					MemUsage mem = trd.getMemUsage(label);
					if (mem != null)
					{
						TestData.LCounter[] tm = (TestData.LCounter[]) mapMem[label];
						if (tm == null)
						{
							tm = new TestData.LCounter[2];
							tm[0] = new TestData.LCounter();
							tm[1] = new TestData.LCounter();
							mapMem.Add(label, tm);
						}
						tm[0].total += mem.avgFree;
						tm[0].count++;
						tm[1].total += mem.avgTotal;
						tm[1].count++;
					}
					TimeData td = trd.getTotals(label);
					if (td != null)
					{
						TestData.DCounter dc = (TestData.DCounter) mapSpeed[label];
						if (dc == null)
						{
							dc = new TestData.DCounter();
							mapSpeed.Add(label, dc);
						}
						dc.count++;
						//dc.total += td.getRate();
						dc.total += (td.count>0 && td.elapsed<=0 ? 1 : td.elapsed); // assume at least 1ms for any countable op
						dc.recordCount += (int)td.count;
					}
				}
			}
			OrderedDictionary res = new OrderedDictionary();
			foreach (String label in res.Keys) {
				TestData.DCounter dc = (TestData.DCounter) mapSpeed[label];
				// TODO: I need to check these formattings to know they are right
				res.Add(label, 
					format(dc.count, RUNCNT) + 
					format(dc.recordCount / dc.count, RECCNT) +
					format(1,(float) (dc.recordCount * 1000.0 / (dc.total>0 ? dc.total : 1.0)), RECSEC)
					//format((float) (dc.total / (double) dc.count), RECSEC)
					);
            
				// also sum by task
				String task = label.Substring(label.LastIndexOf("-")+1);
				LDCounter ldc = (LDCounter) resByTask[task];
				if (ldc==null) {
				  ldc = new LDCounter();
				  resByTask.Add(task,ldc);
				}
				ldc.Dcount += dc.count;
				ldc.DrecordCount += dc.recordCount;
				ldc.Dtotal += (dc.count>0 && dc.total<=0 ? 1 : dc.total); // assume at least 1ms for any countable op 
			}
			foreach (String label in mapMem.Keys) {
				TestData.LCounter[] lc = (TestData.LCounter[]) mapMem[label];
				String speed = (String) res[label];
				bool makeSpeed = false;
				if (speed == null)
				{
					makeSpeed = true;
					speed =  
					  format(lc[0].count, RUNCNT) + 
					  format(0, RECCNT) + 
					  format(0,(float)0.0, RECSEC);
				}
				res.Add(label, speed + 
					format(0, lc[0].total / lc[0].count, FREEMEM) + 
					format(0, lc[1].total / lc[1].count, TOTMEM));
            
				// also sum by task
				String task = label.Substring(label.LastIndexOf("-")+1);
				LDCounter ldc = (LDCounter) resByTask[task];
				if (ldc==null) {
				  ldc = new LDCounter();
				  resByTask.Add(task,ldc);
				  makeSpeed = true;
				}
				if (makeSpeed) {
				  ldc.Dcount += lc[0].count;
				}
				ldc.Lcount0 += lc[0].count;
				ldc.Lcount1 += lc[1].count;
				ldc.Ltotal0 += lc[0].total;
				ldc.Ltotal1 += lc[1].total;
			}
			foreach (String label in res.Keys) {
				sb.Append(format(prefix, ID));
				sb.Append(format(label, OP));
				sb.Append(res[label]).Append("\n");
			}
			// show results by task (srch, optimize, etc.) 
			sb.Append("\n");
			for (int i = 0; i < COLS.Length; i++) {
			  sb.Append(COLS[i]);
			}
			sb.Append("\n");
			foreach (String task in resByTask.Keys) {
				LDCounter ldc = (LDCounter) resByTask[task];
				sb.Append(format("    ", ID));
				sb.Append(format(task, OP));
				sb.Append(format(ldc.Dcount, RUNCNT)); 
				sb.Append(format(ldc.DrecordCount / ldc.Dcount, RECCNT));
				sb.Append(format(1,(float) (ldc.DrecordCount * 1000.0 / (ldc.Dtotal>0 ? ldc.Dtotal : 1.0)), RECSEC));
				sb.Append(format(0, ldc.Ltotal0 / ldc.Lcount0, FREEMEM)); 
				sb.Append(format(0, ldc.Ltotal1 / ldc.Lcount1, TOTMEM));
				sb.Append("\n");
			}
			return sb.ToString();
		}

		// pad number from left
		// numFracDigits must be 0 or 1.
		static String format(int numFracDigits, float f, String col) {
		  String res = padd + f.ToString(numFormat[numFracDigits]);
		  return res.Substring(res.Length - col.Length);
		}

		// pad number from left
		static String format(int n, String col) {
		  String res = padd + n;
		  return res.Substring(res.Length - col.Length);
		}

		// pad string from right
		static String format(String s, String col) {
		  return (s + padd).Substring(0,col.Length);
		}

		/**
		 * Prepare a list of benchmark data, using all possible combinations of
		 * benchmark parameters.
		 *
		 * @param sources   list of directories containing different source document
		 *                  collections
		 * @param analyzers of analyzers to use.
		 */
		public static TestData[] getAll(DirectoryInfo[] sources, Analyzer[] analyzers)
		{
			ArrayList res = new ArrayList(50);
			TestData ref1 = new TestData();
			for (int q = 0; q < analyzers.Length; q++)
			{
				for (int m = 0; m < sources.Length; m++)
				{
					for (int i = 0; i < MAX_BUFFERED_DOCS_COUNTS.Length; i++)
					{
						for (int k = 0; k < MERGEFACTOR_COUNTS.Length; k++)
						{
							for (int n = 0; n < Constants.BOOLEANS.Length; n++)
							{
								for (int p = 0; p < Constants.BOOLEANS.Length; p++)
								{
									ref1.id = "td-" + q + m + i + k + n + p;
									ref1.source = sources[m];
									ref1.analyzer = analyzers[q];
									ref1.maxBufferedDocs = MAX_BUFFERED_DOCS_COUNTS[i];
									ref1.mergeFactor = MERGEFACTOR_COUNTS[k];
									ref1.compound = Constants.BOOLEANS[n];
									ref1.optimize = Constants.BOOLEANS[p];
									try
									{
										res.Add(ref1.clone());
									}
									catch (Exception e)
									{
										System.Diagnostics.Trace.WriteLine( e.Message );
									}
								}
							}
						}
					}
				}
			}
			return (TestData[]) res.ToArray();
		}

		/**
		 * Similar to {@link #getAll(java.io.File[], org.apache.lucene.analysis.Analyzer[])} but only uses
		 * maxBufferedDocs of 10 and 100 and same for mergeFactor, thus reducing the number of permutations significantly.
		 * It also only uses compound file and optimize is always true.
		 *
		 * @param sources
		 * @param analyzers
		 * @return An Array of {@link TestData}
		 */
		public static TestData[] getTestDataMinMaxMergeAndMaxBuffered(DirectoryInfo[] sources, Analyzer[] analyzers)
		{
			ArrayList res = new ArrayList(50);
			TestData ref1 = new TestData();
			for (int q = 0; q < analyzers.Length; q++)
			{
				for (int m = 0; m < sources.Length; m++)
				{
					ref1.id = "td-" + q + m + "_" + 10 + "_" + 10;
					ref1.source = sources[m];
					ref1.analyzer = analyzers[q];
					ref1.maxBufferedDocs = 10;
					ref1.mergeFactor = 10;//MERGEFACTOR_COUNTS[k];
					ref1.compound = true;
					ref1.optimize = true;
					try
					{
						res.Add(ref1.clone());
					}
					catch (Exception e)
					{
						System.Diagnostics.Trace.WriteLine(e.Message);
					}
					ref1.id = "td-" + q + m  + "_" + 10 + "_" + 100;
					ref1.source = sources[m];
					ref1.analyzer = analyzers[q];
					ref1.maxBufferedDocs = 10;
					ref1.mergeFactor = 100;//MERGEFACTOR_COUNTS[k];
					ref1.compound = true;
					ref1.optimize = true;
					try
					{
						res.Add(ref1.clone());
					}
					catch (Exception e)
					{
						System.Diagnostics.Trace.WriteLine(e.Message);
					}
					ref1.id = "td-" + q + m + "_" + 100 + "_" + 10;
					ref1.source = sources[m];
					ref1.analyzer = analyzers[q];
					ref1.maxBufferedDocs = 100;
					ref1.mergeFactor = 10;//MERGEFACTOR_COUNTS[k];
					ref1.compound = true;
					ref1.optimize = true;
					try
					{
						res.Add(ref1.clone());
					}
					catch (Exception e)
					{
						System.Diagnostics.Trace.WriteLine(e.Message);
					}
					ref1.id = "td-" + q + m + "_" + 100 + "_" + 100;
					ref1.source = sources[m];
					ref1.analyzer = analyzers[q];
					ref1.maxBufferedDocs = 100;
					ref1.mergeFactor = 100;//MERGEFACTOR_COUNTS[k];
					ref1.compound = true;
					ref1.optimize = true;
					try
					{
						res.Add(ref1.clone());
					}
					catch (Exception e)
					{
						System.Diagnostics.Trace.WriteLine( e.Message );
					}
				}
			}
			return (TestData[]) res.ToArray();
		}

		protected Object clone()
		{
			TestData cl = new TestData();
			cl.id = id;
			cl.compound = compound;
			cl.heap = heap;
			cl.mergeFactor = mergeFactor;
			cl.maxBufferedDocs = maxBufferedDocs;
			cl.optimize = optimize;
			cl.source = source;
			cl.directory = directory;
			cl.analyzer = analyzer;
			// don't clone runData
			return cl;
		}

		public String toString()
		{
			StringBuilder res = new StringBuilder();
			res.Append("#-- ID: ").Append(id).Append(", ").Append(DateTime.Now).Append(", heap=").Append(heap).Append(" --\n");
			res.Append("# source=").Append(source).Append(", directory=").Append(directory).Append("\n");
			res.Append("# maxBufferedDocs=").Append(maxBufferedDocs).Append(", mergeFactor=").Append(mergeFactor);
			res.Append(", compound=").Append(compound).Append(", optimize=").Append(optimize).Append("\n");
			if (queries != null)
			{
				res.Append(QueryData.getLabels()).Append("\n");
				for (int i = 0; i < queries.Length; i++)
				{
					res.Append("# ").Append(queries[i].toString()).Append("\n");
				}
			}
			return res.ToString();
		}

		public Analyzer getAnalyzer()
		{
			return analyzer;
		}

		public void setAnalyzer(Analyzer analyzer)
		{
			this.analyzer = analyzer;
		}

		public bool isCompound()
		{
			return compound;
		}

		public void setCompound(bool compound)
		{
			this.compound = compound;
		}

		public Lucene.Net.Store.Directory getDirectory()
		{
			return directory;
		}

		public void setDirectory(Lucene.Net.Store.Directory directory)
		{
			this.directory = directory;
		}

		public long getHeap()
		{
			return heap;
		}

		public void setHeap(long heap)
		{
			this.heap = heap;
		}

		public String getId()
		{
			return id;
		}

		public void setId(String id)
		{
			this.id = id;
		}

		public int getMaxBufferedDocs()
		{
			return maxBufferedDocs;
		}

		public void setMaxBufferedDocs(int maxBufferedDocs)
		{
			this.maxBufferedDocs = maxBufferedDocs;
		}

		public int getMergeFactor()
		{
			return mergeFactor;
		}

		public void setMergeFactor(int mergeFactor)
		{
			this.mergeFactor = mergeFactor;
		}

		public bool isOptimize()
		{
			return optimize;
		}

		public void setOptimize(bool optimize)
		{
			this.optimize = optimize;
		}

		public QueryData[] getQueries()
		{
			return queries;
		}

		public void setQueries(QueryData[] queries)
		{
			this.queries = queries;
		}

		public List<TestRunData> getRunData()
		{
			return runData;
		}

		public void setRunData(List<TestRunData> runData)
		{
			this.runData = runData;
		}

		public DirectoryInfo getSource()
		{
			return source;
		}

		public void setSource(DirectoryInfo source)
		{
			this.source = source;
		}
	}
}
