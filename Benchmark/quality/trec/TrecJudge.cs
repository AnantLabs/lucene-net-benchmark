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
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using RTools.Util;

namespace Lucene.Net.Benchmark.quality.trec {

	/**
	 * Judge if given document is relevant to given quality query, based on Trec format for judgements.
	 */
	public class TrecJudge : Judge {

		  Hashtable judgements;
  
		  /**
		   * Constructor from a reader.
		   * <p>
		   * Expected input format:
		   * <pre>
		   *     qnum  0   doc-name     is-relevant
		   * </pre> 
		   * Two sample lines:
		   * <pre> 
		   *     19    0   doc303       1
		   *     19    0   doc7295      0
		   * </pre> 
		   * @param reader where judgments are read from.
		   * @throws IOException 
		   */
		  public TrecJudge (TextReader reader) {
			judgements = new Hashtable();
			QRelJudgement curr = null;
			String zero = "0";
			String line;
    
			try {
			  while (null!=(line=reader.ReadLine())) {
				line = line.Trim();
				if (line.Length==0 || line.StartsWith("#")) {
				  continue;
				}
				StreamTokenizer st = new StreamTokenizer(line);
				RTools.Util.Token t;
				st.NextToken(out t);
				String queryID = t.StringValue;
				st.NextToken(out t);
				st.NextToken(out t);
				String docName = t.StringValue;
				st.NextToken(out t);
				bool relevant = !zero.Equals(t.StringValue);
				if( st.NextToken(out t) ) {
					throw new FormatException("wrong format: " + line + "  next: " + t.StringValue);
				}
				if (relevant) { // only keep relevant docs
				  if (curr==null || !curr.queryID.Equals(queryID)) {
					curr = (QRelJudgement)judgements[queryID];
					if (curr==null) {
					  curr = new QRelJudgement(queryID);
					  judgements.Add(queryID,curr);
					}
				  }
				  curr.addRelevandDoc(docName);
				}
			  }
			} finally {
			  reader.Close();
			}
		  }
  
		  // inherit javadocs
		  public bool isRelevant(String docName, QualityQuery query) {
			QRelJudgement qrj = (QRelJudgement) judgements[query.getQueryID()];
			return qrj!=null && qrj.isRelevant(docName);
		  }

		  /** single Judgement of a trec quality query */
		  private class QRelJudgement {
			public String queryID;
			public StringDictionary relevantDocs;
    
			public QRelJudgement(String queryID) {
			  this.queryID = queryID;
			  relevantDocs = new StringDictionary();
			}
    
			public void addRelevandDoc(String docName) {
			  relevantDocs.Add(docName,docName);
			}

			public bool isRelevant(String docName) {
			  return (relevantDocs[docName].Length != 0);
			}

			public int maxRecall() {
			  return relevantDocs.Count;
			}
		  }

		  // inherit javadocs
		  public bool validateData(QualityQuery[] qq, TextWriter logger) {
			Hashtable missingQueries = (Hashtable) judgements.Clone();
			ArrayList missingJudgements = new ArrayList();
			for (int i=0; i<qq.Length; i++) {
			  String id = qq[i].getQueryID();
			  if (missingQueries[id] != null) {
				missingQueries.Remove(id);
			  } else {
				missingJudgements.Add(id);
			  }
			}
			bool isValid = true;
			if (missingJudgements.Count>0) {
			  isValid = false;
			  if (logger!=null) {
				logger.WriteLine("WARNING: "+missingJudgements.Count+" queries have no judgments! - ");
				for (int i=0; i<missingJudgements.Count; i++) {
				  logger.WriteLine("   "+(String)missingJudgements[i]);
				}
			  }
			}
			if (missingQueries.Count>0) {
			  isValid = false;
			  if (logger!=null) {
				logger.WriteLine("WARNING: "+missingQueries.Count+" judgments match no query! - ");
				  
				foreach(String id in missingQueries.Keys    ) {
				  logger.WriteLine("   "+id);
				}
			  }
			}
			return isValid;
		  }

		  // inherit javadocs
		  public int maxRecall(QualityQuery query) {
			QRelJudgement qrj = (QRelJudgement) judgements[query.getQueryID()];
			if (qrj!=null) {
			  return qrj.maxRecall();
			}
			return 0;
		  }

	}
}
