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
using System.Globalization;
using System.IO;
using System.Text;
using Kajabity.Tools.Java;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * HTML Parser that is based on Lucene's demo HTML parser.
	 */
	public class DemoHTMLParser {

		public DocData parse(DocData docData, String name, DateTime date, Stream reader, DateTimeFormatInfo dateFormat) {
			Lucene.Net.Demo.Html.HTMLParser p = new Lucene.Net.Demo.Html.HTMLParser(reader);
    
			// title
			String title = p.GetTitle();
			// properties 
			JavaProperties props = new JavaProperties();
			foreach (DictionaryEntry metaTag in p.GetMetaTags())
			{
				props.Add( metaTag.Key, metaTag.Value );
			}
			// body
			StreamReader r = p.GetReader();
			char[] c = new char[1024];
			StringBuilder bodyBuf = new StringBuilder();
			int n;
			while ((n = r.Read()) >= 0) {
				if (n>0) {
					bodyBuf.Append(c,0,n);
				}
			}
			r.Close();
			if (date == null && props.GetProperty("date")!=null) {
				try {
					date = DateTime.Parse(props.GetProperty("date").Trim());
				} catch (Lucene.Net.Demo.Html.ParseException e) {
					// do not fail test just because a date could not be parsed
					Console.WriteLine("ignoring date parse exception (assigning 'now') for: "+props.GetProperty("date"));
					date = DateTime.Now; // now 
				}
			}
    
			docData.clear();
			docData.setName(name);
			docData.setBody(bodyBuf.ToString());
			docData.setTitle(title);
			docData.setProps(props);
			docData.setDate(date);
			return docData;
		}

		/*
		*  (non-Javadoc)
		* @see Lucene.Benchmark.ByTask.Feeds.HTMLParser#parse(java.lang.String, java.util.Date, java.lang.StringBuffer, java.text.DateFormat)
		*/
		public DocData parse(DocData docData, String name, DateTime date, String inputText, DateTimeFormatInfo dateFormat) {
			byte[] input = System.Text.Encoding.ASCII.GetBytes(inputText);
			MemoryStream ms = new MemoryStream( input);
			return parse(docData, name, date, ms, dateFormat);
		}
	}
}
