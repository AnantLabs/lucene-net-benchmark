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

using System.Collections;
using System.Reflection;

using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * A QueryMaker that makes queries for a collection created 
	 * using {@link org.apache.lucene.benchmark.byTask.feeds.SingleDocSource}.
	 */
	public class SimpleQueryMaker : AbstractQueryMaker, QueryMaker {

		  /**
		   * Prepare the queries for this test.
		   * Extending classes can override this method for preparing different queries. 
		   * @return prepared queries.
		   * @throws Exception if cannot prepare the queries.
		   */


		  protected override ArrayList prepareQueries() {
			// analyzer (default is standard analyzer)
			  // TODO: assembly stuff
			Assembly assembly = Assembly.LoadFrom( "Lucene.Net.dll" );

			string analyzerName = config.get( "analyzer", "Lucene.Net.Analysis.Standard.StandardAnalyzer" );

			Analyzer anlzr = (Analyzer)assembly.CreateInstance( analyzerName );
    
			QueryParser qp = new QueryParser(DocMaker.BODY_FIELD,anlzr);
			ArrayList qq = new ArrayList();
			Query q1 = new TermQuery(new Term(DocMaker.ID_FIELD,"doc2"));
			qq.Add(q1);
			Query q2 = new TermQuery(new Term(DocMaker.BODY_FIELD,"simple"));
			qq.Add(q2);
			BooleanQuery bq = new BooleanQuery();

			bq.Add( q1, BooleanClause.Occur.MUST );
			bq.Add( q2, BooleanClause.Occur.MUST );
			qq.Add(bq);
			qq.Add(qp.Parse("synthetic body"));
			qq.Add(qp.Parse("\"synthetic body\""));
			qq.Add(qp.Parse("synthetic text"));
			qq.Add(qp.Parse("\"synthetic text\""));
			qq.Add(qp.Parse("\"synthetic text\"~3"));
			qq.Add(qp.Parse("zoom*"));
			qq.Add(qp.Parse("synth*"));
			return qq; 
		  }

	}
}
