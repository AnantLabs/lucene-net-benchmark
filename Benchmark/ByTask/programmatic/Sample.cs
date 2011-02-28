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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Net.Benchmark.ByTask.programmatic {

	/**
	 * Sample performance test written programmatically - no algorithm file is needed here.
	 */
	public class Sample {


		 /**
		   * @param args
		   * @throws Exception 
		   * @throws IOException 
		   */
		  public static void main(String[] args) {
			JavaProperties p = initProps();
			Config conf = new Config(p);
			PerfRunData runData = new PerfRunData(conf);
    
			// 1. top sequence
			TaskSequence top = new TaskSequence(runData,null,null,false); // top level, not parallel
    
			// 2. task to create the index
			CreateIndexTask create = new CreateIndexTask(runData);
			top.addTask(create);
    
			// 3. task seq to add 500 docs (order matters - top to bottom - add seq to top, only then add to seq)
			TaskSequence seq1 = new TaskSequence(runData,"AddDocs",top,false);
			seq1.setRepetitions(500);
			seq1.setNoChildReport();
			top.addTask(seq1);

			// 4. task to add the doc
			AddDocTask addDoc = new AddDocTask(runData);
			//addDoc.setParams("1200"); // doc size limit if supported
			seq1.addTask(addDoc); // order matters 9see comment above)

			// 5. task to close the index
			CloseIndexTask close = new CloseIndexTask(runData);
			top.addTask(close);

			// task to report
			RepSumByNameTask rep = new RepSumByNameTask(runData);
			top.addTask(rep);

			// print algorithm
		  	Console.WriteLine(top.toString());
    
			// execute
			top.doLogic();
		  }

		  // Sample programmatic settings. Could also read from file.
		  private static JavaProperties initProps() {
			JavaProperties p = new JavaProperties();
			p.SetProperty ( "task.max.depth.log"  , "3" );
			p.SetProperty ( "max.buffered"        , "buf:10:10:100:100:10:10:100:100" );
			p.SetProperty ( "doc.maker"           , "Lucene.Net.Benchmark.ByTask.Feeds.ReutersContentSource" );
			p.SetProperty ( "log.step"            , "2000" );
			p.SetProperty ( "doc.delete.step"     , "8" );
			p.SetProperty ( "analyzer"            , "Lucene.Net.Analysis.Standard.StandardAnalyzer" );
			p.SetProperty ( "doc.term.vector"     , "false" );
			p.SetProperty ( "directory"           , "FSDirectory" );
			p.SetProperty ( "query.maker"         , "Lucene.Net.Benchmark.ByTask.Feeds.ReutersQueryMaker" );
			p.SetProperty ( "doc.stored"          , "true" );
			p.SetProperty ( "docs.dir"            , "reuters-out" );
			p.SetProperty ( "compound"            , "cmpnd:true:true:true:true:false:false:false:false" );
			p.SetProperty ( "doc.tokenized"       , "true" );
			p.SetProperty ( "merge.factor"        , "mrg:10:100:10:100:10:100:10:100" );
			return p;
		  }
	}
}
