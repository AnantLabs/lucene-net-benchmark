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

using Lucene.Net.Index;

namespace Lucene.Net.Benchmark.ByTask.Tasks {
	
	
	public class ReopenReaderTask : PerfTask {

		  public ReopenReaderTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  public override int doLogic() {
			IndexReader ir = getRunData().getIndexReader();
			IndexReader or = ir;
			IndexReader nr = ir.Reopen();
			if(nr != or) {
			  getRunData().setIndexReader(nr);
			  or.Close();
			}
			return 1;
		  }
	}
}
