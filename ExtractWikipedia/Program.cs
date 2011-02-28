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
using System.Reflection;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

using Lucene.Net.Benchmark.ByTask;
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Benchmark.ByTask.Stats;
using Lucene.Net.Benchmark.ByTask.Tasks;
using Lucene.Net.Benchmark.ByTask.Utils;

using RTools.Util;
using Kajabity.Tools.Java;
using Kajabity.Tools;
using ICSharpCode.SharpZipLib.BZip2;



namespace ExtractWikipedia {

	/**
	 * Extract the downloaded Wikipedia dump into separate files for indexing.
	 */
	class ExtractWikipedia {

			 private DirectoryInfo outputDir;

			static public int count = 0;

			const int BASE = 10;
			protected DocMaker docMaker;

			public ExtractWikipedia(DocMaker docMaker, DirectoryInfo outputDir) {
				this.outputDir = outputDir;
				this.docMaker = docMaker;
				Console.WriteLine("Deleting all files in " + outputDir);
				DeleteAll( outputDir );
						
			}

			/// <summary>
			/// This is just a simple routine that recoursively deletes everything in a folder.
			/// Will delete the folder itself if second argument is set to "true"
			/// </summary>
			/// <param name="Dir">Directory to delete</param>
			/// <param name="DeleteParent">Delete parent directory as well</param>

			public static void DeleteAll( DirectoryInfo Dir, bool DeleteParent = false ) {
				// Check if the target directory exists, if not, just return.
				if( System.IO.Directory.Exists( Dir.FullName ) == false ) {
					return;
				}

				// Delete all files in a directory
				foreach( FileInfo fi in Dir.GetFiles() ) {
					fi.Delete();
				}

				// Delete contents of each sub directory recursively
				foreach( DirectoryInfo SubDir in Dir.GetDirectories() ) {
					SubDir.Delete( true );
				}

				if( DeleteParent ) {
					Dir.Delete( true );
				}

			} // end of method

			public DirectoryInfo directory(int count, DirectoryInfo _directory) {
				if (_directory == null) {
					_directory = outputDir;
				}
				int baseInt = BASE;
				while (baseInt <= count) {
					baseInt *= BASE;
				}
				if (count < BASE) {
					return _directory;
				}
				//TODO: need to figure out why this is set tiwice in java,
				// suspect because it is creating two directories
				_directory = new DirectoryInfo(Path.Combine(_directory.FullName, (baseInt / BASE).ToString()));
				_directory = new DirectoryInfo(Path.Combine(_directory.FullName, (count / (baseInt / BASE)).ToString()));
				return directory(count % (baseInt / BASE), _directory);
			}

			public void create(String id, String title, String time, String body) {

				DirectoryInfo d = directory(count++, null);
				d.Create();
				FileInfo f = new FileInfo(Path.Combine(d.FullName, id + ".txt"));

				StringBuilder contents = new StringBuilder();

				contents.Append(time);
				contents.Append("\n\n");
				contents.Append(title);
				contents.Append("\n\n");
				contents.Append(body);
				contents.Append("\n");

				try {
					TextWriter writer = f.CreateText();
					writer.Write(contents.ToString());
					writer.Close();
				} catch (IOException ioe) {
					throw new ApplicationException("IO exception in ExtractWikipedia", ioe);
				}

			}

			public void extract() {
				Document doc = null;
				Console.WriteLine("Starting Extraction");
				DateTime start = DateTime.Now;
				try {
					while ((doc = docMaker.makeDocument()) != null) {
					create(doc.Get(DocMaker.ID_FIELD), doc.Get(DocMaker.TITLE_FIELD), doc
						.Get(DocMaker.DATE_FIELD), doc.Get(DocMaker.BODY_FIELD));
					}
				} catch (NoMoreDataException e) {
					//continue
				}
				DateTime finish = DateTime.Now;
				Console.WriteLine("Extraction took " + (finish - start) + " ms");
			}

			public static void Main(String[] args) {

				FileInfo wikipedia = null;
				DirectoryInfo outputDir = new DirectoryInfo("enwiki");
				bool keepImageOnlyDocs = true;
				for (int i = 0; i < args.Length; i++) {
					String arg = args[i];
					if (arg.Equals("--input") || arg.Equals("-i")) {
						wikipedia = new FileInfo(args[i + 1]);
						i++;
					} else if (arg.Equals("--output") || arg.Equals("-o")) {
						outputDir = new DirectoryInfo(args[i + 1]);
						i++;
					} else if (arg.Equals("--discardImageOnlyDocs") || arg.Equals("-d")) {
						keepImageOnlyDocs = false;
					}

				}
				// TODO: Need to add this back in
				//DocMaker docMaker = new EnwikiDocMaker();
				JavaProperties properties = new JavaProperties();

				properties.SetProperty("docs.file", wikipedia.FullName);
				properties.SetProperty("content.source.forever", "false");
				properties.SetProperty("keep.image.only.docs", keepImageOnlyDocs.ToString());
				// TODO: add back in
				//docMaker.setConfig(new Config(properties));
				//docMaker.resetInputs();
				if (wikipedia != null && wikipedia.Exists) {
					Console.WriteLine("Extracting Wikipedia to: " + outputDir + " using EnwikiDocMaker");
					outputDir.Create();
					// TODO ADD BACK IN
					//ExtractWikipedia extractor = new ExtractWikipedia(docMaker, outputDir);
					//extractor.extract();
				} else {
					printUsage();
				}
			}

			private static void printUsage() {
				Console.WriteLine("Usage: ExtractWikipedia -cp <...> Lucene.Net.Benchmark.Utils.ExtractWikipedia --input|-i <Path to Wikipedia XML file> " +
						"[--output|-o <Output Path>] [--discardImageOnlyDocs|-d] [--useLineDocMaker|-l]");
				Console.WriteLine("--discardImageOnlyDocs tells the extractor to skip Wiki docs that contain only images");
				Console.WriteLine("--useLineDocMaker uses the LineDocMaker.  Default is EnwikiDocMaker");
			}
	}
}
