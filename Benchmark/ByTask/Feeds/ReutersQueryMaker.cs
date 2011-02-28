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

using System;
using System.Collections;
using System.Reflection;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * A QueryMaker that makes queries devised manually (by Grant Ingersoll) for
	 * searching in the Reuters collection.
	 */
	public class ReutersQueryMaker : AbstractQueryMaker, QueryMaker {

		  private static String [] STANDARD_QUERIES = {
				//Start with some short queries
				"Salomon", "Comex", "night trading", "Japan Sony",
				//Try some Phrase Queries
				"\"Sony Japan\"", "\"food needs\"~3",
				"\"World Bank\"^2 AND Nigeria", "\"World Bank\" -Nigeria",
				"\"Ford Credit\"~5",
				//Try some longer queries
				"airline Europe Canada destination",
				"Long term pressure by trade " +
				"ministers is necessary if the current Uruguay round of talks on " +
				"the General Agreement on Trade and Tariffs (GATT) is to " +
				"succeed"
			};
  
			public static Query[] getPrebuiltQueries(String field) {
				//  be wary of unanalyzed text
				return new Query[] {
					new SpanFirstQuery(new SpanTermQuery(new Term(field, "ford")), 5),
					new SpanNearQuery(new SpanQuery[]{new SpanTermQuery(new Term(field, "night")), new SpanTermQuery(new Term(field, "trading"))}, 4, false),
					new SpanNearQuery(new SpanQuery[]{new SpanFirstQuery(new SpanTermQuery(new Term(field, "ford")), 10), new SpanTermQuery(new Term(field, "credit"))}, 10, false),
					new WildcardQuery(new Term(field, "fo*")),
				};
			}
  
			/**
			* Parse the strings containing Lucene queries.
			*
			* @param qs array of strings containing query expressions
			* @param a  analyzer to use when parsing queries
			* @return array of Lucene queries
			*/
			public static ArrayList createQueries(ArrayList qs, Analyzer a) {
				QueryParser qp = new QueryParser(DocMaker.BODY_FIELD, a);
				ArrayList queries = new ArrayList();
				for (int i = 0; i < qs.Count; i++)  {
					try {
        
						Object query = qs[i];
						Query q = null;
						if (query is String) {
							q = qp.Parse((String) query);
          
						} else if (query is Query) {
							q = (Query) query;
          
						} else {
							Console.WriteLine("Unsupported Query Type: " + query);
						}
        
						if (q != null) {
							queries.Add(q);
						}
        
					} catch (Exception e)  {
						Console.WriteLine(e.StackTrace);
					}
				}
				return queries;
			}
  
			protected override ArrayList prepareQueries() {
				// analyzer (default is standard analyzer)
				Assembly assembly = Assembly.LoadFrom( "Lucene.Net.dll" );
				String anlzrName = config.get( "analyzer", "Lucene.Net.Analysis.Standard.StandardAnalyzer" );
				Analyzer anlzr= (Analyzer)assembly.CreateInstance( anlzrName ); 
    
				ArrayList queryList = new ArrayList(20);
				queryList.AddRange( STANDARD_QUERIES );
				queryList.AddRange(getPrebuiltQueries(DocMaker.BODY_FIELD));
				return createQueries(queryList, anlzr);
			}

	}
}
