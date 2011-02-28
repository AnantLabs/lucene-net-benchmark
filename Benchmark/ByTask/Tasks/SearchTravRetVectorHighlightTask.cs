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

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Highlight;
using Lucene.Net.Search.Vectorhighlight;

using Lucene.Net.Benchmark.ByTask;
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Benchmark.ByTask.Stats;
using Lucene.Net.Benchmark.ByTask.Tasks;
using Lucene.Net.Benchmark.ByTask.Utils;

using RTools.Util;
using Kajabity.Tools.Java;
using Kajabity.Tools;
using ICSharpCode.SharpZipLib.BZip2; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Search and Traverse and Retrieve docs task.  Highlight the fields in the retrieved documents by using FastVectorHighlighter.
	 *
	 * <p>Note: This task reuses the reader if it is already open.
	 * Otherwise a reader is opened at start and closed at the end.
	 * </p>
	 *
	 * <p>Takes optional multivalued, comma separated param string as: size[&lt;traversal size&gt;],highlight[&lt;int&gt;],maxFrags[&lt;int&gt;],mergeContiguous[&lt;boolean&gt;],fields[name1;name2;...]</p>
	 * <ul>
	 * <li>traversal size - The number of hits to traverse, otherwise all will be traversed</li>
	 * <li>highlight - The number of the hits to highlight.  Will always be less than or equal to traversal size.  Default is Integer.MAX_VALUE (i.e. hits.length())</li>
	 * <li>maxFrags - The maximum number of fragments to score by the highlighter</li>
	 * <li>fragSize - The length of fragments</li>
	 * <li>fields - The fields to highlight.  If not specified all fields will be highlighted (or at least attempted)</li>
	 * </ul>
	 * Example:
	 * <pre>"SearchVecHlgtSameRdr" SearchTravRetVectorHighlight(size[10],highlight[10],maxFrags[3],fields[body]) > : 1000
	 * </pre>
	 *
	 * Fields must be stored and term vector offsets and positions in order must be true for this task to work.
	 *
	 * <p>Other side effects: counts additional 1 (record) for each traversed hit,
	 * and 1 more for each retrieved (non null) document and 1 for each fragment returned.</p>
	 */
	public class SearchTravRetVectorHighlightTask : SearchTravTask {
		
		  protected int _numToHighlight = int.MaxValue;
		  protected int maxFrags = 2;
		  protected int fragSize = 100;
		  protected List<String> paramFields = new List<string>();
		  protected FastVectorHighlighter highlighter;

		  public SearchTravRetVectorHighlightTask(PerfRunData runData) : base(runData) {
			
		  }

		  public override void setup() {
			base.setup();
			//check to make sure either the doc is being stored
			PerfRunData data = getRunData();
			if (data.getConfig().get("doc.stored", false) == false){
			  throw new Exception("doc.stored must be set to true");
			}
			if (data.getConfig().get("doc.term.vector.offsets", false) == false){
			  throw new Exception("doc.term.vector.offsets must be set to true");
			}
			if (data.getConfig().get("doc.term.vector.positions", false) == false){
			  throw new Exception("doc.term.vector.positions must be set to true");
			}
		  }

		  public override bool withRetrieve() {
			return true;
		  }

		  public override int numToHighlight() {
			return _numToHighlight;
		  }
  
		  protected override BenchmarkHighlighter getBenchmarkHighlighter(Query q){
			highlighter = new FastVectorHighlighter( false, false );
			FieldQuery fq = highlighter.GetFieldQuery( q );
			return new strvhBenchmarkHighlighter(highlighter, fq, fragSize, maxFrags);
		  }

		  protected class strvhBenchmarkHighlighter : BenchmarkHighlighter {

			  private FastVectorHighlighter _highlighter;
			  private FieldQuery _fq;
			  private int _fragSize;
			  private int _maxFrags;

			  public strvhBenchmarkHighlighter (FastVectorHighlighter highlighter, FieldQuery fq, int fragSize, int maxFrags) {
				  _highlighter = highlighter;
				  _fq = fq;
				  _fragSize = fragSize;
				  _maxFrags = maxFrags;
			  }

			  public override int doHighlight( IndexReader reader, int doc, String field,
				  Document document, Analyzer analyzer, String text ) {
				  String[] fragments = _highlighter.GetBestFragments( _fq, reader, doc, field, _fragSize, _maxFrags );
				  return fragments != null ? fragments.Length : 0;
			  }
		  }

		  public override List<String> getFieldsToHighlight(Document document) {
			List<String> result = base.getFieldsToHighlight(document);
			//if stored is false, then result will be empty, in which case just get all the param fields
			if (paramFields.Count != 0 && result.Count != 0) {
			  result.AddRange(paramFields);
			} else {
			  result = paramFields;
			}
			return result;
		  }

		  public override void setParams(String args) {
			String [] splits = args.Split(',');
			for (int i = 0; i < splits.Length; i++) {
			  if (splits[i].StartsWith("size[") == true){
				_traversalSize = (int)float.Parse(splits[i].Substring("size[".Length, splits[i].Length - 1 - "size[".Length));
			  } else if (splits[i].StartsWith("highlight[") == true){
				_numToHighlight = (int)float.Parse(splits[i].Substring("highlight[".Length, splits[i].Length - 1 - "highlight[".Length));
			  } else if (splits[i].StartsWith("maxFrags[") == true){
				maxFrags = (int)float.Parse(splits[i].Substring("maxFrags[".Length, splits[i].Length - 1 - "maxFrags[".Length));
			  } else if (splits[i].StartsWith("fragSize[") == true){
				fragSize = (int)float.Parse(splits[i].Substring("fragSize[".Length, splits[i].Length - 1 - "fragSize[".Length));
			  } else if (splits[i].StartsWith("fields[") == true){
				  paramFields = new List<string>();
				String fieldNames = splits[i].Substring("fields[".Length, splits[i].Length - 1 - "fields[".Length);
				String [] fieldSplits = fieldNames.Split(';');
				for (int j = 0; j < fieldSplits.Length; j++) {
				  paramFields.Add(fieldSplits[j]);          
				}

			  }
			}
		  }
	}
}
