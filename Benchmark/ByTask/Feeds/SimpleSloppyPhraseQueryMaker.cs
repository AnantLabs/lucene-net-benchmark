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
using System.Collections.Generic;
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Index;
using Lucene.Net.Search;
using RTools.Util;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * Create sloppy phrase queries for performance test, in an index created using simple doc maker.
	 */
	public class SimpleSloppyPhraseQueryMaker : SimpleQueryMaker {

		  /* (non-Javadoc)
		   * @see org.apache.lucene.benchmark.byTask.feeds.SimpleQueryMaker#prepareQueries()
		   */
		  protected override ArrayList prepareQueries() {
			// extract some 100 words from doc text to an array
			List<String> words;
			List<String> w = new List<String>();
			StreamTokenizer st = new StreamTokenizer(SingleDocSource.DOC_TEXT);
			st.Settings.DoUntermCheck = false;
			st.Settings.OrdinaryChar( '\'' );
			 RTools.Util.Token t; 
			while (st.NextToken(out t) && w.Count<100) {
			  w.Add(t.StringValue);
			}
			words = w;

			// create queries (that would find stuff) with varying slops
			ArrayList queries = new ArrayList(); 
			for (int slop=0; slop<8; slop++) {
			  for (int qlen=2; qlen<6; qlen++) {
				for (int wd=0; wd<words.Count-qlen-slop; wd++) {
				  // ordered
				  int remainedSlop = slop;
				  PhraseQuery q = new PhraseQuery();
				  q.SetSlop(slop);
				  int wind = wd;
				  for (int i=0; i<qlen; i++) {
					q.Add(new Term(DocMaker.BODY_FIELD,words[wind++]));
					if (remainedSlop>0) {
					  remainedSlop--;
					  wind++;
					}
				  }
				  queries.Add(q);
				  // reversed
				  remainedSlop = slop;
				  q = new PhraseQuery();
				  q.SetSlop(slop+2*qlen);
				  wind = wd+qlen+remainedSlop-1;
				  for (int i=0; i<qlen; i++) {
					q.Add(new Term(DocMaker.BODY_FIELD,words[wind--]));
					if (remainedSlop>0) {
					  remainedSlop--;
					  wind--;
					}
				  }
				  queries.Add(q);
				}
			  }
			}
			return queries;
		  }

	}
}


