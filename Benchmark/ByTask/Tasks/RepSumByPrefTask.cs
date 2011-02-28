using System;
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

using System.Collections;
using System.Collections.Specialized;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Report by-name-prefix statistics aggregated by name.
	 * <br>Other side effects: None.
	 */
	public class RepSumByPrefTask : ReportTask {

		public RepSumByPrefTask(PerfRunData runData) : base(runData) {
			// Do nothing here
		}

		protected String prefix;

		public override int doLogic() {
			Report rp = reportSumByPrefix(getRunData().getPoints().taskStats());
    
			Console.WriteLine();
			Console.WriteLine("------------> Report Sum By Prefix ("+prefix+") ("+
				rp.getSize()+" about "+rp.getReported()+" out of "+rp.getOutOf()+")");
			Console.WriteLine(rp.getText());
			Console.WriteLine();

			return 0;
		}

		protected Report reportSumByPrefix (IList taskStats) {
			// aggregate by task name
			int reported = 0;
			OrderedDictionary p2 = new OrderedDictionary();
			foreach (TaskStats stat1 in taskStats) {
				if (stat1.getElapsed()>= new TimeSpan(0) && stat1.getTask().getName().StartsWith(prefix)) { // only ended tasks with proper name
					reported++;
					String name = stat1.getTask().getName();
					TaskStats stat2 = (TaskStats) p2[name];
					if (stat2 == null) {
						try {
							stat2 = (TaskStats) stat1.clone();
						} catch (NotImplementedException e) {
							throw new ApplicationException("problem with clone() in RepSumByPrefTask", e);
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
  

		public void setPrefix(String prefix) {
			this.prefix = prefix;
			ReportSheet.Title = ReportSheet.Title + "-" +  prefix;
		}

		/* (non-Javadoc)
		* @see PerfTask#toString()
		*/
		public override String toString() {
			return base.ToString()+" "+prefix;
		}

	}
}
