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
using System.Globalization;
using System.IO;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.quality.utils {

	/**
	 * Create a log ready for submission.
	 * Extend this class and override
	 * {@link #report(QualityQuery, TopDocs, String, Searcher)}
	 * to create different reports. 
	 */
	public class SubmissionReport {

		  private NumberFormatInfo nf;
		  private TextWriter logger;
		  private String name;
  
		  /**
		   * Constructor for SubmissionReport.
		   * @param logger if null, no submission data is created. 
		   * @param name name of this run.
		   */
		  public SubmissionReport (TextWriter logger, String name) {
			this.logger = logger;
			this.name = name;
			nf = new NumberFormatInfo();
			nf.NumberDecimalDigits = 4;
		  }
  
		  /**
		   * Report a search result for a certain quality query.
		   * @param qq quality query for which the results are reported.
		   * @param td search results for the query.
		   * @param docNameField stored field used for fetching the result doc name.  
		   * @param searcher index access for fetching doc name.
		   * @throws IOException in case of a problem.
		   */
		  public void report(QualityQuery qq, TopDocs td, String docNameField, Searcher searcher) {
			if (logger==null) {
			  return;
			}
			ScoreDoc[] sd = td.scoreDocs;
			String sep = " \t ";
			DocNameExtractor xt = new DocNameExtractor(docNameField);
			for (int i=0; i<sd.Length; i++) {
			  String docName = xt.docName(searcher,sd[i].doc);
			  logger.WriteLine(
				  qq.getQueryID()       + sep +
				  "Q0"                   + sep +
				  format(docName,20)    + sep +
				  format(""+i,7)        + sep +
				  sd[i].score.ToString(nf) + sep +
				  name
				  );
			}
		  }

		  public void flush() {
			if (logger!=null) {
			  logger.Flush();
			}
		  }
  
		  private static String padd = "                                    ";
		  private String format(String s, int minLen) {
			s = (s==null ? "" : s);
			int n = Math.Max(minLen,s.Length);
			return (s+padd).Substring(0,n);
		  }
	}
}
