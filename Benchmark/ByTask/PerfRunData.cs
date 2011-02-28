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
using System.Reflection;
using System.Runtime.CompilerServices;
using Lucene.Net.Analysis;
using Lucene.Net.Benchmark.ByTask.Feeds;
using Lucene.Net.Benchmark.ByTask.Stats;
using Lucene.Net.Benchmark.ByTask.Tasks;
using Lucene.Net.Benchmark.ByTask.Utils;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Lucene.Net.Benchmark.ByTask
{
	public class PerfRunData {

		private Points points;
  
		// objects used during performance test run
		// directory, analyzer, docMaker - created at startup.
		// reader, writer, searcher - maintained by basic tasks. 
		private Lucene.Net.Store.Directory directory;
		private Analyzer analyzer;
		private DocMaker docMaker;
  
		// we use separate (identical) instances for each "read" task type, so each can iterate the quries separately.
		private Hashtable readTaskQueryMaker;
		private Type qmkrClass;

		private IndexReader indexReader;
		private IndexSearcher indexSearcher;
		private IndexWriter indexWriter;
		private Config config;
		private DateTime startTimeMillis;
  
		// constructor
		public PerfRunData (Config config) {
			this.config = config;
			// analyzer (default is standard analyzer)
			Assembly assembly = Assembly.LoadFrom( "Lucene.Net.dll");

			string analyzerName = config.get( "analyzer", "Lucene.Net.Analysis.Standard.StandardAnalyzer" );

			analyzer = (Analyzer)assembly.CreateInstance( analyzerName );

			Type docMakerType = Type.GetType( config.get( "doc.maker", "Lucene.Net.Benchmark.ByTask.Feeds.DocMaker" ) );
			// doc maker
			docMaker = (DocMaker) Activator.CreateInstance(docMakerType);
			docMaker.setConfig(config);
			// query makers
			readTaskQueryMaker = new Hashtable();
			assembly = Assembly.LoadFrom( "Benchmark.exe" );
			string queryMakerClass = config.get( "query.maker", "Lucene.Net.Benchmark.ByTask.Feeds.SimpleQueryMaker" );
			qmkrClass = assembly.GetType(queryMakerClass);

			// index stuff
			reinit(false);
    
			// statistic points
			points = new Points(config);
    
			if (config.get("log.queries","false") != null) {
				Benchmark.LogSheet.AddRowAndCell( "------------> queries:" );
				Console.WriteLine("------------> queries:");
				Benchmark.LogSheet.AddRowsAndCellsSplit( getQueryMaker( new SearchTask( this ) ).printQueries() );
				Console.WriteLine(getQueryMaker(new SearchTask(this)).printQueries());
			 }

		}

		// clean old stuff, reopen 
		public void reinit(bool eraseIndex) {

			// cleanup index
			if (indexWriter!=null) {
				indexWriter.Close();
				indexWriter = null;
			}
			if (indexReader!=null) {
				indexReader.Close();
				indexReader = null;
			}
			if (directory!=null) {
				directory.Close();
			}
    
			// directory (default is ram-dir).
			if (config.get("directory","RAMDirectory") == "FSDirectory") {
				DirectoryInfo workDir = new DirectoryInfo(config.get("work.dir","work"));
				DirectoryInfo indexDir = new DirectoryInfo(Path.Combine(workDir.FullName,"index"));
				if (eraseIndex && indexDir.Exists) {
					FileUtils.fullyDelete(indexDir);
				}
				indexDir.Create();
				directory = FSDirectory.Open(indexDir);
			} else {
				directory = new RAMDirectory();
			}

			// inputs
			resetInputs();
    
			// release unused stuff
			// not a clear alternative to this so I'm just commenting it 
			// - Jason Ramsey 1/26/11
			//System.runFinalization();
			//System.gc();

			// Re-init clock
			setStartTimeMillis();
		}
  
		// for compatibility reasons I am going to keep the 
		// method name "setStartTimeMillis", but I am going
		// to reutrn a datetime instead.
		public DateTime setStartTimeMillis() {
			startTimeMillis = DateTime.Now;
			return startTimeMillis;
		}

		/**
   * @return Start time in milliseconds
   */
		public DateTime getStartTimeMillis() {
			return startTimeMillis;
		}

		/**
   * @return Returns the points.
   */
		public Points getPoints() {
			return points;
		}

		/**
   * @return Returns the directory.
   */
		public Lucene.Net.Store.Directory getDirectory() {
			return directory;
		}

		/**
   * @param directory The directory to set.
   */
		public void setDirectory(Lucene.Net.Store.Directory directory) {
			this.directory = directory;
		}

		/**
   * @return Returns the indexReader.
   */
		public IndexReader getIndexReader() {
			return indexReader;
		}

		/**
   * @return Returns the indexSearcher.
   */
		public IndexSearcher getIndexSearcher() {
			return indexSearcher;
		}

		/**
   * @param indexReader The indexReader to set.
   */
		public void setIndexReader(IndexReader indexReader) {
			this.indexReader = indexReader;
			if (indexReader != null) {
				indexSearcher = new IndexSearcher(indexReader);
			} else {
				indexSearcher = null;
			}
		}

		/**
   * @return Returns the indexWriter.
   */
		public IndexWriter getIndexWriter() {
			return indexWriter;
		}

		/**
   * @param indexWriter The indexWriter to set.
   */
		public void setIndexWriter(IndexWriter indexWriter) {
			this.indexWriter = indexWriter;
		}

		/**
   * @return Returns the anlyzer.
   */
		public Analyzer getAnalyzer() {
			return analyzer;
		}


		public void setAnalyzer(Analyzer analyzer) {
			this.analyzer = analyzer;
		}

		/** Returns the docMaker. */
		public DocMaker getDocMaker() {
			return docMaker;
		}

		/**
   * @return Returns the config.
   */
		public Config getConfig() {
			return config;
		}

		public void resetInputs()
		{
			docMaker.resetInputs();
			foreach(Type key in readTaskQueryMaker.Keys)
			{
				if (readTaskQueryMaker[key] != null)
				{
					QueryMaker qm = (QueryMaker)readTaskQueryMaker[ key ];
					qm.resetInputs();
				}
			}
		}

		/**
		* @return Returns the queryMaker by read task type (class)
		*/
		[MethodImpl(MethodImplOptions.Synchronized)]
		public QueryMaker getQueryMaker(ReadTask readTask) {
			// mapping the query maker by task class allows extending/adding new search/read tasks
			// without needing to modify this class.
			Type readTaskClass = readTask.GetType();
			QueryMaker qm = (QueryMaker) readTaskQueryMaker[readTaskClass];
			if (qm == null) {
				try {
					Assembly assembly = Assembly.LoadFrom( "Benchmark.exe" );
					qm = (QueryMaker) assembly.CreateInstance(qmkrClass.FullName);
					qm.setConfig(config);
				} catch (Exception e) {
					throw new ApplicationException("Unknown Exception", e);
				}
				readTaskQueryMaker.Add(readTaskClass,qm);
			}
			return qm;
		} // end of method
	} // end of class
} // end of namespace
