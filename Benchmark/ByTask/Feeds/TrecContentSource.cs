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
using System.Reflection;
using System.Text;
using System.Threading;
using Lucene.Net.Benchmark.ByTask.Utils;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;


// TODO: again, this isn't a file I need for work, so I'lm going to put off
// finishing translating this.
namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * Implements a {@link ContentSource} over the TREC collection.
	 * <p>
	 * Supports the following configuration parameters (on top of
	 * {@link ContentSource}):
	 * <ul>
	 * <li><b>work.dir</b> - specifies the working directory. Required if "docs.dir"
	 * denotes a relative path (<b>default=work</b>).
	 * <li><b>docs.dir</b> - specifies the directory where the TREC files reside.
	 * Can be set to a relative path if "work.dir" is also specified
	 * (<b>default=trec</b>).
	 * <li><b>html.parser</b> - specifies the {@link HTMLParser} class to use for
	 * parsing the TREC documents content (<b>default=DemoHTMLParser</b>).
	 * <li><b>content.source.encoding</b> - if not specified, ISO-8859-1 is used.
	 * </ul>
	 */
	public class TrecContentSource : ContentSource {

		System.IO.StreamWriter outWriter;

		public TrecContentSource() : base()
		{
			outWriter = new System.IO.StreamWriter(Console.OpenStandardOutput(), System.Console.Out.Encoding);
			outWriter.AutoFlush = true;
		}

		// TODO (3.0): change StringBuffer to StringBuilder

		 public class DateFormatInfo {
			public DateTimeFormatInfo dfs;
		  }

		  private const String DATE = "Date: ";
		  private const String DOCHDR = "<DOCHDR>";
		  private const String TERMINATING_DOCHDR = "</DOCHDR>";
		  private const String DOCNO = "<DOCNO>";
		  private const String TERMINATING_DOCNO = "</DOCNO>";
		  private const String DOC = "<DOC>";
		  private const String TERMINATING_DOC = "</DOC>";

		  private readonly static String NEW_LINE = Environment.NewLine;

		  private static String[] DATE_FORMATS = {
			   "EEE, dd MMM yyyy kk:mm:ss z",	  // Tue, 09 Dec 2003 22:39:08 GMT
			   "EEE MMM dd kk:mm:ss yyyy z",  	// Tue Dec 09 16:45:08 2003 EST
			   "EEE, dd-MMM-':'y kk:mm:ss z", 	// Tue, 09 Dec 2003 22:39:08 GMT
			   "EEE, dd-MMM-yyy kk:mm:ss z", 	  // Tue, 09 Dec 2003 22:39:08 GMT
			   "EEE MMM dd kk:mm:ss yyyy",  	  // Tue Dec 09 16:45:08 2003
		  };

		  private ThreadLocal<DateFormatInfo>[] dateFormats = new ThreadLocal<DateFormatInfo>[4];
		  private ThreadLocal<StringBuilder> trecDocReader = new ThreadLocal<StringBuilder>();
		  private ThreadLocal<StringBuilder> trecDocBuffer = new ThreadLocal<StringBuilder>();
		  private DirectoryInfo dataDir = null;
		  private ArrayList inputFiles = new ArrayList();
		  private int nextFile = 0;
		  private int rawDocSize;

		  // Use to synchronize threads on reading from the TREC documents.
		  private Object lockObject = new Object();

		  // Required for test
		  TextReader reader;
		  int iteration = 0;
		  HTMLParser htmlParser;
  
		  private ThreadLocal<DateFormatInfo>[] getDateFormatInfo() {
			if (dateFormats[0].Value.dfs.FullDateTimePattern == null) {
				dateFormats = new ThreadLocal<DateFormatInfo>[ DATE_FORMATS.Length ];
			  for (int i = 0; i < dateFormats.Length; i++) {
				dateFormats[i].Value.dfs.FullDateTimePattern = DATE_FORMATS[i];
			  }
			}
			return dateFormats;
		  }

		  private ThreadLocal<StringBuilder> getDocBuffer() {
			  ThreadLocal<StringBuilder> sb = (ThreadLocal<StringBuilder>)trecDocBuffer;
			if (sb == null) {
			  sb = new ThreadLocal<StringBuilder>();
			  trecDocBuffer = sb;
			}
			return sb;
		  }
  
		  private TextReader getTrecDocReader(ThreadLocal<StringBuilder> docBuffer) {
			  TextReader r = reader;
			//if (r == null) {
			//    r = new ;
			//  trecDocReader.set(r);
			//} else {
			//  r.set(docBuffer);
			//}
			return r;
		  }

		  // read until finding a line that starts with the specified prefix, or a terminating tag has been found.
		  public void read(ThreadLocal<StringBuilder> buf, String prefix, bool collectMatchLine,
							bool collectAll, String terminatingTag) {
			String sep = "";
			while (true) {
			  String line = reader.ReadLine();

			  if (line == null) {
				openNextFile();
				continue;
			  }

			  rawDocSize += line.Length;

			  if (line.StartsWith(prefix)) {
				if (collectMatchLine) {
				  buf.Value.Append(sep).Append(line);
				  sep = NEW_LINE;
				}
				break;
			  }

			  if (terminatingTag != null && line.StartsWith(terminatingTag)) {
				// didn't find the prefix that was asked, but the terminating
				// tag was found. set the length to 0 to signal no match was
				// found.
				buf.Value  = new StringBuilder();
				break;
			  }

			  if (collectAll) {
				buf.Value.Append(sep).Append(line);
				sep = NEW_LINE;
			  }
			}
		  }
  
		  void openNextFile() {
			close();
			int retries = 0;
			while (true) {
			  if (nextFile >= inputFiles.Count) { 
				// exhausted files, start a new round, unless forever set to false.
				if (!forever) {
				  throw new NoMoreDataException();
				}
				nextFile = 0;
				iteration++;
			  }
			  FileInfo f = (FileInfo) inputFiles[nextFile++];
			  if (verbose) {
				  Benchmark.LogSheet.AddRowsAndCellsSplit( "opening: " + f.Name + " length: " + f.Length );
			  	Console.WriteLine("opening: " + f.Name + " length: " + f.Length);
			  }
			  try {
				GZipInputStream zis = new GZipInputStream( f.Open(FileMode.Open) );
				  // I need to put the encoding back in here.  The problem is that
				  // it is begins stored in the properties as a string, and I need
				  // to find some way to translate that.
				reader = new StreamReader(zis);
				//	, encoding);
				return;
			  } catch (Exception e) {
				retries++;
				if (retries < 20 && verbose) {
					Benchmark.LogSheet.AddRowAndCell("Skipping 'bad' file " + f.FullName + "  #retries=" + retries);
					Console.WriteLine("Skipping 'bad' file " + f.FullName + "  #retries=" + retries);
				  continue;
				}
				throw new NoMoreDataException();
			  }
			}
		  }

		  DateTime parseDate(String dateStr) {
			dateStr = dateStr.Trim();
			ThreadLocal<DateFormatInfo>[] dfi = getDateFormatInfo();
			for (int i = 0; i < dfi.Length; i++) {
			  DateFormatInfo df = dfi[i].Value;
			  DateTime d = DateTime.Parse( dateStr );
			  if (d != null) {
				// Parse succeeded.
				return d;
			  }
			}
			// do not fail test just because a date could not be parsed
			if (verbose) {
				Benchmark.LogSheet.AddRowAndCell("failed to parse date (assigning 'now') for: " + dateStr);
				Console.WriteLine("failed to parse date (assigning 'now') for: " + dateStr);
			}
			   DateTime r = new DateTime(0);
			return r; 
		  }
  
		  public override void close() {
			if (reader == null) {
			  return;
			}

			try {
			  reader.Close();
			} catch (IOException e) {
			  if (verbose) {
			  	Console.WriteLine("failed to close reader !");
				  outWriter.WriteLine(e.StackTrace);
			  }
			}
			reader = null;
		  }

		  public override DocData getNextDocData(DocData docData) {
			String dateStr = null, name = null;
			TextReader r = null;
			// protect reading from the TREC files by multiple threads. The rest of the
			// method, i.e., parsing the content and returning the DocData can run
			// unprotected.
			lock (lockObject) {
			  if (reader == null) {
				openNextFile();
			  }
				ThreadLocal<StringBuilder> docBuf = getDocBuffer();
      
			  // 1. skip until doc start
			  docBuf = null;
			  read(docBuf, DOC, false, false, null);

			  // 2. name
			  docBuf.Value = new StringBuilder();
			  read(docBuf, DOCNO, true, false, null);
			  name = docBuf.ToString().Substring(DOCNO.Length, docBuf.ToString().IndexOf(TERMINATING_DOCNO,
				  DOCNO.Length));
			  name = name + "_" + iteration;

			  // 3. skip until doc header
			  docBuf.Value = new StringBuilder();
			  read(docBuf, DOCHDR, false, false, null);

			  bool findTerminatingDocHdr = false;

			  // 4. date - look for the date only until /DOCHDR
			  docBuf.Value = new StringBuilder();
			  read(docBuf, DATE, true, false, TERMINATING_DOCHDR);
			  if (docBuf.Value.Length != 0) {
				// Date found.
				dateStr = docBuf.ToString().Substring(DATE.Length);
				findTerminatingDocHdr = true;
			  }

			  // 5. skip until end of doc header
			  if (findTerminatingDocHdr) {
				docBuf.Value = new StringBuilder();
				read(docBuf, TERMINATING_DOCHDR, false, false, null);
			  }

			  // 6. collect until end of doc
			  docBuf.Value = new StringBuilder();
			  read(docBuf, TERMINATING_DOC, false, true, null);
      
			  // 7. Set up a Reader over the read content
			  r = getTrecDocReader(docBuf);
			  // Resetting the thread's reader means it will reuse the instance
			  // allocated as well as re-read from docBuf.
			  r.Dispose();
      
			  // count char length of parsed html text (larger than the plain doc body text).
			  addBytes(docBuf.Value.Length); 
			}

			// This code segment relies on HtmlParser being thread safe. When we get 
			// here, everything else is already private to that thread, so we're safe.
			DateTime date = dateStr != null ? parseDate(dateStr) : DateTime.Now;
			try {
			  docData = htmlParser.parse(docData, name, date, r, null);
			  addDoc();
			} catch (ApplicationException e) {
			  IOException ex = new IOException(e.Message);
			  throw ex;
			}

			return docData;
		  }

		  public override void resetInputs() {
			lock (lockObject) {
			  base.resetInputs();
			  close();
			  nextFile = 0;
			  iteration = 0;
			}
		  }

		  public override void setConfig(Config config) {
			base.setConfig(config);
			DirectoryInfo workDir = new DirectoryInfo(config.get("work.dir", "work"));
			String d = config.get("docs.dir", "trec");
			dataDir = new DirectoryInfo(d);
			collectFiles(dataDir, inputFiles);
			if (inputFiles.Count == 0) {
			  throw new ArgumentException("No files in dataDir: " + dataDir);
			}
			try {
			  String parserClassName = config.get("html.parser",
				  "Lucene.Net.Benchmark.ByTask.Feeds.DemoHTMLParser");
				// TODO: assembly stuff
			  Assembly assembly = Assembly.LoadFrom( "Benchmark.exe" );
			  htmlParser = (HTMLParser)assembly.CreateInstance( parserClassName );
			} catch (Exception e) {
			  // Should not get here. Throw runtime exception.
			  throw new ApplicationException("Application exception from TrecContentSource", e);
			}
			if (encoding == null) {
			  encoding = "ISO-8859-1";
			}
		  }
	}
}
