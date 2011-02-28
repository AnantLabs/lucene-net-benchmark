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
	 * Delete a document by docid. If no docid param is supplied, deletes doc with
	 * <code>id = last-deleted-doc + doc.delete.step</code>.
	 */
	public class DeleteDocTask : PerfTask {

		 /**
		* Gap between ids of deleted docs, applies when no docid param is provided.
		*/
		public const int DEFAULT_DOC_DELETE_STEP = 8;
  
		public DeleteDocTask(PerfRunData runData) : base(runData) {
			// do nothing
		}

		private int deleteStep = -1;
		private static int lastDeleted = -1;

		private int docid = -1;
		private bool byStep = true;
  
		public override int doLogic() {
			getRunData().getIndexReader().DeleteDocument(docid);
			lastDeleted = docid;
			return 1; // one work item done here
		}

		/* (non-Javadoc)
		* @see org.apache.lucene.benchmark.byTask.tasks.PerfTask#setup()
		*/
		public override void setup() {
			base.setup();
			if (deleteStep<0) {
				deleteStep = getRunData().getConfig().get("doc.delete.step",DEFAULT_DOC_DELETE_STEP);
			}
			// set the docid to be deleted
			docid = (byStep ? lastDeleted + deleteStep : docid);
		}

		protected override String getLogMessage(int recsCount) {
			return "deleted " + recsCount + " docs, last deleted: " + lastDeleted;
		}
  
		/**
		* Set the params (docid only)
		* @param params docid to delete, or -1 for deleting by delete gap settings.
		*/
		public override void setParams(String args) {
			base.setParams(args);
			docid = (int) float.Parse(args);
			byStep = (docid < 0);
		}
  
		/* (non-Javadoc)
		* @see org.apache.lucene.benchmark.byTask.tasks.PerfTask#supportsParams()
		*/
		public override bool supportsParams() {
			return true;
		  }
	}
}
