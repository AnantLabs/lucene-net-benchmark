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

using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.quality {


	/**
	 * Parse a QualityQuery into a Lucene query.
	 */
	public abstract class QualityQueryParser {

		/**
	   * Parse a given QualityQuery into a Lucene query.
	   * @param qq the quality query to be parsed.
	   * @throws ParseException if parsing failed.
	   */
	  public abstract Query parse(QualityQuery qq);
	}
}
