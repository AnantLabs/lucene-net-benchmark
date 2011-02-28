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
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Lucene.Net.Benchmark.ByTask.Tasks;
using Lucene.Net.Benchmark.ByTask.Utils;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * A {@link ContentSource} reading one line at a time as a
	 * {@link org.apache.lucene.document.Document} from a single file. This saves IO
	 * cost (over DirContentSource) of recursing through a directory and opening a
	 * new file for every document.<br>
	 * The expected format of each line is (arguments are separated by &lt;TAB&gt;):
	 * <i>title, date, body</i>. If a line is read in a different format, a
	 * {@link RuntimeException} will be thrown. In general, you should use this
	 * content source for files that were created with {@link WriteLineDocTask}.<br>
	 * <br>
	 * Config properties:
	 * <ul>
	 * <li>docs.file=&lt;path to the file&gt;
	 * <li>content.source.encoding - default to UTF-8.
	 * </ul>
	 */
	public class LineDocSource : ContentSource {

		  private static readonly string SEP = WriteLineDocTask.SEP;
		  private FileInfo file;
		  private StreamReader reader;

		  [MethodImpl(MethodImplOptions.Synchronized)]
		  public void openFile() {
			try {
			  if (reader != null) {
				reader.Close();
			  }
				reader = new StreamReader(file.FullName, Encoding.UTF8);
			} catch (IOException e) {
			  throw new ApplicationException("IO exception thrown in LineDocSource", e);
			}
		  }

		  public override void close() {
			if (reader != null) {
			  reader.Close();
			  reader = null;
			}
		  }
  
		  public override DocData getNextDocData(DocData docData) {
			String line;
			lock(this) {
			  line = reader.ReadLine();
			  if (line == null) {
				if (!forever) {
				  throw new NoMoreDataException();
				}
				// Reset the file
				openFile();
				return getNextDocData(docData);
			  }
			}
    
			// A line must be in the following format. If it's not, fail !
			// title <TAB> date <TAB> body <NEWLINE>
			int spot = line.IndexOf(SEP);
			if (spot == -1) {
			  throw new ApplicationException("line: [" + line + "] is in an invalid format !");
			}
			int spot2 = line.IndexOf(SEP, 1 + spot);
			if (spot2 == -1) {
			  throw new ApplicationException("line: [" + line + "] is in an invalid format !");
			}
			// The date String was written in the format of DateTools.dateToString.
			docData.clear();
			docData.setBody(line.Substring(1 + spot2, line.Length));
			docData.setTitle(line.Substring(0, spot));
			docData.setDate(line.Substring(1 + spot, spot2));
			return docData;
		  }

		  public override void resetInputs() {
			base.resetInputs();
			openFile();
		  }
  
		  public override void setConfig(Config config) {
			base.setConfig(config);
			String fileName = config.get("docs.file", null);
			if (fileName == null) {
			  throw new ArgumentOutOfRangeException("docs.file must be set");
			}
			FileInfo fi = new FileInfo( fileName );
			String file = fi.FullName;
			if (encoding == null) {
			  encoding = "UTF-8";
			}
		  }
	}
}
