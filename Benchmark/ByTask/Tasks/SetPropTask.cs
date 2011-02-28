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

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Set a performance test configuration property.
	 * A property may have a single value, or a sequence of values, separated by ":". 
	 * If a sequence of values is specified, each time a new round starts, 
	 * the next (cyclic) value is taken.  
	 * <br>Other side effects: none.
	 * <br>Takes mandatory param: "name,value" pair. 
	 * @see org.apache.lucene.benchmark.byTask.tasks.NewRoundTask
	 */
	public class SetPropTask : PerfTask {
	
		  public SetPropTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  private String name;
		  private String value;
  
		  public override int doLogic() {
			if (name==null || value==null) {
			  throw new Exception(getName()+" - undefined name or value: name="+name+" value="+value);
			}
			getRunData().getConfig().Set(name,value);
			return 0;
		  }

		  /**
		   * Set the params (property name and value).
		   * @param params property name and value separated by ','.
		   */
		  public override void setParams(String args) {
			base.setParams(args);
			int k = args.IndexOf(",");
			name = args.Substring(0,k).Trim();
			value = args.Substring(k+1).Trim();
		  }

		  /* (non-Javadoc)
		   * @see org.apache.lucene.benchmark.byTask.tasks.PerfTask#supportsParams()
		   */
		  public override bool supportsParams() {
			return true;
		  }

	}
}
