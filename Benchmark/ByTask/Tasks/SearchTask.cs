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

using Lucene.Net.Benchmark.ByTask.Feeds;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Search task.
	 * 
	 * <p>Note: This task reuses the reader if it is already open. 
	 * Otherwise a reader is opened at start and closed at the end.
	 */
	public class SearchTask : ReadTask {

		public SearchTask( PerfRunData runData ) : base( runData ) {
			
		}

		public override bool withRetrieve() {
			return false;
		}

		public override bool withSearch() {
			return true;
		}

		public override bool withTraverse() {
			return false;
		}

		public override bool withWarm() {
			return false;
		}

		public override QueryMaker getQueryMaker() {
			return getRunData().getQueryMaker( this );
		}
	}
}
