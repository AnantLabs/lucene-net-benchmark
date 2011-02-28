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
using Lucene.Net.Analysis;
using Lucene.Net.Benchmark.ByTask;
using Lucene.Net.Benchmark.ByTask.Tasks;
using Lucene.Net.Documents;
using Lucene.Net.Highlight;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Search and Traverse and Retrieve docs task.  Highlight the fields in the retrieved documents.
	 *
	 * Uses the {@link org.apache.lucene.search.highlight.SimpleHTMLFormatter} for formatting.
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
	 * <li>mergeContiguous - true if contiguous fragments should be merged.</li>
	 * <li>fields - The fields to highlight.  If not specified all fields will be highlighted (or at least attempted)</li>
	 * </ul>
	 * Example:
	 * <pre>"SearchHlgtSameRdr" SearchTravRetHighlight(size[10],highlight[10],mergeContiguous[true],maxFrags[3],fields[body]) > : 1000
	 * </pre>
	 *
	 * Documents must be stored in order for this task to work.  Additionally, term vector positions can be used as well.
	 *
	 * <p>Other side effects: counts additional 1 (record) for each traversed hit,
	 * and 1 more for each retrieved (non null) document and 1 for each fragment returned.</p>
	 */
	public class SearchTravRetHighlightTask : SearchTravTask {

		protected int numToHighlight = int.MaxValue;
		protected bool mergeContiguous;
		protected int maxFrags = 2;
		protected List<String> paramFields = new List<String>();
		protected Highlighter highlighter;
		protected int maxDocCharsToAnalyze;
		protected float _traversalSize;

		public SearchTravRetHighlightTask( PerfRunData runData )
			: base( runData ) {
			// do nothing
		}

		public override void setup() {
			base.setup();
			//check to make sure either the doc is being stored
			PerfRunData data = getRunData();
			if( data.getConfig().get( "doc.stored", false ) == false ) {
				throw new Exception( "doc.stored must be set to true" );
			}
			maxDocCharsToAnalyze = data.getConfig().get( "highlighter.maxDocCharsToAnalyze", Highlighter.DEFAULT_MAX_DOC_BYTES_TO_ANALYZE );
		}

		public override bool withRetrieve() {
			return true;
		}

		public int NumToHighlight() {
			return numToHighlight;
		}


		// This is a little messy, but it'll work
		public class strhtBenchmarkHighlighter : BenchmarkHighlighter {

			Highlighter hl;
			Boolean mc;
			int mf;

			public strhtBenchmarkHighlighter( Highlighter _hl, Boolean _mc, int _mf ) {
				hl = _hl;
				this.mc = _mc;
				this.mf = _mf;
			}

			public override int doHighlight( IndexReader reader, int doc, String field,
				  Document document, Analyzer analyzer, String text ) {
				TokenStream ts = TokenSources.GetAnyTokenStream( reader, doc, field, analyzer );
				TextFragment[] frag = hl.GetBestTextFragments( ts, text, mc, mf );
				return frag != null ? frag.Length : 0;
			}

		}

		protected override BenchmarkHighlighter getBenchmarkHighlighter( Query q ) {
			highlighter = new Highlighter( new SimpleHTMLFormatter(), new QueryScorer( q ) );
			highlighter.SetMaxDocBytesToAnalyze( maxDocCharsToAnalyze );
			return new strhtBenchmarkHighlighter( highlighter, mergeContiguous, maxFrags );
		}

		public override List<String> getFieldsToHighlight( Document document ) {
			List<String> result = base.getFieldsToHighlight( document );
			//if stored is false, then result will be empty, in which case just get all the param fields
			if( paramFields.Count == 0 && result.Count == 0 ) {
				result.AddRange( paramFields );
			}
			else {
				result = paramFields;
			}
			return result;
		}

		public override void setParams( String args ) {
			String[] splits = args.Split( ',' );
			for( int i = 0; i < splits.Length; i++ ) {
				if( splits[ i ].StartsWith( "size[" ) == true ) {
					_traversalSize = (int)float.Parse( splits[ i ].Substring( "size[".Length, (splits[ i ].Length - 1) - "size[".Length ) );
				}
				else if( splits[ i ].StartsWith( "highlight[" ) == true ) {
					numToHighlight = (int)float.Parse( splits[ i ].Substring( "highlight[".Length, (splits[ i ].Length - 1) - "highlight[".Length ) );
				}
				else if( splits[ i ].StartsWith( "maxFrags[" ) == true ) {
					maxFrags = (int)float.Parse( splits[ i ].Substring( "maxFrags[".Length, (splits[ i ].Length - 1) - "maxFrags[".Length ) );
				}
				else if( splits[ i ].StartsWith( "mergeContiguous[" ) == true ) {
					mergeContiguous = Boolean.Parse( splits[ i ].Substring( "mergeContiguous[".Length, (splits[ i ].Length - 1 ) - "mergeContiguous[".Length) );
				}
				else if( splits[ i ].StartsWith( "fields[" ) == true ) {
					paramFields = new List<String>();
					String fieldNames = splits[ i ].Substring( "fields[".Length, (splits[ i ].Length - 1) - "fields[".Length );
					String [] fieldSplits = fieldNames.Split( ';' );
					for( int j = 0; j < fieldSplits.Length; j++ ) {
						paramFields.Add( fieldSplits[ j ] );
					}

				}
			}
		}
	}
}
