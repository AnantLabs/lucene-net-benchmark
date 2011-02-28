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
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.quality.utils {

	/**
	 * Utility: extract doc names from an index
	 */
	public class DocNameExtractor {

		  private FieldSelector fldSel;
		  private String docNameField;
  
		  public class dneFieldSelector : FieldSelector {
			  public FieldSelectorResult Accept( String fieldName ) {
				  throw new NotImplementedException("Use the override of Accept with two params");
			  }

			  public FieldSelectorResult Accept(String fieldName, String docNameField) {
				return fieldName.Equals(docNameField) ? 
					FieldSelectorResult.LOAD_AND_BREAK :
					  FieldSelectorResult.NO_LOAD;
			  }
		  }
		  /**
		   * Constructor for DocNameExtractor.
		   * @param docNameField name of the stored field containing the doc name. 
		   */
		  public DocNameExtractor (String docNameField) {
			this.docNameField = docNameField;
			fldSel = new dneFieldSelector();
		  }
  
		  /**
		   * Extract the name of the input doc from the index.
		   * @param searcher access to the index.
		   * @param docid ID of doc whose name is needed.
		   * @return the name of the input doc as extracted from the index.
		   * @throws IOException if cannot extract the doc name from the index.
		   */
		  public String docName(Searcher searcher, int docid) {
			return searcher.Doc(docid,fldSel).Get(docNameField);
		  }
	}
}
