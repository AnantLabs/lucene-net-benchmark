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
using System.Reflection;
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Benchmark.ByTask.Utils;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Consumes a {@link org.apache.lucene.benchmark.byTask.feeds.ContentSource}.
	 * Supports the following parameters:
	 * <ul>
	 * <li>content.source - the content source to use. (mandatory)
	 * </ul>
	 */
	public class ConsumeContentSourceTask : PerfTask {

		 private ContentSource source;
		 private DocData dd = new DocData();
  
		  public ConsumeContentSourceTask(PerfRunData runData) : base(runData) {
			Config config = runData.getConfig();
			String sourceClass = config.get("content.source", null);
			if (sourceClass == null) {
			  throw new ArgumentException("content.source must be defined");
			}
			try {
				// TODO: I've got to either do these in setup or come up
				// with an alternative.
				Assembly assembly = Assembly.LoadFrom("Benchmark.exe");
			  source = (ContentSource) assembly.CreateInstance(sourceClass);
			  source.setConfig(config);
			  source.resetInputs();
			} catch (Exception e) {
			  throw new ApplicationException("Consume Content Source Task threw an exception", e);
			}
		  }

		  protected override String getLogMessage(int recsCount) {
			return "read " + recsCount + " documents from the content source";
		  }
  
		  public override void close() {
			source.close();
			base.close();
		  }

		  public override int doLogic() {
			dd = source.getNextDocData(dd);
			return 1;
		  }
	}
}
