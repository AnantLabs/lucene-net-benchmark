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
using System.Reflection;

using Lucene.Net.Analysis;

using RTools.Util;


namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Create a new {@link org.apache.lucene.analysis.Analyzer} and set it it in the getRunData() for use by all future tasks.
	 *
	 */
	public class NewAnalyzerTask : PerfTask {

		  private ArrayList/*<String>*/ analyzerClassNames;
		  private int current;

		  public NewAnalyzerTask(PerfRunData runData) : base(runData) {
			analyzerClassNames = new ArrayList();
		  }

		  public override int doLogic() {
			String className = null;
			try {
			  if (current >= analyzerClassNames.Count)
			  {
				current = 0;
			  }
			  className = (String) analyzerClassNames[current++];
			  if (className == null || className.Equals(""))
			  {
				className = "Lucene.Net.Analysis.Standard.StandardAnalyzer"; 
			  }
			  if (className.IndexOf(".") == -1  || className.StartsWith("Standard."))//there is no package name, assume o.a.l.analysis
			  {
				className = "Lucene.Net.Analysis." + className;
			  }
				// TODO: assembly stuff
			  Assembly assembly = Assembly.LoadFrom("Lucene.Net.dll");
			  getRunData().setAnalyzer((Analyzer) assembly.CreateInstance(className));
				  //Class.forName(className).newInstance());
			  Benchmark.LogSheet.AddRowAndCell( "Changed Analyzer to: " + className );
			  Console.WriteLine("Changed Analyzer to: " + className);
			} catch (Exception e) {
			  throw new ApplicationException("Error creating Analyzer: " + className, e);
			}
			return 1;
			  //  throws IOException
		  }

		  /**
		   * Set the params (analyzerClassName only),  Comma-separate list of Analyzer class names.  If the Analyzer lives in
		   * org.apache.lucene.analysis, the name can be shortened by dropping the o.a.l.a part of the Fully Qualified Class Name.
		   * <p/>
		   * Example Declaration: {"NewAnalyzer" NewAnalyzer(WhitespaceAnalyzer, SimpleAnalyzer, StopAnalyzer, standard.StandardAnalyzer) >
		   * @param params analyzerClassName, or empty for the StandardAnalyzer
		   */
		  public override void setParams(String args) {
			  base.setParams(args);
			  RTools.Util.Token t;
			  StreamTokenizer tokenizer = new StreamTokenizer(args);
			  tokenizer.Settings.WhitespaceChar(',');
			  tokenizer.Settings.WordChar( '.' );
			while ( tokenizer.NextToken(out t)) {
				String s = t.StringValue;
			  analyzerClassNames.Add(s.Trim());
			}
		  }

		  /* (non-Javadoc)
		   * @see org.apache.lucene.benchmark.byTask.tasks.PerfTask#supportsParams()
		   */
		  public override bool supportsParams() {
			return true;
		  }
	} // end of class
} // end of namespace3
