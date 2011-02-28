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
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.quality.utils {

	/**
	 * Simplistic quality query parser. A Lucene query is created by passing 
	 * the value of the specified QualityQuery name-value pair into 
	 * a Lucene's QueryParser using StandardAnalyzer. */
	public class SimpleQQParser : QualityQueryParser {

		  private String qqName;
		  private String indexField;
		  [ThreadStatic]
		  public static QueryParser qp;

		  /**
		   * Constructor of a simple qq parser.
		   * @param qqName name-value pair of quality query to use for creating the query
		   * @param indexField corresponding index field  
		   */
		  public SimpleQQParser(String qqName, String indexField) {
			this.qqName = qqName;
			this.indexField = indexField;
		  }

		  /* (non-Javadoc)
		   * @see Lucene.Benchmark.quality.QualityQueryParser#parse(Lucene.Benchmark.quality.QualityQuery)
		   */
		  public override Query parse(QualityQuery qq)  {
			if (qp==null) {
			  qp = new QueryParser(indexField, new StandardAnalyzer());
			}
			return qp.Parse(qq.getValue(qqName));
		  }
	}
}
