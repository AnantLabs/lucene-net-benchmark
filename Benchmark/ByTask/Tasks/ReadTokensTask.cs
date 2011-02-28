using System;
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
using System.Collections;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Documents;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	public class ReadTokensTask : PerfTask {

		public ReadTokensTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  private int totalTokenCount = 0;
  
		  // volatile data passed between setup(), doLogic(), tearDown().
		  private Document doc = null;
  
		  public override void setup() {
			base.setup();
			DocMaker docMaker = getRunData().getDocMaker();
			doc = docMaker.makeDocument();
		  }

		  protected override String getLogMessage(int recsCount) {
			return "read " + recsCount + " docs; " + totalTokenCount + " tokens";
		  }
  
		  public override void tearDown() {
			doc = null;
			base.tearDown();
		  }

		  Lucene.Net.Analysis.Token token = new Lucene.Net.Analysis.Token();

		  public override int doLogic() {
			ArrayList fields = (ArrayList)doc.GetFields();
			int numField = fields.Count;
			Analyzer analyzer = getRunData().getAnalyzer();
			int tokenCount = 0;
			for(int i=0;i<numField;i++) {
			  Field field = (Field) fields[i];
			  TokenStream stream;
			  TokenStream streamValue = field.TokenStreamValue();

			  if (streamValue != null) 
				stream = streamValue;
			  else {
				// the field does not have a TokenStream,
				// so we have to obtain one from the analyzer
				TextReader reader;			  // find or make Reader
				TextReader readerValue = field.ReaderValue();

				if (readerValue != null)
				  reader = readerValue;
				else {
				  String stringValue = field.StringValue();
				  if (stringValue == null)
					throw new ArgumentException("field must have either TokenStream, String or Reader value");
				  StringReader sr = new StringReader( stringValue );
				  reader = sr;
				}
        
				// Tokenize field
				stream = analyzer.ReusableTokenStream(field.Name(), reader);
			  }

			  // reset the TokenStream to the first token
			  stream.Reset();

			  while(stream.Next(token) != null)
				tokenCount++;
			}
			totalTokenCount += tokenCount;
			return tokenCount;
		  }

		  // This was deleted because it wasn't need in C#

		  ///* Simple StringReader that can be reset to a new string;
		  // * we use this when tokenizing the string value from a
		  // * Field. */
		  //ReusableStringReader stringReader = new ReusableStringReader();

		  //private static class ReusableStringReader : StringReader {
		  //  int upto;
		  //  int left;
		  //  String s;
		  //  void init(String s) {
		  //    this.s = s;
		  //    left = s.Length;
		  //    this.upto = 0;
		  //  }
		  //  public int read(char[] c) {
		  //    return read(c, 0, c.Length);
		  //  }
		  //  public int read(char[] c, int off, int len) {
		  //    if (left > len) {
		  //      s.Substring(upto, upto+len);
		  //      //s.Substring(upto, upto+len, c, off);
		  //      upto += len;
		  //      left -= len;
		  //      return len;
		  //    } else if (0 == left) {
		  //      return -1;
		  //    } else {
		  //      //s.getChars(upto, upto+left, c, off);
		  //      int r = left;
		  //      left = 0;
		  //      upto = s.Length;
		  //      return r;
		  //    }
		  //  }
		  //  public void close() {};
		  //}
	}
}
