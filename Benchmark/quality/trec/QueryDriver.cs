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
using Lucene.Net.Benchmark.quality.utils;
using Lucene.Net.Search;

namespace Lucene.Net.Benchmark.quality.trec {
	
	
	public class QueryDriver {

		  public static void main(String[] args) {

    
			FileInfo topicsFile = new FileInfo(args[0]);
			FileInfo qrelsFile = new FileInfo(args[1]);
			Searcher searcher = new IndexSearcher(args[3]);

			int maxResults = 1000;
			String docNameField = "docname";

			System.IO.StreamWriter outWriter;
			outWriter = new System.IO.StreamWriter( Console.OpenStandardOutput(), System.Console.Out.Encoding );
			outWriter.AutoFlush = true;

			// use trec utilities to read trec topics into quality queries
			TrecTopicsReader qReader = new TrecTopicsReader();
			QualityQuery[] qqs = qReader.readQueries(topicsFile.OpenText());

			// prepare judge, with trec utilities that read from a QRels file
			Judge judge = new TrecJudge(qrelsFile.OpenText());

			// validate topics & judgments match each other
			judge.validateData(qqs, outWriter);

			// set the parsing of quality queries into Lucene queries.
			QualityQueryParser qqParser = new SimpleQQParser("title", "body");

			// run the benchmark
			QualityBenchmark qrun = new QualityBenchmark(qqs, qqParser, searcher, docNameField);
			qrun.setMaxResults(maxResults);
			SubmissionReport submitLog = null;
			QualityStats[] stats = qrun.execute(judge, submitLog, outWriter);

			// print an avarage sum of the results
			QualityStats avg = QualityStats.average(stats);
			avg.log("SUMMARY", 2, outWriter, "  ");
		  }

	}
}
