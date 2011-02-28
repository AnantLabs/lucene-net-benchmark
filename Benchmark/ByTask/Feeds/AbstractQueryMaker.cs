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
using System.Runtime.CompilerServices;
using System.Text;
using Lucene.Net.Benchmark.ByTask.Utils;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * Abstract base query maker. 
	 * Each query maker should just implement the {@link #prepareQueries()} method.
	 **/
	 public abstract class AbstractQueryMaker {

		protected int qnum = 0;
		protected ArrayList queries;
		protected Config config;

		public virtual void resetInputs() {
			qnum = 0;
		}

		protected abstract ArrayList prepareQueries();
		 // throws Exception

		public virtual void setConfig(Config config){
			this.config = config;
			queries = prepareQueries();
			// throws Exception 
		}

		public virtual String printQueries() {
			String newline = Environment.NewLine;
			StringBuilder sb = new StringBuilder();
			if (queries != null) {
				for (int i = 0; i < queries.Count; i++) {
				sb.Append(i+". "+ Format.simpleName(queries[i].GetType())+" - "+queries[i].ToString());
				sb.Append(newline);
				}
			}
			return sb.ToString();
		}

		public virtual Query makeQuery() {
			return (Query)queries[nextQnum()];
			//
		}
  
		// return next qnum
		 [MethodImpl(MethodImplOptions.Synchronized)]
		protected virtual  int nextQnum() {
			int res = qnum;
			qnum = (qnum+1) % queries.Count;
			return res;
		}

		/*
		*  (non-Javadoc)
		* @see org.apache.lucene.benchmark.byTask.feeds.QueryMaker#makeQuery(int)
		*/
		public virtual Query makeQuery(int size){
			throw new Exception(this+".makeQuery(int size) is not supported!");
		}
	}
}
