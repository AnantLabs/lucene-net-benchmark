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
using System.IO;
using Lucene.Net.Index;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Close index writer.
	 * <br>Other side effects: index writer object in perfRunData is nullified.
	 * <br>Takes optional param "doWait": if false, then close(false) is called.
	 */
	public class CloseIndexTask : PerfTask {

		 public CloseIndexTask(PerfRunData runData) : base(runData) {
			// do nothing
		}

		bool doWait = true;

		public override int doLogic() {
			IndexWriter iw = getRunData().getIndexWriter();
			if (iw != null) {
				// If infoStream was set to output to a file, close it.
				StreamWriter infoStream = iw.GetInfoStream();
				System.IO.StreamWriter outWriter;
				outWriter = new System.IO.StreamWriter( Console.OpenStandardOutput(), System.Console.Out.Encoding );
				outWriter.AutoFlush = true;
				System.IO.StreamWriter errWriter;
				errWriter = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
				errWriter.AutoFlush = true;
				if( infoStream != null && infoStream != outWriter
					&& infoStream != errWriter) {
					infoStream.Close();
				}
				iw.Close(doWait);
				getRunData().setIndexWriter(null);
			}
			return 1;
			//  throws IOException
		}

		public override void setParams(String args) {
			base.setParams(args);
			doWait = Boolean.Parse(args);
		}

		public override bool supportsParams() {
			return true;
		}
	}
}
