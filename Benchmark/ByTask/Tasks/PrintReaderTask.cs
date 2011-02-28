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
using Lucene.Net.Benchmark.ByTask.Utils;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Lucene.Net.Benchmark.ByTask.Tasks {
	
	
	public class PrintReaderTask : PerfTask {

		private String userData = null;
  
		  public PrintReaderTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }
  
		  public override void setParams(String args) {
			base.setParams(args);
			userData = args;
		  }
  
		  public override bool supportsParams() {
			return true;
		  }
  
		  public override int doLogic() {
			Directory dir = getRunData().getDirectory();
			Config config = getRunData().getConfig();
			IndexReader r = null;
			if (userData == null) 
			  r = IndexReader.Open(dir);
			else
			  r = OpenReaderTask.openCommitPoint(userData, dir, config, true);
			Benchmark.LogSheet.AddRowAndCell( "--> numDocs:" + r.NumDocs() + " dels:" + r.NumDeletedDocs() );
			Console.WriteLine("--> numDocs:"+r.NumDocs()+" dels:"+r.NumDeletedDocs());
			r.Close();
			return 1;
		  }
	}
}
