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
using System.Collections.Specialized;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Report all statistics aggregated by name.
	 * <br>Other side effects: None.
	 */
	class RepSumByNameTask : ReportTask {

		  public RepSumByNameTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  public override int doLogic() {
			Report rp = reportSumByName(getRunData().getPoints().taskStats());

			Console.WriteLine();
			Console.WriteLine("------------> Report Sum By (any) Name ("+
				rp.getSize()+" about "+rp.getReported()+" out of "+rp.getOutOf()+")");
			Console.WriteLine(rp.getText());
			Console.WriteLine();

			return 0;
			  //  throws Exception
		  }

		  /**
		   * Report statistics as a string, aggregate for tasks named the same.
		   * @return the report
		   */
		  protected Report reportSumByName(IList taskStats) {
			// aggregate by task name
			int reported = 0;
			OrderedDictionary p2 = new OrderedDictionary();
			  foreach (TaskStats stat1 in taskStats) {
			  if (stat1.getElapsed() >= new TimeSpan(0)) { // consider only tasks that ended
				reported++;
				String name = stat1.getTask().getName();
				TaskStats stat2 = (TaskStats)p2[name];
				if (stat2 == null) {
				  try {
					stat2 = (TaskStats) stat1.clone();
				  } catch (NotImplementedException e) {
					throw new ApplicationException("Clone not implemented in reportSumByName", e);
				  }
				  p2.Add(name,stat2);
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
