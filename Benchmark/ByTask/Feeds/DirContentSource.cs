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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Lucene.Net.Benchmark.ByTask.Utils;

// TODO: Again, this is a class that I don't immediately need for Snapstream, so I am going
// to put off translating it this time.  It would be useful, however, and shouldn't be too
// hard to tanslate.

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * A {@link ContentSource} using the Dir collection for its input. Supports
	 * the following configuration parameters (on top of {@link ContentSource}):
	 * <ul>
	 * <li><b>work.dir</b> - specifies the working directory. Required if "docs.dir"
	 * denotes a relative path (<b>default=work</b>).
	 * <li><b>docs.dir</b> - specifies the directory the Dir collection. Can be set
	 * to a relative path if "work.dir" is also specified (<b>default=dir-out</b>).
	 * <li><b>html.parser</b> - specifies the {@link HTMLParser} class to use for
	 * parsing the TREC documents content (<b>default=DemoHTMLParser</b>).
	 * </ul>
	 */
	public class DirContentSource : ContentSource {

		  private class DateFormatInfo {
			public DateTimeFormatInfo df = new DateTimeFormatInfo();
		 }
			
		[ThreadStatic]
		private static DateFormatInfo dateFormat;
		private DirectoryInfo dataDir = null;
		private FileInfo[] files;
		private int iteration = 0;
		private IEnumerator inputFiles;

		// get/initiate a thread-local simple date format (must do so 
		// because SimpleDateFormat is not thread-safe).
		private DateFormatInfo getDateFormatInfo() {
			DateFormatInfo dfi = (DateFormatInfo) dateFormat;
			if (dfi == null) {
				dfi = new DateFormatInfo();
				// date format: 30-MAR-1987 14:22:36.87
				dfi.df.FullDateTimePattern ="dd-MMM-yyyy kk:mm:ss.SSS";
				dateFormat = dfi;
			}
			return dfi;
		}
  
		private DateTime parseDate(String dateStr) {
			DateFormatInfo dfi = getDateFormatInfo();
			return DateTime.Parse(dateStr);
		}

		public override void close() {
			inputFiles = null;
		}
  
		public override DocData getNextDocData(DocData docData) {
			FileInfo f = null;
			String name = null;
			lock (this) {
					
				if (!inputFiles.MoveNext()) { 
					// exhausted files, start a new round, unless forever set to false.
					if (!forever) {
						throw new NoMoreDataException();
					}
					inputFiles = files.GetEnumerator();
					iteration++;
				}
				f = (FileInfo) inputFiles.Current;
				// System.err.println(f);
				name = f.Name+"_"+iteration;
			}
    
			TextReader reader = f.OpenText();
			String line = null;
			//First line is the date, 3rd is the title, rest is body
			String dateStr = reader.ReadLine();
			reader.ReadLine();//skip an empty line
			String title = reader.ReadLine();
			reader.ReadLine();//skip an empty line
			StringBuilder bodyBuf = new StringBuilder(1024);
			while ((line = reader.ReadLine()) != null) {
				bodyBuf.Append(line).Append(' ');
			}
			reader.Close();
			addBytes(f.Length);
    
			DateTime date = parseDate(dateStr);
    
			docData.clear();
			docData.setName(name);
			docData.setBody(bodyBuf.ToString());
			docData.setTitle(title);
			docData.setDate(date);
			return docData;
		}
  
		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void resetInputs() {
			base.resetInputs();
			inputFiles = files.GetEnumerator();
		}

		public override void setConfig(Config config) {
			base.setConfig(config);
    
			DirectoryInfo workDir = new DirectoryInfo(config.get("work.dir", "work"));
			String d = config.get("docs.dir", "dir-out");
			dataDir = new DirectoryInfo(d);
			if (!dataDir.FullName.StartsWith("/")) {
				dataDir = new DirectoryInfo(Path.Combine(workDir.FullName, d));
			}
			// Rather than useing the special iterator we're just going to limit
			// the files that are returned by GetFiles
			files = getAllFiles( dataDir );
			// Rather than use the comparer routine we'll sort the files here
			Array.Sort( files, delegate( FileInfo f1, FileInfo f2 ) {
				return files.Length.CompareTo( f2.Length );
			} );
			inputFiles = files.GetEnumerator();
			if (dataDir.GetFiles().Length == 0) {
				throw new ApplicationException("No txt files in dataDir: " + dataDir.FullName);
			}
		}

		// This method simply recursively runs through a tree of directories
		// and gets a list of all the .TXT files in each of them.
		public FileInfo[] getAllFiles( DirectoryInfo soureDir ) {
			// Gather all the files first
			List<FileInfo> localFiles = soureDir.GetFiles( "*.txt" ).ToList<FileInfo>();
			// no go through all the subdirectories
			DirectoryInfo[] directories = soureDir.GetDirectories();
			foreach( DirectoryInfo dir in directories ) {
				localFiles.AddRange( getAllFiles( dir ).ToList<FileInfo>() );
			}
			return (FileInfo[])localFiles.ToArray();
		}
	}
}
