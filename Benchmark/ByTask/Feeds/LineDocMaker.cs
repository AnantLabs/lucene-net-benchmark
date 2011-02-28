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
using Lucene.Net.Benchmark.ByTask.Utils;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * A DocMaker reading one line at a time as a Document from a single file. This
	 * saves IO cost (over DirContentSource) of recursing through a directory and
	 * opening a new file for every document. It also re-uses its Document and Field
	 * instance to improve indexing speed.<br>
	 * The expected format of each line is (arguments are separated by &lt;TAB&gt;):
	 * <i>title, date, body</i>. If a line is read in a different format, a
	 * {@link RuntimeException} will be thrown. In general, you should use this doc
	 * maker with files that were created with 
	 * {@link org.apache.lucene.benchmark.byTask.tasks.WriteLineDocTask}.<br>
	 * <br>
	 * Config properties:
	 * <ul>
	 * <li>doc.random.id.limit=N (default -1) -- create random docid in the range
	 * 0..N; this is useful with UpdateDoc to test updating random documents; if
	 * this is unspecified or -1, then docid is sequentially assigned
	 * </ul>
	 * @deprecated Please use {@link DocMaker} instead, with content.source=LineDocSource
	 */
	public class LineDocMaker : DocMaker {

		System.IO.StreamWriter outWriter;

		public LineDocMaker() : base()
		{
			outWriter = new System.IO.StreamWriter(Console.OpenStandardOutput(), System.Console.Out.Encoding);
			outWriter.AutoFlush = true;
		}


		public void setConfig(Config config) {
			base.setConfig(config);
			source = new LineDocSource();
			source.setConfig(config);
			outWriter.WriteLine("NOTE: LineDocMaker is deprecated; please use DocMaker instead (which is the default if you don't specify doc.maker) with content.source=LineDocSource");
		  }
	}
}
