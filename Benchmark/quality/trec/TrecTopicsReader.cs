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
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Lucene.Net.Benchmark.quality.trec {

	/**
	 * Read TREC topics.
	 * <p>
	 * Expects this topic format -
	 * <pre>
	 *   &lt;top&gt;
	 *   &lt;num&gt; Number: nnn
	 *     
	 *   &lt;title&gt; title of the topic
	 *     
	 *   &lt;desc&gt; Description:
	 *   description of the topic
	 *     
	 *   &lt;narr&gt; Narrative:
	 *   "story" composed by assessors.
	 *    
	 *   &lt;/top&gt;
	 * </pre>
	 * Comment lines starting with '#' are ignored.
	 */
	public class TrecTopicsReader {

		  private readonly String newline = Environment.NewLine;
  
		  /**
		   *  Constructor for Trec's TopicsReader
		   */
		  public TrecTopicsReader() : base() {
			// do nothing
		  }

		  /**
		   * Read quality queries from trec format topics file.
		   * @param reader where queries are read from.
		   * @return the result quality queries.
		   * @throws IOException if cannot read the queries.
		   */
		  public QualityQuery[] readQueries(TextReader reader) {
			ArrayList res = new ArrayList();
			String sb;
			try {
			  while (null!=(sb=read(reader,"<top>",null,false,false))) {
				StringDictionary fields = new StringDictionary();
				// id
				sb = read(reader,"<num>",null,true,false);
				int k = sb.IndexOf(":");
				String id = sb.Substring(k+1).Trim();
				// title
				sb = read(reader,"<title>",null,true,false);
				k = sb.IndexOf(">");
				String title = sb.Substring(k+1).Trim();
				// description
				sb = read(reader,"<desc>",null,false,false);
				sb = read(reader,"<narr>",null,false,true);
				String descripion = sb.ToString().Trim();
				// we got a topic!
				fields.Add("title",title);
				fields.Add("description",descripion);
				QualityQuery topic = new QualityQuery(id,fields);
				res.Add(topic);
				// skip narrative, get to end of doc
				read(reader,"</top>",null,false,false);
			  }
			} finally {
			  reader.Close();
			}
			// sort result array (by ID) 
			QualityQuery[] qq = (QualityQuery[]) res.ToArray();
			Array.Sort(qq);
			return qq;
		  }

		  // read until finding a line that starts with the specified prefix
		  private String read (TextReader reader, String prefix, StringBuilder sb, bool collectMatchLine, bool collectAll) {
			sb = (sb==null ? new StringBuilder() : sb);
			String sep = "";
			while (true) {
			  String line = reader.ReadLine();
			  if (line==null) {
				return null;
			  }
			  if (line.StartsWith(prefix)) {
				if (collectMatchLine) {
				  sb.Append(sep+line);
				  sep = newline;
				}
				break;
			  }
			  if (collectAll) {
				sb.Append(sep+line);
				sep = newline;
			  }
			}
			//System.out.println("read: "+sb);
			return sb.ToString();
		  }
	}
}
