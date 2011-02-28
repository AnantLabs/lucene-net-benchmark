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
using System.Globalization;
using System.IO;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * HTML Parsing Interface for test purposes
	 */
	public interface HTMLParser {

		  /**
		   * Parse the input Reader and return DocData. 
		   * A provided name or date is used for the result, otherwise an attempt is 
		   * made to set them from the parsed data.
		   * @param dateFormat date formatter to use for extracting the date.   
		   * @param name name of the result doc data. If null, attempt to set by parsed data.
		   * @param date date of the result doc data. If null, attempt to set by parsed data.
		   * @param reader of html text to parse.
		   * @return Parsed doc data.
		   * @throws IOException
		   * @throws InterruptedException
		   */
			DocData parse(DocData docData, String name, DateTime date, TextReader reader, DateTimeFormatInfo dateFormat);
  
		  /**
		   * Parse the inputText and return DocData. 
		   * @param inputText the html text to parse.
		   * @see #parse(DocData, String, Date, Reader, DateFormat)
		   */
		  DocData parse(DocData docData, String name, DateTime date, String inputText, DateTimeFormatInfo dateFormat);
	}
}
