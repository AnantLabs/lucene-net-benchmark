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
using System.Collections.Specialized;
using System.Text;
using Lucene.Net.Benchmark.ByTask.Utils;
using SimpleOX.Excel;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Report (abstract) task - all report tasks extend this task.
	 */
	public abstract class ReportTask : PerfTask {

		protected int ReportSheetIndex;
		protected DataSheet ReportSheet;

		public ReportTask(PerfRunData runData) : base(runData) {
			ReportSheetIndex = Benchmark.ExcelDoc.CreateDataSheet( this.getName() );
			ReportSheet = (DataSheet)Benchmark.ExcelDoc.Sheets[ ReportSheetIndex ];
		}

		/* (non-Javadoc)
		* @see PerfTask#shouldNeverLogAtStart()
		*/
		protected override bool shouldNeverLogAtStart() {
			return true;
		}

		/* (non-Javadoc)
		* @see PerfTask#shouldNotRecordStats()
		*/
		protected override bool shouldNotRecordStats() {
			return true;
		}

		/*
		* From here start the code used to generate the reports. 
		* Subclasses would use this part to generate reports.
		*/
  
		protected static readonly String newline = Environment.NewLine;
  
		/**
		* Get a textual summary of the benchmark results, average from all test runs.
		*/
		protected static readonly String OP =          "Operation  ";
		protected static readonly String ROUND =       " round";
		protected static readonly String RUNCNT =      "   runCnt";
		protected static readonly String RECCNT =      "   recsPerRun";
		protected static readonly String RECSEC =      "        rec/s";
		protected static readonly String ELAPSED =     "  elapsedSec";
		protected static readonly String USEDMEM =     "    avgUsedMem";
		protected static readonly String TOTMEM =      "    avgTotalMem";
		protected static readonly String[] COLS = {
			RUNCNT,
			RECCNT,
			RECSEC,
			ELAPSED,
			USEDMEM,
			TOTMEM
		};

		/**
		* Compute a title line for a report table
		* @param longestOp size of longest op name in the table
		* @return the table title line.
		*/
		protected String tableTitle (String longestOp) {
			ReportSheet.StartRow();
			StringBuilder sb = new StringBuilder();
			ReportSheet.AddCell( OP );
			sb.Append(Format.format(OP,longestOp));
			ReportSheet.AddCell( ROUND );
			sb.Append(ROUND);
			String colNames = getRunData().getConfig().getColsNamesForValsByRound();
			String[] cols = colNames.Split( ' ' );
			foreach( String col in cols ) {
				if( col != "" ) {
					ReportSheet.AddCell( col.Trim() );
				}
			}
			sb.Append(colNames);
			for (int i = 0; i < COLS.Length; i++) {
				ReportSheet.AddCell( COLS[ i ] );
				sb.Append(COLS[i]);
			}
			return sb.ToString(); 
		}
  
		/**
		* find the longest op name out of completed tasks.  
		* @param taskStats completed tasks to be considered.
		* @return the longest op name out of completed tasks.
		*/
		protected String longestOp(List<TaskStats> taskStats) {
			String longest = OP;
			foreach (TaskStats stat in taskStats) {
				if (stat.getElapsed()>= new TimeSpan(0)) { // consider only tasks that ended
					String name = stat.getTask().getName();
					if (name.Length > longest.Length) {
						longest = name;
					}
				}
			}
			return longest;
		}
  
		/**
		* Compute a report line for the given task stat.
		* @param longestOp size of longest op name in the table.
		* @param stat task stat to be printed.
		* @return the report line.
		*/
		protected String taskReportLine(String longestOp, TaskStats stat) {
			ReportSheet.StartRow();
			PerfTask task = stat.getTask();
			StringBuilder sb = new StringBuilder();
			ReportSheet.AddCell( task.getName() );
			sb.Append(Format.format(task.getName(), longestOp));
			String round = (stat.getRound()>=0 ? ""+stat.getRound() : "-");
			ReportSheet.AddCell( round );
			sb.Append(Format.formatPaddLeft(round, ROUND));
			String valString = getRunData().getConfig().getColsValuesForValsByRound( stat.getRound() );
			String[] vals = valString.Split( ' ' );
			foreach( String val in vals ) {
				if( val == "" ) {
					//ReportSheet.AddCell( "" );
				}
				else {
					int temp;
					if( int.TryParse( val.Trim(), out  temp ) ) {
						ReportSheet.AddCell( temp );
					}
					else {
						ReportSheet.AddCell( val.Trim() );
					}
				}
			}
			sb.Append(valString);
			int numRuns = stat.getNumRuns();
			ReportSheet.AddCell( numRuns );
			sb.Append(Format.format(numRuns, RUNCNT));
			int statCount = stat.getCount();
			ReportSheet.AddCell( statCount / numRuns );
			sb.Append(Format.format(statCount / numRuns, RECCNT));
			TimeSpan elapsed = (stat.getElapsed()> new TimeSpan(0) ? stat.getElapsed() : new TimeSpan(1)); // assume at least 1ms
			float perSec = (float)( stat.getCount() * 1000.0 / elapsed.TotalMilliseconds );
			String perSecString = perSec.ToString("N2");
			ReportSheet.AddCell( float.Parse(perSecString));
			sb.Append(Format.format(2, (float)(stat.getCount() * 1000.0 / elapsed.TotalMilliseconds), RECSEC));
			ReportSheet.AddCell( float.Parse((((float) stat.getElapsed().TotalMilliseconds / 1000)).ToString("N2")) );
			sb.Append(Format.format(2, (float) stat.getElapsed().TotalMilliseconds / 1000, ELAPSED));
			ReportSheet.AddCell( float.Parse(((float)stat.getMaxUsedMem() / stat.getNumRuns()).ToString("N0")) );
			sb.Append(Format.format(0, (float) stat.getMaxUsedMem() / stat.getNumRuns(), USEDMEM));
			ReportSheet.AddCell( float.Parse(((float)stat.getMaxTotMem() / stat.getNumRuns()).ToString()) );
			sb.Append(Format.format(0, (float) stat.getMaxTotMem() / stat.getNumRuns(), TOTMEM));
			return sb.ToString();
		}

		protected Report genPartialReport(int reported, OrderedDictionary partOfTasks, int totalSize) {
			// There's probably a cleaner way to convert this, but this seems to be working for now
			List<TaskStats> lts = new List<TaskStats>();
			foreach( TaskStats ts in partOfTasks.Values ) {
				lts.Add( ts );
			}
			String longetOp = longestOp(lts);
			bool first = true;
			StringBuilder sb = new StringBuilder();
			String TT = tableTitle( longetOp );
			sb.Append(TT);
			sb.Append(newline);
			int lineNum = 0;
			foreach (TaskStats statTask in partOfTasks.Values) {
				if (!first) {
					sb.Append(newline);
				}
				first = false;
				String line = taskReportLine(longetOp,statTask);
				lineNum++;
				if (partOfTasks.Count>2 && lineNum%2==0) {
					line = line.Replace("   "," - ");
				}
				sb.Append(line);
			} // end of foreach
			String reptxt = (reported==0 ? "No Matching Entries Were Found!" : sb.ToString());
			return new Report(reptxt,partOfTasks.Count,reported,totalSize);
		}

	}
}
