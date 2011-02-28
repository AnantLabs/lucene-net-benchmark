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

using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Report all statistics with no aggregations.
	 * <br>Other side effects: None.
	 */
	public class RepAllTask : ReportTask {

		 public RepAllTask(PerfRunData runData) : base(runData) {
			// do nothing
		   }

		  public override int doLogic() {
			Report rp = reportAll((List<TaskStats>)getRunData().getPoints().taskStats());
    
			Console.WriteLine();
			Console.WriteLine("------------> Report All ("+rp.getSize()+" out of "+rp.getOutOf()+")");
			Console.WriteLine(rp.getText());
			Console.WriteLine();
			Benchmark.LogSheet.AddRowAndCell("");
			Benchmark.LogSheet.AddRowAndCell("------------> Report All (" + rp.getSize() + " out of " + rp.getOutOf() + ")" );
			Benchmark.LogSheet.AddRowAndCell(rp.getText() );
			Benchmark.LogSheet.AddRowAndCell("");
			return 0;
		  }
  
		  /**
		   * Report detailed statistics as a string
		   * @return the report
		   */
		  public Report reportAll(List<TaskStats> taskStats) {
			String longestOp = base.longestOp(taskStats);
			bool first = true;
			StringBuilder sb = new StringBuilder();
			sb.Append(tableTitle(longestOp));
			sb.Append(newline);
			int reported = 0;
			foreach (TaskStats stat in taskStats) {
			  if (stat.getElapsed()>= new TimeSpan(0)) { // consider only tasks that ended
				if (!first) {
				  sb.Append(newline);
				}
				first = false;
				String line = taskReportLine(longestOp, stat);
				reported++;
				if (taskStats.Count>2 && reported%2==0) {
				  line = line.Replace("   "," - ");
				}
				sb.Append(line);
			  }
			}
			String reptxt = (reported==0 ? "No Matching Entries Were Found!" : sb.ToString());
			return new Report(reptxt,reported,reported,taskStats.Count);
		  }
	}
}
