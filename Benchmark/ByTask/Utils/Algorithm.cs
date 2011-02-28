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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Lucene.Net.Benchmark.ByTask.Tasks;
using RTools.Util;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

// TODO: redo file with out the rTools, though this is low priority

namespace Lucene.Net.Benchmark.ByTask.Utils {

	public class Algorithm {

		private TaskSequence sequence;
  
		/**
		* Read algorithm from file
		* @param runData perf-run-data used at running the tasks.
		* @throws Exception if errors while parsing the algorithm 
		*/

		public Algorithm (PerfRunData runData) {
			// TODO: change the output of getAlgorithmText so that
			// I get an array of strings so I can treat it like
			// a text reader
			String algTxt = runData.getConfig().getAlgorithmText();
			sequence = new TaskSequence(runData,null,null,false);
			TaskSequence currSequence = sequence;
			PerfTask prevTask = null;
			// we'll use the streamtokenizer in RTools to take the place
			// of the java StreamTokenizer - Jason Ramsey 1/30/11
			StreamTokenizerWithPeek stok = new StreamTokenizerWithPeek(algTxt);
			stok.Settings.CommentChar('#');
			stok.Settings.GrabEol = false;
			stok.Settings.GrabComments = false;
			stok.Settings.OrdinaryChar('"');
			stok.Settings.OrdinaryChar('/');
			stok.Settings.OrdinaryChar('(');
			stok.Settings.OrdinaryChar(')');
			stok.Settings.OrdinaryChar('-');

			bool colonOk = false; 
			bool isDisableCountNextTask = false; // only for primitive tasks
			currSequence.setDepth(0);
			String taskPackage = typeof(PerfTask).Namespace + ".";
    
			Type[] paramClass = { typeof( PerfRunData)};
			PerfRunData[] paramObj  = {runData};
			Token t;
			//stok.NextToken(out t);
			while (stok.NextToken(out t)) { 
					switch(t.type) {
						case CharTypeBits.Word:
							 String s = t.StringValue;
							 Type temp = Type.GetType( taskPackage + s + "Task" );
							ConstructorInfo cnstr = Type.GetType( taskPackage + s + "Task" ).GetConstructor( paramClass );
							PerfTask task = (PerfTask) cnstr.Invoke(paramObj);
							task.setDisableCounting(isDisableCountNextTask);
							isDisableCountNextTask = false;
							currSequence.addTask(task);
							if (task is RepSumByPrefTask) {
								stok.NextToken(out t);
								String prefix = t.StringValue;
								if (prefix==null || prefix.Length==0) { 
									throw new Exception("named report prefix problem - "+stok.ToString()); 
								}
								((RepSumByPrefTask) task).setPrefix(prefix);
							}
							// check for task param: '(' someParam ')'
							Char nextC = (Char)stok.PeekNextChar();
							//if (t.type == CharTypeBits.Char && Char.Parse(t.ToString()) !='(') {
							if (nextC != '(') {
								// TODO: I don't know what to replace this with yet.  I'll figure it out
								// during debugging
								//stok.pushBack();
								// do nothing, 
							} else {
								stok.NextToken( out t );
								// get params, for tasks that supports them, - anything until next ')'
								StringBuilder args = new StringBuilder();
								stok.NextToken(out t);
								bool notDone = true;
								while (notDone)
								{
									if (t.StringValue.Length == 1)
									{
										if (Char.Parse(t.StringValue) == ')')
										{
											notDone = false;
											break;
										}
									}
								switch (t.type) {
										case CharTypeBits.Digit:  
											args.Append((int)t.Object);
											break;
										case CharTypeBits.Word:    
											args.Append(t.StringValue);             
											break;
										case CharTypeBits.Eof:     
											throw new Exception("unexpexted EOF: - "+stok.ToString());
										case CharTypeBits.Int:
											args.Append(int.Parse(t.StringValue));
											break;
										default:
											args.Append(Char.Parse(t.StringValue));
											break;
									}
									stok.NextToken(out t);
								}
								String prm = args.ToString().Trim();
								if (prm.Length>0) {
									task.setParams(prm);
								}
							}

						// ---------------------------------------
						colonOk = false; prevTask = task;
						break;
  
					default:
						char c = Char.Parse(t.StringValue);
          
						switch(c) {
          
						case ':' :
							if (!colonOk) throw new Exception("colon unexpexted: - "+stok.ToString());
							colonOk = false;
							// get repetitions number
							stok.NextToken(out t);
							bool asterikTest = false;
							if (t.StringValue.Length == 1)
							{
								if (Char.Parse(t.StringValue) == '*')
								{
									asterikTest = true;
								}
							}
							if (asterikTest) {
								((TaskSequence)prevTask).setRepetitions(TaskSequence.REPEAT_EXHAUST);
							} else {
								if (!Regex.IsMatch(t.StringValue,"^\\d+$"))  {
									throw new Exception("expected repetitions number or XXXs: - "+stok.ToString());
								} else {
									double num = double.Parse(t.StringValue);
									nextC = (Char)stok.PeekNextChar();
									if (t.type == CharTypeBits.Word && t.StringValue.Equals("s")) {
										stok.NextToken( out t );
										((TaskSequence) prevTask).setRunTime(num);
									} else {
										//stok.pushBack();
										((TaskSequence) prevTask).setRepetitions((int) num);
									}
								}
							}
							// check for rate specification (ops/min)
							nextC = (Char)stok.PeekNextChar();
							if (nextC!=':') {
								// TODO: Here we go again
								//stok.pushBack();
							} else {
								// to get rid of colon
								stok.NextToken( out t ); 
								// get rate number
								stok.NextToken(out t);
								if (t.type==CharTypeBits.Digit) throw new Exception("expected rate number: - "+stok.ToString());
								// check for unit - min or sec, sec is default
								nextC = (Char)stok.PeekNextChar();
								if (nextC!='/') {
									// TODO: and again
									//stok.pushBack();
									((TaskSequence)prevTask).setRate(int.Parse(t.StringValue),false); // set rate per sec
								} else {
									stok.NextToken(out t);
									if (t.type!=CharTypeBits.Word) throw new Exception("expected rate unit: 'min' or 'sec' - "+stok.ToString());
									String unit = t.StringValue.ToLower();
									if ("min".Equals(unit)) {
									((TaskSequence)prevTask).setRate(int.Parse(t.StringValue),true); // set rate per min
									} else if ("sec".Equals(unit)) {
									((TaskSequence)prevTask).setRate(int.Parse(t.StringValue),false); // set rate per sec
									} else {
									throw new Exception("expected rate unit: 'min' or 'sec' - "+stok.ToString());
									}
								}
							}
							colonOk = false;
							break;
    
						case '{' : 
						case '[' :  
							// a sequence
							// check for sequence name
							String name = null;
								// TODO: this needs to be a peek
							nextC = (Char)stok.PeekNextChar();
							if (nextC !='"') {
								//stok.pushBack();
								// do nothing
							} else {
								// this just gets rid of the quote we found
								stok.NextToken( out t );
								stok.NextToken(out t);
								name = t.StringValue;
								stok.NextToken(out t);
								if (char.Parse(t.StringValue)!='"' || name==null || name.Length==0) { 
									throw new Exception("sequence name problem - "+stok.ToString()); 
								}
							}
							// start the sequence
							TaskSequence seq2 = new TaskSequence(runData, name, currSequence, c=='[');
							currSequence.addTask(seq2);
							currSequence = seq2;
							colonOk = false;
							break;
    
						case '>' :
							currSequence.setNoChildReport();
							goto FallThrough;
						case '}' : 
						case ']' : 
						FallThrough:
							// end sequence
							colonOk = true; prevTask = currSequence;
							currSequence = currSequence.getParent();
							break;
          
						case '-' :
							isDisableCountNextTask = true;
							break;
              
						} //switch(c)
						break;
          
					} //switch(stok.ttype)
      
				}
		
    
    if (sequence != currSequence) {
      throw new Exception("Unmatched sequences");
    }
    
    // remove redundant top level enclosing sequences
    while (sequence.isCollapsable() && sequence.getRepetitions()==1 && sequence.getRate()==0) {
      ArrayList tList = sequence.getTasks();
      if (t!=null && tList.Count==1) {
        PerfTask p = (PerfTask) tList[0];
        if (p is TaskSequence) {
          sequence = (TaskSequence) p;
          continue;
        }
      }
      break;
    }
  }

  /* (non-Javadoc)
   * @see java.lang.Object#toString()
   */
  public String toString() {
    String newline = Environment.NewLine;
    StringBuilder sb = new StringBuilder();
    sb.Append(sequence.toString());
    sb.Append(newline);
    return sb.ToString();
  }

  /**
   * Execute this algorithm
   * @throws Exception 
   */
  public void execute() {
    try {
      sequence.runAndMaybeStats(true);
    } finally {
      sequence.close();
    }
  }

  /**
   * Expert: for test purposes, return all tasks participating in this algorithm.
   * @return all tasks participating in this algorithm.
   */
  public ArrayList extractTasks() {
    ArrayList res = new ArrayList();
    extractTasks(res, sequence);
    return res;
  }

  private void extractTasks (ArrayList extrct, TaskSequence seq) {
    if (seq==null) 
      return;
    extrct.Add(seq);
    ArrayList t = sequence.getTasks();
    if (t==null) 
      return;
    for (int i = 0; i < t.Count; i++) {
      PerfTask p = (PerfTask) t[0];
      if (p is TaskSequence) {
        extractTasks(extrct, (TaskSequence)p);
      } else {
        extrct.Add(p);
      }
    }
  }
	}
}
