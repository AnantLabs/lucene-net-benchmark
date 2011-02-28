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

namespace Lucene.Net.Benchmark.quality.trec {


	/**
	 * Read topics of TREC 1MQ track.
	 * <p>
	 * Expects this topic format -
	 * <pre>
	 *   qnum:qtext
	 * </pre>
	 * Comment lines starting with '#' are ignored.
	 * <p>
	 * All topics will have a single name value pair.
	 */
	public class Trec1MQReader {

		  private String name;
  
		  /**
		   *  Constructor for Trec's 1MQ TopicsReader
		   *  @param name name of name-value pair to set for all queries.
		   */
		  public Trec1MQReader(String name) : base()  {
			this.name = name;
		  }

		  /**
		   * Read quality queries from trec 1MQ format topics file.
		   * @param reader where queries are read from.
		   * @return the result quality queries.
		   * @throws IOException if cannot read the queries.
		   */
		  public QualityQuery[] readQueries(TextReader reader) {
			ArrayList res = new ArrayList();
			String line;
			try {
			  while (null!=(line=reader.ReadLine())) {
				line = line.Trim();
				if (line.StartsWith("#")) {
				  continue;
				}
				// id
				int k = line.IndexOf(":");
				String id = line.Substring(0,k).Trim();
				// qtext
				String qtext = line.Substring(k+1).Trim();
				// we got a topic!
				StringDictionary fields = new StringDictionary();
				fields.Add(name,qtext);
				//System.out.println("id: "+id+" qtext: "+qtext+"  line: "+line);
				QualityQuery topic = new QualityQuery(id,fields);
				res.Add(topic);
			  }
			} finally {
			  reader.Close();
			}
			// sort result array (by ID) 
			QualityQuery[] qq = (QualityQuery[]) res.ToArray();
			Array.Sort(qq);
			return qq;
		  }
	}
}
