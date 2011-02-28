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
using System.Text;

namespace Lucene.Net.Benchmark.stats {
	
	/**
	 * This class holds a set of memory usage values.
	 *
	 */
	public class MemUsage {

		public long maxFree, minFree, avgFree;

		public long maxTotal, minTotal, avgTotal;

		public String toString() {
			return toScaledString( 1, "B" );
		}

		/** Scale down the values by divisor, append the unit string. */
		public String toScaledString( int div, String unit ) {
			StringBuilder sb = new StringBuilder();
			sb.Append( "free=" ).Append( minFree / div );
			sb.Append( "/" ).Append( avgFree / div );
			sb.Append( "/" ).Append( maxFree / div ).Append( " " ).Append( unit );
			sb.Append( ", total=" ).Append( minTotal / div );
			sb.Append( "/" ).Append( avgTotal / div );
			sb.Append( "/" ).Append( maxTotal / div ).Append( " " ).Append( unit );
			return sb.ToString();
		}
	}
}
