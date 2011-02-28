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
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.BZip2;
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Benchmark.ByTask.Utils;
using Lucene.Net.Documents;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * A task which writes documents, one line per document. Each line is in the
	 * following format: title &lt;TAB&gt; date &lt;TAB&gt; body. The output of this
	 * task can be consumed by
	 * {@link org.apache.lucene.benchmark.byTask.feeds.LineDocMaker} and is intended
	 * to save the IO overhead of opening a file per document to be indexed.<br>
	 * Supports the following parameters:
	 * <ul>
	 * <li>line.file.out - the name of the file to write the output to. That
	 * parameter is mandatory. <b>NOTE:</b> the file is re-created.
	 * <li>bzip.compression - whether the output should be bzip-compressed. This is
	 * recommended when the output file is expected to be large. (optional, default:
	 * false).
	 * </ul>
	 * <b>NOTE:</b> this class is not thread-safe and if used by multiple threads the
	 * output is unspecified (as all will write to the same output file in a
	 * non-synchronized way).
	 */
	public class WriteLineDocTask : PerfTask {

		System.IO.StreamWriter outWriter;
		bool doBzipCompression = false;
		BZip2OutputStream bos = null;

		  public readonly static string SEP = "\t";
		  private readonly Regex NORMALIZER = new Regex(@"[\t\r\n\}\{]+");

		  private int docSize = 0;
		  private StreamWriter lineFileOut = null;
		  private DocMaker docMaker;
  
		  public WriteLineDocTask(PerfRunData runData) : base(runData) {
			outWriter = new System.IO.StreamWriter( Console.OpenStandardOutput(), System.Console.Out.Encoding );
			outWriter.AutoFlush = true;
			Config config = runData.getConfig();
			String fileName = config.get("line.file.out", null);
			if (fileName == null) {
			  throw new ArgumentException("line.file.out must be set");
			}

			lineFileOut = new StreamWriter( fileName );
			
			String doBZCompress = config.get("bzip.compression", null);
			if (doBZCompress != null) {
			  // Property was set, use the value.
			  doBzipCompression = Boolean.Parse(doBZCompress);
			} else {
			  // Property was not set, attempt to detect based on file's extension
			  doBzipCompression = fileName.EndsWith("bz2");
			}

			if (doBzipCompression) {
			  // Wrap with BOS since BZip2CompressorOutputStream calls out.write(int) 
			  // and does not use the write(byte[]) version. This proved to speed the 
			  // compression process by 70% !
				FileStream fs = File.OpenWrite(fileName);
				bos = new BZip2OutputStream(fs);
			}

			docMaker = runData.getDocMaker();
		  }

		  protected override String getLogMessage(int recsCount) {
			return "Wrote " + recsCount + " line docs";
		  }
  
		  public override int doLogic() {

			  Document doc;
			doc = docSize > 0 ? docMaker.makeDocument( docSize ) : docMaker.makeDocument();

			Field f = doc.GetField(DocMaker.BODY_FIELD);
			String body = f != null ? NORMALIZER.Replace( f.StringValue(), " " ) : "";
    
			f = doc.GetField(DocMaker.TITLE_FIELD);
			String title = f != null ? NORMALIZER.Replace( f.StringValue(), " " ) : "";
    
			if (body.Length > 0 || title.Length > 0) {
      
			  f = doc.GetField(DocMaker.DATE_FIELD);
				 String date = f != null ? NORMALIZER.Replace(f.StringValue(), " ") : "";
				if( doBzipCompression ) {
					bos.Write(System.Text.Encoding.UTF8.GetBytes(title), 0, title.Length );
					bos.Write( System.Text.Encoding.UTF8.GetBytes(SEP), 0, 1 );
					bos.Write( System.Text.Encoding.UTF8.GetBytes(date), 0, date.Length );
					bos.Write( System.Text.Encoding.UTF8.GetBytes(SEP), 0, 1 );
					bos.Write( System.Text.Encoding.UTF8.GetBytes(body), 0, body.Length );
					bos.Write( System.Text.Encoding.UTF8.GetBytes("\n"), 0 , 1 );
				}
				else
				{
					try {
						lineFileOut.Write( title, 0, title.Length );
					}
					catch( Exception e ) {
						Console.WriteLine( e.Message );
					}
					lineFileOut.Write(SEP);
					lineFileOut.Write(date, 0, date.Length);
					lineFileOut.Write(SEP);
					try {
						lineFileOut.Write( body, 0, body.Length );
					}
					catch( Exception e ) {
						Console.WriteLine( e.Message );
					}
					lineFileOut.Write(lineFileOut.NewLine);
				}
			}
			return 1;
		  }

		  public override void close() {
			lineFileOut.Close();
			base.close();
			  //  throws Exception
		  }
  
		  /**
		   * Set the params (docSize only)
		   * @param params docSize, or 0 for no limit.
		   */
		  public override void setParams(String args) {
			if (base.supportsParams()) {
			  base.setParams(args);
			}
			docSize = (int) float.Parse(args); 
		  }

		  public override bool supportsParams() {
			return true;
		  }
	}
}
