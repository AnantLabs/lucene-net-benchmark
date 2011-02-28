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
using System.Text;
using System.Threading;
using Lucene.Net.Benchmark.ByTask.Stats;
using Lucene.Net.Benchmark.ByTask.Utils;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * An abstract task to be tested for performance. <br>
	 * Every performance task extends this class, and provides its own
	 * {@link #doLogic()} method, which performs the actual task. <br>
	 * Tasks performing some work that should be measured for the task, can override
	 * {@link #setup()} and/or {@link #tearDown()} and place that work there. <br>
	 * Relevant properties: <code>task.max.depth.log</code>.<br>
	 * Also supports the following logging attributes:
	 * <ul>
	 * <li>log.step - specifies how often to log messages about the current running
	 * task. Default is 1000 {@link #doLogic()} invocations. Set to -1 to disable
	 * logging.
	 * <li>log.step.[class Task Name] - specifies the same as 'log.step', only for a
	 * particular task name. For example, log.step.AddDoc will be applied only for
	 * {@link AddDocTask}, but not for {@link DeleteDocTask}. It's a way to control
	 * per task logging settings. If you want to omit logging for any other task,
	 * include log.step=-1. The syntax is "log.step." together with the Task's
	 * 'short' name (i.e., without the 'Task' part).
	 * </ul>
	 */
	public abstract class PerfTask : ICloneable {

		const int DEFAULT_LOG_STEP = 1000;
  
		private PerfRunData runData;
  
		// propeties that all tasks have
		private String name;
		private int depth = 0;
		protected int logStep;
		private int logStepCount = 0;
		private int maxDepthLogStart = 0;
		private bool disableCounting = false;
		protected String args = null;
  
		protected static readonly String NEW_LINE = Environment.NewLine;

		/** Should not be used externally */
		private PerfTask() {
			name = Format.simpleName(this.GetType());
			if (name.EndsWith("Task")) {
				name = name.Substring(0, name.Length - 4);
			}
		}

		/**
		* @deprecated will be removed in 3.0. checks if there are any obsolete
		*             settings, like doc.add.log.step and doc.delete.log.step and
		*             alerts the user.
		*/
		private void checkObsoleteSettings(Config config) {
			if (config.get("doc.add.log.step", null) != null) {
				throw new ApplicationException("doc.add.log.step is not supported anymore. " +
      				"Use log.step.AddDoc and refer to CHANGES to read on the recent " +
      				"API changes done to Benchmark's DocMaker and Task-based logging.");
			}
    
			if (config.get("doc.delete.log.step", null) != null) {
				throw new ApplicationException("doc.delete.log.step is not supported anymore. " +
					"Use log.step.DeleteDoc and refer to CHANGES to read on the recent " +
					"API changes done to Benchmark's DocMaker and Task-based logging.");
			}
		}
  
		public PerfTask(PerfRunData runData) : this() {

			this.runData = runData;
			Config config = runData.getConfig();
			this.maxDepthLogStart = config.get("task.max.depth.log",0);

			String logStepAtt = "log.step";
			// TODO (1.5): call getClass().getSimpleName() instead.
			String taskName = this.GetType().Name;
			int idx = taskName.LastIndexOf('.');
			if( idx == -1 )
				idx = 0;
			// To support test internal classes. when we move to getSimpleName, this can be removed.
			int idx2 = taskName.IndexOf('$', idx);
			if (idx2 != -1) idx = idx2;
			if( idx > 0 )
				idx++;
			String taskLogStepAtt = "log.step." + taskName.Substring(idx, taskName.Length - 4 /* w/o the 'Task' part */);
			if (config.get(taskLogStepAtt, null) != null) {
				logStepAtt = taskLogStepAtt;
			}

			// It's important to read this from Config, to support vals-by-round.
			logStep = config.get(logStepAtt, DEFAULT_LOG_STEP);
			// To avoid the check 'if (logStep > 0)' in tearDown(). This effectively
			// turns logging off.
			if (logStep <= 0) {
				logStep = int.MaxValue;
			}
			checkObsoleteSettings(config);
		}
  
		public virtual Object clone() {
			// tasks having non primitive data structures should override this.
			// otherwise parallel running of a task sequence might not run correctly. 
			return  base.MemberwiseClone();
		}

		public virtual void close() {
			// do nothing
		}

		/**
		* Run the task, record statistics.
		* @return number of work items done by this task.
		*/
		public int runAndMaybeStats(bool reportStats) {
			int count;
			if (reportStats && depth <= maxDepthLogStart && !shouldNeverLogAtStart()) {
				Benchmark.LogSheet.AddRowAndCell( "------------> starting task: " + getName() );
				Console.WriteLine("------------> starting task: " + getName());
			}
			if (!reportStats || shouldNotRecordStats()) {
				setup();
				count = doLogic();
				count = disableCounting ? 0 : count;
				tearDown();
				return count;
			}
			setup();
			Points pnts = runData.getPoints();
			TaskStats ts = pnts.markTaskStart(this, runData.getConfig().getRoundNumber());
			count = doLogic();
			count = disableCounting ? 0 : count;
			pnts.markTaskEnd(ts, count);
			tearDown();
			return count;
		}

		/**
		* Perform the task once (ignoring repetitions specification)
		* Return number of work items done by this task.
		* For indexing that can be number of docs added.
		* For warming that can be number of scanned items, etc.
		* @return number of work items done by this task.
		*/
		public abstract int doLogic();
		//throws Exception
  
		/**
		* @return Returns the name.
		*/
		public virtual String getName() {
			if (args==null) {
				return name;
			} 
			return new StringBuilder(name).Append('(').Append(args).Append(')').ToString();
		}

		/**
		* @param name The name to set.
		*/
		protected void setName(String name) {
			this.name = name;
		}

		/**
		* @return Returns the run data.
		*/
		public PerfRunData getRunData() {
			return runData;
		}

		/**
		* @return Returns the depth.
		*/
		public int getDepth() {
			return depth;
		}

		/**
		* @param depth The depth to set.
		*/
		public void setDepth(int depth) {
			this.depth = depth;
		}
  
		// compute a blank string padding for printing this task indented by its depth  
		public String getPadding () {
			char[] c = new char[4*getDepth()];
			for (int i = 0; i < c.Length; i++) c[i] = ' ';
			return new String(c);
		}
  
		/* (non-Javadoc)
		* @see java.lang.Object#toString()
		*/
		public virtual String toString() {
			String padd = getPadding();
			StringBuilder sb = new StringBuilder(padd);
			if (disableCounting) {
				sb.Append('-');
			}
			sb.Append(getName());
			return sb.ToString();
		}

		/**
		* @return Returns the maxDepthLogStart.
		*/
		int getMaxDepthLogStart() {
			return maxDepthLogStart;
		}

		protected virtual String getLogMessage(int recsCount) {
			return "processed " + recsCount + " records";
		}
  
		/**
		* Tasks that should never log at start can override this.  
		* @return true if this task should never log when it start.
		*/
		protected virtual bool shouldNeverLogAtStart () {
			return false;
		}
  
		/**
		* Tasks that should not record statistics can override this.  
		* @return true if this task should never record its statistics.
		*/
		protected virtual bool shouldNotRecordStats () {
			return false;
		}

		/**
		* Task setup work that should not be measured for that specific task.
		* By default it does nothing, but tasks can implement this, moving work from 
		* doLogic() to this method. Only the work done in doLogicis measured for this task.
		* Notice that higher level (sequence) tasks containing this task would then 
		* measure larger time than the sum of their contained tasks.
		* @throws Exception 
		*/
		public virtual void setup () {
			//  throws Exception
		}
  
		/**
		* Task tearDown work that should not be measured for that specific task.
		* By default it does nothing, but tasks can implement this, moving work from 
		* doLogic() to this method. Only the work done in doLogicis measured for this task.
		* Notice that higher level (sequence) tasks containing this task would then 
		* measure larger time than the sum of their contained tasks.
		*/
		public virtual void tearDown() {
			if (++logStepCount % logStep == 0) {
				TimeSpan time = (DateTime.Now - runData.getStartTimeMillis());
				Benchmark.LogSheet.AddRowAndCell( ( time.TotalMilliseconds / 1000 ) + " sec --> "
					+ Thread.CurrentThread.Name + " " + getLogMessage( logStepCount ) );
				Console.WriteLine((time.TotalMilliseconds / 1000) + " sec --> "
					+ Thread.CurrentThread.Name + " " + getLogMessage(logStepCount));
			}
			//  throws Exception 
		}

		/**
		* Sub classes that supports parameters must override this method to return true.
		* @return true iff this task supports command line args.
		*/
		public virtual bool supportsParams () {
			return false;
		}
  
		/**
		* Set the args of this task.
		* @exception UnsupportedOperationException for tasks supporting command line parameters.
		*/
		public virtual void setParams(String args) {
			if (!supportsParams()) {
				throw new ApplicationException(getName()+" does not support command line parameters.");
			}
			this.args = args;
		}
  
		/**
		* @return Returns the args.
		*/
		public String getParams() {
			return args;
		}

		/**
		* Return true if counting is disabled for this task.
		*/
		public bool isDisableCounting() {
			return disableCounting;
		}

		/**
		* See {@link #isDisableCounting()}
		*/
		public void setDisableCounting(bool disableCounting) {
			this.disableCounting = disableCounting;
		}

		#region ICloneable Members

		public virtual object Clone() {
			return clone();
		}

		#endregion
	}
}
