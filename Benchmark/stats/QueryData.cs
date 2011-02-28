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
using System.Collections.Generic;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.stats {

	/**
	 * This class holds parameters for a query benchmark.
	 *
	 */
	public class QueryData {
		/** Benchmark id */
		public String id;
		/** Lucene query */
		public Query q;
		/** If true, re-open index reader before benchmark. */
		public bool reopen;
		/** If true, warm-up the index reader before searching by sequentially
		 * retrieving all documents from index.
		 */
		public bool warmup;
		/**
		 * If true, actually retrieve documents returned in Hits.
		 */
		public bool retrieve;

		/**
		 * Prepare a list of benchmark data, using all possible combinations of
		 * benchmark parameters.
		 * @param queries source Lucene queries
		 * @return The QueryData
		 */
		public static QueryData[] getAll( Query[] queries ) {
			List<QueryData> vqd = new List<QueryData>();
			for( int i = 0; i < queries.Length; i++ ) {
				for( int r = 1; r >= 0; r-- ) {
					for( int w = 1; w >= 0; w-- ) {
						for( int t = 0; t < 2; t++ ) {
							QueryData qd = new QueryData();
							qd.id = "qd-" + i + r + w + t;
							qd.reopen = Constants.BOOLEANS[ r ];
							qd.warmup = Constants.BOOLEANS[ w ];
							qd.retrieve = Constants.BOOLEANS[ t ];
							qd.q = queries[ i ];
							vqd.Add( qd );
						}
					}
				}
			}
			return (QueryData[])vqd.ToArray();
		}

		/** Short legend for interpreting toString() output. */
		public static String getLabels() {
			return "# Query data: R-reopen, W-warmup, T-retrieve, N-no";
		}

		public String toString() {
			return id + " " + ( reopen ? "R" : "NR" ) + " " + ( warmup ? "W" : "NW" ) +
			  " " + ( retrieve ? "T" : "NT" ) + " [" + q.ToString() + "]";
		}
	}
}
