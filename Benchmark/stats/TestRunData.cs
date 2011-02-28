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
using System.Text;

namespace Lucene.Net.Benchmark.stats {

	/**
	 * This class holds series of TimeData related to a single test run. TimeData
	 * values may contribute to different measurements, so this class provides also
	 * some useful methods to separate them.
	 *
	 */
	public class TestRunData {

		private String id;

		/** Start and end time of this test run. */
		private DateTime start = DateTime.Now, end = DateTime.Now;

		private Hashtable data = new Hashtable();

		public TestRunData() { }

		public TestRunData( String id ) {
			this.id = id;
		}

		public Hashtable getData() {
			return data;
		}

		public String getId() {
			return id;
		}

		public void setId( String id ) {
			this.id = id;
		}

		public DateTime getEnd() {
			return end;
		}

		public DateTime getStart() {
			return start;
		}

		/** Mark the starting time of this test run. */
		public void startRun() {
			start = DateTime.Now;
		}

		/** Mark the ending time of this test run. */
		public void endRun() {
			end = DateTime.Now;
		}

		/** Add a data point. */
		public void addData( TimeData td ) {
			td.recordMemUsage();
			List<Object> v = (List<Object>)data[ td.name ];
			if( v == null ) {
				v = new List<Object>();
				data[ td.name ] =  v;
			}
			v.Add( td.clone() );
		}

		/** Get a list of all available types of data points. */
		public String[] getLabels() {
			return (String[])data.Keys;
		}

		/** Get total values from all data points of a given type. */
		public TimeData getTotals( String label ) {
			List<Object> v = (List<Object>)data[ label ];
			if( v == null ) {
				return null;
			}
			TimeData res = new TimeData( "TOTAL " + label );
			for( int i = 0; i < v.Count; i++ ) {
				TimeData td = (TimeData)v[ i ];
				res.count += td.count;
				res.elapsed += td.elapsed;
			}
			return res;
		}

		/** Get total values from all data points of all types.
		 * @return a list of TimeData values for all types.
		 */
		public List<Object> getTotals() {
			String[] labels = getLabels();
			List<Object> v = new List<Object>();
			foreach (String label in labels) {
				TimeData td = getTotals( label );
				v.Add( td );
			}
			return v;
		}

		/** Get memory usage stats for a given data type. */
		public MemUsage getMemUsage( String label ) {
			List<Object> v = (List<Object>)data[ label ];
			if( v == null ) {
				return null;
			}
			MemUsage res = new MemUsage();
			res.minFree = long.MinValue;
			res.minTotal = long.MaxValue;
			long avgFree = 0L, avgTotal = 0L;
			for( int i = 0; i < v.Count; i++ ) {
				TimeData td = (TimeData)v[ i ];
				if( res.maxFree < td.freeMem ) {
					res.maxFree = td.freeMem;
				}
				if( res.maxTotal < td.totalMem ) {
					res.maxTotal = td.totalMem;
				}
				if( res.minFree > td.freeMem ) {
					res.minFree = td.freeMem;
				}
				if( res.minTotal > td.totalMem ) {
					res.minTotal = td.totalMem;
				}
				avgFree += td.freeMem;
				avgTotal += td.totalMem;
			}
			res.avgFree = avgFree / v.Count;
			res.avgTotal = avgTotal / v.Count;
			return res;
		}

		/** Return a string representation. */
		public String toString() {
			StringBuilder sb = new StringBuilder();
			String[] labels = getLabels();
			foreach (String label in labels) {
				sb.Append( id ).Append( "-" ).Append( label ).Append( " " ).Append( getTotals( label ).toString( false ) ).Append( " " );
				sb.Append( getMemUsage( label ).toScaledString( 1024 * 1024, "MB" ) ).Append( "\n" );
			}
			return sb.ToString();
		}
	}
}
