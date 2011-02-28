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
using Lucene.Net.Index;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Optimize the index.
	 * <br>Other side effects: none.
	 */
	public class OptimizeTask : PerfTask {

		public OptimizeTask(PerfRunData runData) : base(runData) {
			// do nothing
		}

		int maxNumSegments = 1;

		public override int doLogic() {
			IndexWriter iw = getRunData().getIndexWriter();
			iw.Optimize( maxNumSegments );
			Benchmark.LogSheet.AddRowAndCell("optimize called");
			Console.WriteLine("optimize called");
			return 1;
		}

		public override void setParams(String args) {
			base.setParams(args);
			maxNumSegments = (int)Double.Parse(args);
		}

		public override bool supportsParams() {
			return true;
		}
	}
}
