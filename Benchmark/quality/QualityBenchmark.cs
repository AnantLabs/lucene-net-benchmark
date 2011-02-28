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
using System.IO;
using Lucene.Net.Benchmark.quality.utils;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.quality {

	/**
	 * Main entry point for running a quality benchmark.
	 * <p>
	 * There are two main configurations for running a quality benchmark: <ul>
	 * <li>Against existing judgements.</li>
	 * <li>For submission (e.g. for a contest).</li>
	 * </ul>
	 * The first configuration requires a non null
	 * {@link org.apache.lucene.benchmark.quality.Judge Judge}. 
	 * The second configuration requires a non null 
	 * {@link org.apache.lucene.benchmark.quality.utils.SubmissionReport SubmissionLogger}.
	 */
	public class QualityBenchmark {

		 /** Quality Queries that this quality benchmark would execute. */
		  protected QualityQuery[] qualityQueries;
  
		  /** Parser for turning QualityQueries into Lucene Queries. */
		  protected QualityQueryParser qqParser;
  
		  /** Index to be searched. */
		  protected Searcher searcher;

		  /** index field to extract doc name for each search result; used for judging the results. */  
		  protected String docNameField;
  
		  /** maximal number of queries that this quality benchmark runs. Default: maxint. Useful for debugging. */
		  private int maxQueries = int.MaxValue;
  
		  /** maximal number of results to collect for each query. Default: 1000. */
		  private int maxResults = 1000;

		  /**
		   * Create a QualityBenchmark.
		   * @param qqs quality queries to run.
		   * @param qqParser parser for turning QualityQueries into Lucene Queries. 
		   * @param searcher index to be searched.
		   * @param docNameField name of field containing the document name.
		   *        This allows to extract the doc name for search results,
		   *        and is important for judging the results.  
		   */
		  public QualityBenchmark(QualityQuery[] qqs, QualityQueryParser qqParser, 
			  Searcher searcher, String docNameField) {
			this.qualityQueries = qqs;
			this.qqParser = qqParser;
			this.searcher = searcher;
			this.docNameField = docNameField;
		  }

		  /**
		   * Run the quality benchmark.
		   * @param judge the judge that can tell if a certain result doc is relevant for a certain quality query. 
		   *        If null, no judgements would be made. Usually null for a submission run. 
		   * @param submitRep submission report is created if non null.
		   * @param qualityLog If not null, quality run data would be printed for each query.
		   * @return QualityStats of each quality query that was executed.
		   * @throws Exception if quality benchmark failed to run.
		   */
		  public  QualityStats[] execute(Judge judge, SubmissionReport submitRep, 
										  TextWriter qualityLog) {
			int nQueries = Math.Min(maxQueries, qualityQueries.Length);
			QualityStats[] stats = new QualityStats[nQueries]; 
			for (int i=0; i<nQueries; i++) {
			  QualityQuery qq = qualityQueries[i];
			  // generate query
			  Query q = qqParser.parse(qq);
			  // search with this query 
			  DateTime t1 = DateTime.Now;
			  TopDocs td = searcher.Search(q,null,maxResults);
			  long searchTime = (DateTime.Now-t1).Milliseconds;
			  //most likely we either submit or judge, but check both 
			  if (judge!=null) {
				stats[i] = analyzeQueryResults(qq, q, td, judge, qualityLog, searchTime);
			  }
			  if (submitRep!=null) {
				submitRep.report(qq,td,docNameField,searcher);
			  }
			} 
			if (submitRep!=null) {
			  submitRep.flush();
			}
			return stats;
		  }
  
		  /* Analyze/judge results for a single quality query; optionally log them. */  
		  private QualityStats analyzeQueryResults(QualityQuery qq, Query q, TopDocs td, Judge judge, TextWriter logger, long searchTime) {
			QualityStats stts = new QualityStats(judge.maxRecall(qq),searchTime);
			ScoreDoc[] sd = td.scoreDocs;
			DateTime t1 = DateTime.Now; // extraction of first doc name we measure also construction of doc name extractor, just in case.
			DocNameExtractor xt = new DocNameExtractor(docNameField);
			for (int i=0; i<sd.Length; i++) {
			  String docName = xt.docName(searcher,sd[i].doc);
			  long docNameExtractTime = (DateTime.Now - t1).Milliseconds;
			  t1 = DateTime.Now;
			  bool isRelevant = judge.isRelevant(docName,qq);
			  stts.addResult(i+1,isRelevant, docNameExtractTime);
			}
			if (logger!=null) {
			  logger.WriteLine(qq.getQueryID()+"  -  "+q);
			  stts.log(qq.getQueryID()+" Stats:",1,logger,"  ");
			}
			return stts;
		  }

		  /**
		   * @return the maximum number of quality queries to run. Useful at debugging.
		   */
		  public int getMaxQueries() {
			return maxQueries;
		  }

		  /**
		   * Set the maximum number of quality queries to run. Useful at debugging.
		   */
		  public void setMaxQueries(int maxQueries) {
			this.maxQueries = maxQueries;
		  }

		  /**
		   * @return the maximum number of results to collect for each quality query.
		   */
		  public int getMaxResults() {
			return maxResults;
		  }

		  /**
		   * set the maximum number of results to collect for each quality query.
		   */
		  public void setMaxResults(int maxResults) {
			this.maxResults = maxResults;
		  }

	}
}
