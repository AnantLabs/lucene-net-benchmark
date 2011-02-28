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
using Lucene.Net.Index;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Deletes a percentage of documents from an index randomly
	 * over the number of documents.  The parameter, X, is in
	 * percent.  EG 50 means 1/2 of all documents will be
	 * deleted.
	 *
	 * <p><b>NOTE</b>: the param is an absolute percentage of
	 * maxDoc().  This means if you delete 50%, and then delete
	 * 50% again, the 2nd delete will do nothing.
	 */
	public class DeleteByPercentTask : PerfTask {

		  double percent;
		  int numDeleted = 0;
		  Random random = new Random(DateTime.Now.Millisecond);

		  public DeleteByPercentTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }
  
		  public override void setup() {
			base.setup();
		  }

		  public override void setParams(String args) {
			base.setParams(args);
			percent = Double.Parse(args)/100;
		  }

		  public override bool supportsParams() {
			return true;
		  }

		  public override int doLogic() {
			IndexReader r = getRunData().getIndexReader();
			int maxDoc = r.MaxDoc();
			int numDeleted = 0;
			// percent is an absolute target:
			int numToDelete = ((int) (maxDoc * percent)) - r.NumDeletedDocs();
			if (numToDelete < 0) {
			  r.UndeleteAll();
			  numToDelete = (int) (maxDoc * percent);
			}
			while (numDeleted < numToDelete) {
			  double delRate = ((double) (numToDelete-numDeleted))/r.NumDocs();
			  TermDocs termDocs = r.TermDocs(null);
			  while (termDocs.Next() && numDeleted < numToDelete) {
				if (random.NextDouble() <= delRate) {
				  r.DeleteDocument(termDocs.Doc());
				  numDeleted++;
				}
			  }
			  termDocs.Close();
			}
			Benchmark.LogSheet.AddRowAndCell( "--> processed (delete) " + numDeleted + " docs" );
		  	Console.WriteLine("--> processed (delete) " + numDeleted + " docs");
			return numDeleted;
		  }
	}
}
