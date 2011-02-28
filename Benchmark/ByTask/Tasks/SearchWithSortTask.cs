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
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Does sort search on specified field.
	 * 
	 */
	public class SearchWithSortTask : ReadTask {

		private bool doScore = true;
		private bool doMaxScore = true;
		private Sort sort;

		public SearchWithSortTask( PerfRunData runData ) : base(runData) {
			// do nothing
		}

		/**
		 * SortFields: field:type,field:type[,noscore][,nomaxscore]
		 *
		 * If noscore is present, then we turn off score tracking
		 * in {@link org.apache.lucene.search.TopFieldCollector}.
		 * If nomaxscore is present, then we turn off maxScore tracking
		 * in {@link org.apache.lucene.search.TopFieldCollector}.
		 * 
		 * name:string,page:int,subject:string
		 * 
		 */
		public override void setParams( String sortField ) {
			base.setParams( sortField );
			String[] fields = sortField.Split( ',' );
			SortField[] sortFields = new SortField[ fields.Length ];
			int upto = 0;
			for( int i = 0; i < fields.Length; i++ ) {
				String field = fields[ i ];
				SortField sortField0;
				if( field.Equals( "doc" ) ) {
					sortField0 = SortField.FIELD_DOC;
				}
				if( field.Equals( "score" ) ) {
					sortField0 = SortField.FIELD_SCORE;
				}
				else if( field.Equals( "noscore" ) ) {
					doScore = false;
					continue;
				}
				else if( field.Equals( "nomaxscore" ) ) {
					doMaxScore = false;
					continue;
				}
				else {
					int index = field.LastIndexOf( ":" );
					String fieldName;
					String typeString;
					if( index != -1 ) {
						fieldName = field.Substring( 0, index );
						typeString = field.Substring( 1 + index, field.Length - (1 + index) );
					}
					else {
						throw new ApplicationException( "You must specify the sort type ie page:int,subject:string" );
					}
					int type = getType( typeString );
					sortField0 = new SortField( fieldName, type );
				}
				sortFields[ upto++ ] = sortField0;
			}

			if( upto < sortFields.Length ) {
				SortField[] newSortFields = new SortField[ upto ];
				Array.Copy( sortFields, 0, newSortFields, 0, upto );
				sortFields = newSortFields;
			}
			this.sort = new Sort( sortFields );
		}

		public int getType( String typeString ) {
			int type;
			if( typeString.Equals( "float" ) ) {
				type = SortField.FLOAT;
			}
			else if( typeString.Equals( "double" ) ) {
				type = SortField.DOUBLE;
			}
			else if( typeString.Equals( "byte" ) ) {
				type = SortField.BYTE;
			}
			else if( typeString.Equals( "short" ) ) {
				type = SortField.SHORT;
			}
			else if( typeString.Equals( "int" ) ) {
				type = SortField.INT;
			}
			else if( typeString.Equals( "long" ) ) {
				type = SortField.LONG;
			}
			else if( typeString.Equals( "string" ) ) {
				type = SortField.STRING;
			}
			else if( typeString.Equals( "string_val" ) ) {
				type = SortField.STRING_VAL;
			}
			else {
				throw new ApplicationException( "Unrecognized sort field type " + typeString );
			}
			return type;
		}

		public override bool supportsParams() {
			return true;
		}

		public override QueryMaker getQueryMaker() {
			return getRunData().getQueryMaker( this );
		}

		public override bool withRetrieve() {
			return false;
		}

		public override bool withSearch() {
			return true;
		}

		public override bool withTraverse() {
			return false;
		}

		public override bool withWarm() {
			return false;
		}

		public override bool withScore() {
			return doScore;
		}

		public override bool withMaxScore() {
			return doMaxScore;
		}

		public override Sort getSort() {
			if( sort == null ) {
				throw new MissingFieldException( "No sort field was set" );
			}
			return sort;
		}

	}
}
