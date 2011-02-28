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
using Lucene.Net.Benchmark.ByTask.Feeds;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Search and Traverse task.
	 * 
	 * <p>Note: This task reuses the reader if it is already open. 
	 * Otherwise a reader is opened at start and closed at the end.
	 * <p/>
	 * 
	 * <p>Takes optional param: traversal size (otherwise all results are traversed).</p>
	 * 
	 * <p>Other side effects: counts additional 1 (record) for each traversed hit.</p>
	 */
	public class SearchTravTask : ReadTask {

		  protected int _traversalSize = int.MaxValue;

		  public SearchTravTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  public override bool withRetrieve() {
			return false;
		  }

		  public override bool withSearch() {
			return true;
		  }

		  public override bool withTraverse() {
			return true;
		  }

		  public override bool withWarm() {
			return false;
		  }

		  public override QueryMaker getQueryMaker() {
			return getRunData().getQueryMaker(this);
		  }

		  public override int traversalSize() {
			return _traversalSize;
		  }

		  public override void setParams(String args) {
			base.setParams(args);
			_traversalSize = (int)float.Parse(args);
		  }

		  /* (non-Javadoc)
		   * @see org.apache.lucene.benchmark.byTask.tasks.PerfTask#supportsParams()
		   */
		  public override bool supportsParams() {
			return true;
		  }
	}
}
