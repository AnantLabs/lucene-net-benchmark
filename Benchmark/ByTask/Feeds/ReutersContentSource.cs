/**
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Lucene.Net.Benchmark.ByTask.Utils;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * A {@link ContentSource} reading from the Reuters collection.
	 * <p>
	 * Config properties:
	 * <ul>
	 * <li><b>work.dir</b> - path to the root of docs and indexes dirs (default
	 * <b>work</b>).
	 * <li><b>docs.dir</b> - path to the docs dir (default <b>reuters-out</b>).
	 * </ul>
	 */
	public class ReutersContentSource : ContentSource {

		  public class DateFormatInfo {
			public String df;
		  }

		  private ThreadLocal<DateFormatInfo> dateFormat = new ThreadLocal<DateFormatInfo>();
		  private DirectoryInfo dataDir = null;
		  private ArrayList inputFiles = new ArrayList();
		  private int nextFile = 0;
		  private int iteration = 0;
  
		  public override void setConfig(Config config) {
			base.setConfig(config);
			DirectoryInfo workDir = new DirectoryInfo(config.get("work.dir", "work"));
			String d = config.get("docs.dir", "reuters-out");
			dataDir = new DirectoryInfo(d);
			if (!dataDir.Name.StartsWith("/")) {
			  dataDir = new DirectoryInfo(Path.Combine(workDir.Name, d));
			}
			inputFiles.Clear();
			collectFiles(dataDir, inputFiles);
			if (inputFiles.Count == 0) {
			  throw new ApplicationException("No txt files in dataDir: "+dataDir.FullName);
			}
		  }

			[MethodImpl(MethodImplOptions.Synchronized)]	
		  private DateFormatInfo getDateFormatInfo() {
			DateFormatInfo dfi = (DateFormatInfo) dateFormat.Value;
			if (dfi == null) {
			  dfi = new DateFormatInfo();
			  // date format: 30-MAR-1987 14:22:36.87
			  dfi.df = "dd-MMM-yyyy hh:mm:ss.fff";
			  //dfi.df.setLenient(true);
			  //dfi.pos = new ParsePosition(0);
			  dateFormat.Value = dfi;
			}
			return dfi;
		  }

		  private String parseDate(String dateStr) {
			DateFormatInfo dfi = getDateFormatInfo();
			DateTime dt;
			try {
				dt = DateTime.Parse(dateStr.Trim());
			}
			catch( Exception ) {
				// again, we can't read it, so we're going to 
				// provide a default
				dt = DateTime.Now;
				//Console.WriteLine( "In the second datetime try" );
			}
			return  String.Format("{0:" + dfi.df + "}", dt);
		  }


		  public override void close() {
			// TODO implement?
			  //  throws IOException
			  //throw new NotImplementedException();
		  }
  
		  public override DocData getNextDocData(DocData docData) {
			FileInfo f = null;
			String name = null;
			lock (this) {
			  if (nextFile >= inputFiles.Count) {
				// exhausted files, start a new round, unless forever set to false.
				if (!forever) {
				  throw new NoMoreDataException();
				}
				nextFile = 0;
				iteration++;
			  }
			  f = (FileInfo) inputFiles[nextFile++];
			  name = f.FullName + "_" + iteration;
			}

			TextReader reader = File.OpenText(f.FullName);
			try {
			  // First line is the date, 3rd is the title, rest is body
			  String dateStr = reader.ReadLine();
			  reader.ReadLine();// skip an empty line
			  String title = reader.ReadLine();
			  reader.ReadLine();// skip an empty line
			  StringBuilder bodyBuf = new StringBuilder(1024);
			  String line = null;
			  while ((line = reader.ReadLine()) != null) {
				bodyBuf.Append(line).Append(' ');
			  }
			  reader.Close();
      
			  addBytes(f.Length);
			  DateTime dt;
			  try {
				  dt = DateTime.Parse( dateStr );
			  }
			  catch( Exception ) {
				  // OK, so this means we coudn't parse the date, so we'll
				  // fall back on a default
				  dt = DateTime.Now;
				  //Console.WriteLine( "First Datetime try" );
			  }
					
			  String temp = String.Format( "{0:dd-MMM-yyyy hh:mm:ss.fff}", dt );
			  String date = parseDate(dateStr.Trim());
      
			  docData.clear();
			  docData.setName(name);
			  docData.setBody(bodyBuf.ToString());
			  docData.setTitle(title);
			  docData.setDate(date);
			  return docData;
			} finally {
			  reader.Close();
			}
		  }

			[MethodImpl(MethodImplOptions.Synchronized)]
		  public override void resetInputs(){
			base.resetInputs();
			nextFile = 0;
			iteration = 0;
		  }

	}
}
