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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kajabity.Tools.Java;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/** Output of parsing (e.g. HTML parsing) of an input document. */
	public class DocData {

		private String name;
		private String body;
		private String title;
		private DateTime date;
		private JavaProperties props;

		public virtual void clear() {
			name = null;
			body = null;
			title = null;
			// can't have null time objects so we'll just leave default
			//date = null;
			props = null;
		}

		public String getBody() {
			return body;
		}

		/**
		 * @return the date. If the ctor with Date was called, then the String
		 *         returned is the output of
		 *         {@link DateTools#dateToString(Date, org.apache.lucene.document.DateTools.Resolution)}
		 *         . Otherwise it's the String passed to the other ctor.
		 */
		public DateTime getDate() {
			return date;
		}

		public String getName() {
			return name;
		}

		public JavaProperties getProps() {
			return props;
		}

		public String getTitle() {
			return title;
		}

		public void setBody( String body ) {
			this.body = body;
		}

		public void setDate( DateTime date ) {
			if( date != null ) {
				// TODO: not sure what to do with this.  Looks like he's reformating
				// a string if it doesn't contain seconds, but for now I'll just
				// set what was passed because I'm using idomatic C# time
				//setDate( DateTools.dateToString( date, DateTools.Resolution.SECOND ) );
				this.date = date;
			}
			else {
				// can't do this either, so we'll just try and leave default
				//this.date = null;
			}
		}

		public void setDate( String date ) {
			this.date = DateTime.Parse(date);
		}

		public void setName( String name ) {
			this.name = name;
		}

		public void setProps( JavaProperties props ) {
			this.props = props;
		}

		public void setTitle( String title ) {
			this.title = title;
		}
	}
}
