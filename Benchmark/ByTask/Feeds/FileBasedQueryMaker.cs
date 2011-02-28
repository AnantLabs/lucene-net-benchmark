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
using System.IO;
using System.Reflection;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * Create queries from a FileReader.  One per line, pass them through the
	 * QueryParser.  Lines beginning with # are treated as comments
	 *
	 * File can be specified as a absolute, relative or resource.
	 * Two properties can be set:
	 * file.query.maker.file=&lt;Full path to file containing queries&gt;
	 * <br/>
	 * file.query.maker.default.field=&lt;Name of default field - Default value is "body"&gt;
	 *
	 * Example:
	 * file.query.maker.file=c:/myqueries.txt
	 * file.query.maker.default.field=body
	 */
	public class FileBasedQueryMaker : AbstractQueryMaker, QueryMaker {

		System.IO.StreamWriter errWriter;

		FileBasedQueryMaker () : base()
		{
			errWriter = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
			errWriter.AutoFlush = true;
		}

		  protected override ArrayList prepareQueries() {

				// TODO: assembly stuff
				Assembly assembly = Assembly.LoadFrom("Lucene.Net.dll");
				Analyzer anlzr = (Analyzer) assembly.CreateInstance(config.get("analyzer",
						"Lucene.Net.Analysis.Standard.StandardAnalyzer"));
				String defaultField = config.get("file.query.maker.default.field", DocMaker.BODY_FIELD);
				QueryParser qp = new QueryParser(defaultField, anlzr);

				ArrayList qq = new ArrayList();
				String fileName = config.get("file.query.maker.file", null);
				if( fileName != null )
				{
					FileInfo file = new FileInfo(fileName);
					TextReader reader = null;
					if (file.Exists)
					{
						reader = File.OpenText(fileName);
					}
					else
					{
						//see if we can find it as a resource
						// not sure what the c# alternative of this is
						//InputStream asStream = FileBasedQueryMaker.class.getClassLoader().getResourceAsStream(fileName);
						//if (asStream != null) {
						//  reader = new InputStreamReader(asStream);
						//}
					}
					if (reader != null)
					{
						try
						{
							String line = null;
							int lineNum = 0;
							while ((line = reader.ReadLine()) != null)
							{
								line = line.Trim();
								if (!line.Equals("") && !line.StartsWith("#"))
								{
									Query query = null;
									try
									{
										query = qp.Parse(line);
									}
									catch (Lucene.Net.QueryParsers.ParseException e)
									{

										errWriter.WriteLine("Exception: " + e.Message + " occurred while parsing line: "
										                   + lineNum + " Text: " + line);
									}
									qq.Add(query);
								}
								lineNum++;
							}
						}
						finally
						{
							reader.Close();
						}
					}
					else
					{
						errWriter.WriteLine("No Reader available for: " + fileName);
					}

				}
		  	return qq;
			  }
	}
}
