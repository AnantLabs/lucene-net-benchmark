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
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Lucene.Net.Benchmark.quality.utils {

	/**
	 * Suggest Quality queries based on an index contents.
	 * Utility class, used for making quality test benchmarks.
	 */
	public class QualityQueriesFinder {

		  private static readonly String newline = Environment.NewLine;
		  private Lucene.Net.Store.Directory dir;
  
		  /**
		   * Constructor over a directory containing the index.
		   * @param dir directory containing the index we search for the quality test. 
		   */
		  private QualityQueriesFinder(Lucene.Net.Store.Directory dir) {
			this.dir = dir;
			
		  }

		  /**
		   * @param args {index-dir}
		   * @throws IOException  if cannot access the index.
		   */
		  public static void main(String[] args) {

			System.IO.StreamWriter errWriter;
			errWriter = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
			errWriter.AutoFlush = true;

			if (args.Length<1) {
			  errWriter.WriteLine("Usage: java QualityQueriesFinder <index-dir>");
			  return;
			}
			QualityQueriesFinder qqf = new QualityQueriesFinder(FSDirectory.Open(new DirectoryInfo(args[0]), new NativeFSLockFactory()));
			String[] q = qqf.bestQueries("body",20);
			for (int i=0; i<q.Length; i++) {
			  Console.WriteLine(newline+formatQueryAsTrecTopic(i,q[i],null,null));
			}
		  }

		  private String [] bestQueries(String field,int numQueries) {
			String[] words = bestTerms("body",4*numQueries);
			int n = words.Length;
			int m = n/4;
			String[] res = new String[m];
			for (int i=0; i<res.Length; i++) {
			  res[i] = words[i] + " " + words[m+i]+ "  " + words[n-1-m-i]  + " " + words[n-1-i];
			  //System.out.println("query["+i+"]:  "+res[i]);
			}
			return res;
		  }
  
		  private static String formatQueryAsTrecTopic (int qnum, String title, String description, String narrative) {
			return 
			  "<top>" + newline +
			  "<num> Number: " + qnum             + newline + newline + 
			  "<title> " + (title==null?"":title) + newline + newline + 
			  "<desc> Description:"               + newline +
			  (description==null?"":description)  + newline + newline +
			  "<narr> Narrative:"                 + newline +
			  (narrative==null?"":narrative)      + newline + newline +
			  "</top>";
		  }
  
		  private String [] bestTerms(String field,int numTerms) {
			ArrayList pq = new ArrayList();
			IndexReader ir = IndexReader.Open(dir);
			try {
			  int threshold = ir.MaxDoc() / 10; // ignore words too common.
			  TermEnum terms = ir.Terms(new Term(field,""));
			  while (terms.Next()) {
				if (!field.Equals(terms.Term().Field())) {
				  break;
				}
				int df = terms.DocFreq();
				if (df<threshold) {
				  String ttxt = terms.Term().Text();
				  pq.Add(new TermDf(ttxt,df));
				}
			  }
			} finally {
			  ir.Close();
			}
			String[] res = new String[pq.Count];
			int i = 0;
			pq.Sort();
			foreach(TermDf tdf in pq) {
			  res[i++] = tdf.word;
			 Console.WriteLine(i+".   word:  "+tdf.df+"   "+tdf.word);
			}
			return res;
		  }

		  private class TermDf : IComparable {
			public String word;
			public int df;
			public TermDf (String word, int freq) {
			  this.word = word;
			  this.df = freq;
			}

			#region IComparable Members

			public int CompareTo( object obj ) {
				return this.df.CompareTo( (int)obj );
			}

			#endregion
		  }
  
		  //private class TermsDfQueue : SortedDictionary<int, TermDf> {
		  //  public TermsDfQueue (int maxSize) {
		  //      // initialize(maxSize);
		  //  }
		  //  protected bool lessThan(Object a, Object b) {
		  //    TermDf tf1 = (TermDf) a;
		  //    TermDf tf2 = (TermDf) b;
		  //    return tf1.df < tf2.df;
		  //  }
		  //}
	}
}
