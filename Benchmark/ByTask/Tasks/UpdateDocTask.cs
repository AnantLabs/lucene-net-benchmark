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
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Update a document, using IndexWriter.updateDocument,
	 * optionally with of a certain size.
	 * <br>Other side effects: none.
	 * <br>Takes optional param: document size. 
	 */
	public class UpdateDocTask : PerfTask {

		  public UpdateDocTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  private int docSize = 0;
  
		  // volatile data passed between setup(), doLogic(), tearDown().
		  private Document doc = null;
  
		  public override void setup() {
			base.setup();
			DocMaker docMaker = getRunData().getDocMaker();
			if (docSize > 0) {
			  doc = docMaker.makeDocument(docSize);
			} else {
			  doc = docMaker.makeDocument();
			}
		  }

		  public override void tearDown() {
			doc = null;
			base.tearDown();
		  }

		  public override int doLogic() {
			String docID = doc.Get(DocMaker.ID_FIELD);
			if (docID == null) {
			  throw new MissingFieldException("document must define the docid field");
			}
			getRunData().getIndexWriter().UpdateDocument(new Term(DocMaker.ID_FIELD, docID), doc);
			return 1;
		  }

		  protected override String getLogMessage(int recsCount) {
			return "updated " + recsCount + " docs";
		  }
  
		  /**
		   * Set the params (docSize only)
		   * @param params docSize, or 0 for no limit.
		   */
		  public override void setParams(String args) {
			base.setParams(args);
			docSize = (int) float.Parse(args); 
		  }

		  /* (non-Javadoc)
		   * @see org.apache.lucene.benchmark.byTask.tasks.PerfTask#supportsParams()
		   */
		  public override bool supportsParams() {
			return true;
		  }

	}
}
