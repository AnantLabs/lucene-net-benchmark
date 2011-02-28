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
using Lucene.Net.Index;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	public class FlushReaderTask : PerfTask {

		  String userData = null;
  
		  public FlushReaderTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }
  
		  public override bool supportsParams() {
			return true;
		  }
  
		  public override void setParams(String args) {
			base.setParams(args);
			userData = args;
		  }
  
		  public override int doLogic() {
			IndexReader reader = getRunData().getIndexReader();
			if (userData != null) {
			  Dictionary<string, string> map = new Dictionary<string, string>();
			  map.Add(OpenReaderTask.USER_DATA, userData);
			  reader.Flush(map);
			} else {
			  reader.Flush();
			}
			return 1;
		  }
	}
}
