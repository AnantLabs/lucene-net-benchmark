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
using System.Collections.Specialized;
using System.Collections;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Report all statistics grouped/aggregated by name and round.
	 * <br>Other side effects: None.
	 */
	public class RepSumByNameRoundTask : ReportTask {

		  public RepSumByNameRoundTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  public override int doLogic() {
			Report rp = reportSumByNameRound(getRunData().getPoints().taskStats());

			Console.WriteLine();
			Console.WriteLine("------------> Report Sum By (any) Name and Round ("+
				rp.getSize()+" about "+rp.getReported()+" out of "+rp.getOutOf()+")");
			Console.WriteLine(rp.getText());
			Console.WriteLine();
			Benchmark.LogSheet.AddRowAndCell("");
			Benchmark.LogSheet.AddRowAndCell( "------------> Report Sum By (any) Name and Round (" +
				rp.getSize() + " about " + rp.getReported() + " out of " + rp.getOutOf() + ")" );
			Benchmark.LogSheet.AddRowAndCell( rp.getText() );
			Benchmark.LogSheet.AddRowAndCell("");
    
			return 0;
		  }

		  /**
		   * Report statistics as a string, aggregate for tasks named the same, and from the same round.
		   * @return the report
		   */
		  protected Report reportSumByNameRound(IList taskStats) {
			// aggregate by task name and round
			OrderedDictionary p2 = new OrderedDictionary();
			int reported = 0;
			  foreach (TaskStats stat1 in taskStats) {
				  if (stat1.getElapsed()>= new TimeSpan(0)) { // consider only tasks that ended
					reported++;
					String name = stat1.getTask().getName();
					String rname = stat1.getRound()+"."+name; // group by round
					TaskStats stat2 = (TaskStats) p2[rname];
					if (stat2 == null) {
					  try {
						stat2 = (TaskStats) stat1.clone();
					  } catch (NotImplementedException e) {
						throw new ApplicationException("RepSumByNameRoundTask threw not supported clone exception", e);
					  }
					  p2.Add(rname,stat2);
					} else {
					  stat2.add(stat1);
					}
				  }
			    }
			// now generate report from secondary list p2    
			return genPartialReport(reported, p2, taskStats.Count);
		  }

	}
}
