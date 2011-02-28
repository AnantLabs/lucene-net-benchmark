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
using System.Threading;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Spawns a BG thread that periodically (defaults to 3.0
	 * seconds, but accepts param in seconds) wakes up and asks
	 * IndexWriter for a near real-time reader.  Then runs a
	 * single query (body: 1) sorted by docdate, and prints
	 * time to reopen and time to run the search.
	 *
	 * <b>NOTE</b>: this is very experimental at this point, and
	 * subject to change.  It's also not generally usable, eg
	 * you cannot change which query is executed.
	 */
	public class NearRealtimeReaderTask : PerfTask  {

		  float pauseSec = 3.0f;
		  static Thread t;
		  ReopenThread rt;

		  public class ReopenThread {

			  readonly IndexWriter writer;
			  readonly int pauseMsec;

			  public volatile bool done;

			  public ReopenThread( IndexWriter writer, float pauseSec ) {
				  this.writer = writer;
				  this.pauseMsec = (int)( 1000 * pauseSec );
				  t = new Thread( new ThreadStart( this.run ) );
			  }

			  public void run() {

				  IndexReader reader = null;

				  Query query = new TermQuery( new Term( "body", "1" ) );
				  SortField sf = new SortField( "docdate", SortField.LONG );
				  Sort sort = new Sort( sf );

				  try {
					  while( !done ) {
						  DateTime t0 = DateTime.Now;
						  if( reader == null ) {
							  reader = writer.GetReader();
						  }
						  else {
							  IndexReader newReader = reader.Reopen();
							  if( reader != newReader ) {
								  reader.Close();
								  reader = newReader;
							  }
						  }

						  DateTime t1 = DateTime.Now;
						  TopFieldDocs hits = new IndexSearcher( reader ).Search( query, null, 10, sort );
						  DateTime t2 = DateTime.Now;
						  Benchmark.LogSheet.AddRowAndCell( "nrt: open " + ( t1 - t0 ) + " msec; search " + ( t2 - t1 ) + " msec, " + hits.totalHits +
										   " results; " + reader.NumDocs() + " docs" );
						  Console.WriteLine( "nrt: open " + ( t1 - t0 ) + " msec; search " + ( t2 - t1 ) + " msec, " + hits.totalHits +
										   " results; " + reader.NumDocs() + " docs" );

						  DateTime t4 = DateTime.Now;
						  int delay = (int)( pauseMsec - ( t4 - t0 ).TotalMilliseconds );
						  if( delay > 0 ) {
							  try {
								  Thread.Sleep( delay );
							  }
							  catch( ThreadInterruptedException ie ) {
								  throw new ApplicationException( "Thread problem in NearRealTimeReaderTask", ie );
							  }
						  }
					  }
				  }
				  catch( Exception e ) {
					  throw new ApplicationException( "Thread problem in NearRealTimeReaderTask", e );
				  }
			  }
		  }

		  public NearRealtimeReaderTask(PerfRunData runData) : base(runData) {
			// do nothing
		  }

		  public override int doLogic() {
			  if( t == null ) {
				  IndexWriter w = getRunData().getIndexWriter();
				  rt = new ReopenThread ( w, pauseSec );
			  }
			  return 1;
		  }

		  public override void setParams(String args) {
			base.setParams(args);
			pauseSec = float.Parse(args);
		  }

		  public override bool supportsParams() {
			return true;
		  }

		  // Close the thread
		  public override void close() {
			  if( t == null ) {
				  t.Abort();
				  t.Join();
			  }
		  }
	}
}
