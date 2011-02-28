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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ICSharpCode.SharpZipLib.BZip2;
using Lucene.Net.Benchmark.ByTask.Utils;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * Represents content from a specified source, such as TREC, Reuters etc. A
	 * {@link ContentSource} is responsible for creating {@link DocData} objects for
	 * its documents to be consumed by {@link DocMaker}. It also keeps track
	 * of various statistics, such as how many documents were generated, size in
	 * bytes etc.
	 * <p>
	 * Supports the following configuration parameters:
	 * <ul>
	 * <li><b>content.source.forever</b> - specifies whether to generate documents
	 * forever (<b>default=true</b>).
	 * <li><b>content.source.verbose</b> - specifies whether messages should be
	 * output by the content source (<b>default=false</b>).
	 * <li><b>content.source.encoding</b> - specifies which encoding to use when
	 * reading the files of that content source. Certain implementations may define
	 * a default value if this parameter is not specified. (<b>default=null</b>).
	 * <li><b>content.source.log.step</b> - specifies for how many documents a
	 * message should be logged. If set to 0 it means no logging should occur.
	 * <b>NOTE:</b> if verbose is set to false, logging should not occur even if
	 * logStep is not 0 (<b>default=0</b>).
	 * </ul>
	 */
	public abstract class ContentSource {
		private const int BZIP = 0;
		private const int OTHER = 1;
		private readonly Dictionary<string, int> extensionToType = new Dictionary<string, int>() { {".bz2", BZIP}, { ".bzip", BZIP } };
  
		protected const int BUFFER_SIZE = 1 << 16; // 64K

		private long bytesCount;
		private long totalBytesCount;
		private int docsCount;
		private int totalDocsCount;
		private Config config;

		protected static bool forever;
		protected int logStep;
		protected bool verbose;
		protected String encoding;
		// Don't need this for C#
		//private CompressorStreamFactory csFactory = new CompressorStreamFactory();

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected void addBytes(long numBytes) {
			bytesCount += numBytes;
			totalBytesCount += numBytes;
		}
  
		[MethodImpl(MethodImplOptions.Synchronized)]
		protected void addDoc() {
			++docsCount;
			++totalDocsCount;
		}

		/**
		* A convenience method for collecting all the files of a content source from
		* a given directory. The collected {@link File} instances are stored in the
		* given <code>files</code>.
		*/
		protected virtual void collectFiles(DirectoryInfo dir, ArrayList files) {
			if (!dir.Exists) {
				return;
			}
    
			FileInfo[] dirFiles = dir.GetFiles();
			
			//Array.Sort(dirFiles);
			for (int i = 0; i < dirFiles.Length; i++) {
				FileInfo file = dirFiles[i];
				files.Add(file);
			}
			DirectoryInfo[] dirDirs = dir.GetDirectories();
			//Array.Sort(dirDirs);
			foreach (DirectoryInfo DirectoryInfo in dirDirs)
			{
				collectFiles(DirectoryInfo, files);
			}
			
		}

		/**
		* Returns an {@link InputStream} over the requested file. This method
		* attempts to identify the appropriate {@link InputStream} instance to return
		* based on the file name (e.g., if it ends with .bz2 or .bzip, return a
		* 'bzip' {@link InputStream}).
		*/
		protected Stream getInputStream(FileInfo file) {
			// First, create a FileInputStream, as this will be required by all types.
			// Wrap with BufferedInputStream for better performance

			Stream str = null;

			FileStream fs = File.Open(file.FullName, FileMode.Open);
    
			String fileName = file.Name;
			int idx = fileName.LastIndexOf('.');
			int type = OTHER;
			if (idx != -1) {
				int typeInt = (int) extensionToType[fileName.Substring(idx)];
				if (typeInt != null) {
					type = typeInt;
				}
			}
			switch (type) {
				case BZIP:
				try {
					// According to BZip2CompressorInputStream's code, it reads the first 
					// two file header chars ('B' and 'Z'). It is important to wrap the
					// underlying input stream with a buffered one since
					// Bzip2CompressorInputStream uses the read() method exclusively.
						str = new BZip2InputStream( fs );
					
				} catch (Exception e) {
					throw new Exception( "SharpZipLib threw and execption", e );
				}
				break;
				default: // Do nothing, stay with FileInputStream
				return str;
			}
    
			return str;

				// throws IOException
		}
  
		/**
		* Returns true whether it's time to log a message (depending on verbose and
		* the number of documents generated).
		*/
		protected bool shouldLog() {
			return verbose && logStep > 0 && docsCount % logStep == 0;
		}

		/** Called when reading from this content source is no longer required. */
		public abstract void close();
		// throws IOException
  
		/** Returns the number of bytes generated since last reset. */
		public long getBytesCount() { return bytesCount; }

		/** Returns the number of generated documents since last reset. */
		public int getDocsCount() { return docsCount; }
  
		public Config getConfig() { return config; }

		/** Returns the next {@link DocData} from the content source. */
		public abstract DocData getNextDocData(DocData docData);

		/** Returns the total number of bytes that were generated by this source. */ 
		public long getTotalBytesCount() { return totalBytesCount; }

		/** Returns the total number of generated documents. */
		public int getTotalDocsCount() { return totalDocsCount; }

		/**
		* Resets the input for this content source, so that the test would behave as
		* if it was just started, input-wise.
		* <p>
		* <b>NOTE:</b> the default implementation resets the number of bytes and
		* documents generated since the last reset, so it's important to call
		* super.resetInputs in case you override this method.
		*/
		public virtual void resetInputs() {
			bytesCount = 0;
			docsCount = 0;
		}

		/**
		* Sets the {@link Config} for this content source. If you override this
		* method, you must call super.setConfig.
		*/
		public virtual void setConfig(Config config) {
			this.config = config;
			forever = config.get("content.source.forever", true);
			logStep = config.get("content.source.log.step", 0);
			verbose = config.get("content.source.verbose", false);
			encoding = config.get("content.source.encoding", null);
		}
	}
}
