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

using System.IO;
using System.Collections;

using Lucene.Net.Benchmark.ByTask;
using Lucene.Net.Benchmark.ByTask.Tasks;
using Lucene.Net.Benchmark.ByTask.Utils;
using Lucene.Net.Index;
using Lucene.Net.Store;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Open an index reader.
	 * <br>Other side effects: index reader object in perfRunData is set.
	 * <br> Optional params readOnly,commitUserData eg. OpenReader(false,commit1)
	 */
	public class OpenReaderTask : PerfTask {

		  public const String USER_DATA = "userData";
		  private bool readOnly = true;
		  private String commitUserData = null;

		  public OpenReaderTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  public override int doLogic() {
			Lucene.Net.Store.Directory dir = getRunData().getDirectory();
			Config config = getRunData().getConfig();
			IndexReader r = null;
			if (commitUserData != null) {
			  r = openCommitPoint(commitUserData, dir, config, readOnly);
			} else {
			  IndexDeletionPolicy indexDeletionPolicy = CreateIndexTask.getIndexDeletionPolicy(config);
			  r = IndexReader.Open(dir, indexDeletionPolicy, readOnly); 
			}
			getRunData().setIndexReader(r);
			return 1;
		  }
 
		  public static IndexReader openCommitPoint(String userData, Lucene.Net.Store.Directory dir, Config config, bool readOnly) {
			IndexReader r = null;
			ICollection commits = IndexReader.ListCommits(dir);
			foreach (IndexCommit ic in commits) {
			  IDictionary<String, String> map = ic.GetUserData();
			  String ud = null;
			  if (map != null) {
				ud = (String)map[USER_DATA];
			  }
			  if (ud != null && ud.Equals(userData)) {
				IndexDeletionPolicy indexDeletionPolicy = CreateIndexTask.getIndexDeletionPolicy(config);
				r = IndexReader.Open(ic, indexDeletionPolicy, readOnly);
				break;
			  }
			}
			if (r == null) throw new IOException("cannot find commitPoint userData:"+userData);
			return r;
		  }
  
		  public override void setParams(String args) {
			base.setParams(args);
			if (args != null) {
			  String[] split = args.Split(',');
			  if (split.Length > 0) {
				readOnly = Boolean.Parse(split[0]);
			  }
			  if (split.Length > 1) {
				commitUserData = split[1];
			  }
			}
		  }

		  public override bool supportsParams() {
			return true;
		  }

	}
}
