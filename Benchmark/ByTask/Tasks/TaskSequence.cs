
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
using System.Text;
using System.Threading;
using Lucene.Net.Benchmark.ByTask.Feeds;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Sequence of parallel or sequential tasks.
	 */
	class TaskSequence : PerfTask {
		public static int REPEAT_EXHAUST = -2; 
		private ArrayList tasks;
		private int repetitions = 1;
		private bool parallel;
		private TaskSequence parent;
		private bool letChildReport = true;
		private int rate = 0;
		private bool perMin = false; // rate, if set, is, by default, be sec.
		private String seqName;
		private bool exhausted = false;
		private bool resetExhausted = false;
		private PerfTask[] tasksArray;
		private bool anyExhaustibleTasks;
		private bool collapsable = false; // to not collapse external sequence named in alg.  
  
		private bool fixedTime;                      // true if we run for fixed time
		private double runTimeSec;                      // how long to run for

		public TaskSequence (PerfRunData runData, String name, TaskSequence parent, bool parallel) : base(runData) {
			collapsable = (name == null);
			name = (name!=null ? name : (parallel ? "Par" : "Seq"));
			setName(name);
			setSequenceName();
			this.parent = parent;
			this.parallel = parallel;
			tasks = new ArrayList();
		}

		public override void close() {
			initTasksArray();
			for(int i=0;i<tasksArray.Length;i++) {
				tasksArray[i].close();
			}
			getRunData().getDocMaker().close();
		}

		public void initTasksArray() {
			if (tasksArray == null) {
				int numTasks = tasks.Count;
				tasksArray = new PerfTask[numTasks];
				for(int k=0;k<numTasks;k++) {
					tasksArray[k] = (PerfTask)tasks[k];
					anyExhaustibleTasks |= tasksArray[k] is ResetInputsTask;
					anyExhaustibleTasks |= tasksArray[k] is TaskSequence;
				}
			}
		}

		/**
		* @return Returns the parallel.
		*/
		public bool isParallel() {
			return parallel;
		}

		/**
		* @return Returns the repetitions.
		*/
		public int getRepetitions() {
			return repetitions;
		}

		public void setRunTime(double sec) {
			runTimeSec = sec;
			fixedTime = true;
		}

		/**
		* @param repetitions The repetitions to set.
		* @throws Exception 
		*/
		public void setRepetitions(int repetitions) {
			fixedTime = false;
			this.repetitions = repetitions;
			if (repetitions==REPEAT_EXHAUST) {
				if (isParallel()) {
					throw new Exception("REPEAT_EXHAUST is not allowed for parallel tasks");
				}
				if (getRunData().getConfig().get("content.source.forever",true)) {
					throw new Exception("REPEAT_EXHAUST requires setting content.source.forever=false");
				}
			}
			setSequenceName();
		}

		/**
		* @return Returns the parent.
		*/
		public TaskSequence getParent() {
			return parent;
		}

		/*
		* (non-Javadoc)
		* @see Lucene.Benchmark.ByTask.Tasks.PerfTask#doLogic()
		*/
		public override int doLogic() {
			exhausted = resetExhausted = false;
			return ( parallel ? doParallelTasks() : doSerialTasks());
		}

		public int doSerialTasks() {
			if (rate > 0) {
				return doSerialTasksWithRate();
			}
    
			initTasksArray();
			int count = 0;

			DateTime t0 = DateTime.Now;

			TimeSpan runTime = TimeSpan.FromMilliseconds(runTimeSec*1000);

			for (int k=0; fixedTime || (repetitions==REPEAT_EXHAUST && !exhausted) || k<repetitions; k++) {
				for(int l=0;l<tasksArray.Length;l++)
					try {
						PerfTask task = tasksArray[l];
						count += task.runAndMaybeStats(letChildReport);
						if (anyExhaustibleTasks)
							updateExhausted(task);
					} catch (NoMoreDataException e) {
						exhausted = true;
					}
				if (fixedTime && DateTime.Now-t0 > runTime) {
					repetitions = k+1;
					break;
				}
			}
			return count;
		}

		public int doSerialTasksWithRate() {
			initTasksArray();
			TimeSpan delayStep = TimeSpan.FromMilliseconds((perMin ? 60000 : 1000) /rate);
			DateTime nextStartTime = DateTime.Now;
			int count = 0;
			for (int k=0; (repetitions==REPEAT_EXHAUST && !exhausted) || k<repetitions; k++) {
				for (int l=0;l<tasksArray.Length;l++) {
					PerfTask task = tasksArray[l];
					TimeSpan waitMore = nextStartTime - DateTime.Now;
					if (waitMore.TotalMilliseconds > 0) {
						//Console.WriteLine("wait: "+waitMore+" for rate: "+ratePerMin+" (delayStep="+delayStep+")");
						Thread.Sleep(waitMore);
					}
					nextStartTime += delayStep; // this aims at avarage rate. 
					try {
						count += task.runAndMaybeStats(letChildReport);
						if (anyExhaustibleTasks)
						updateExhausted(task);
					} catch (NoMoreDataException e) {
						exhausted = true;
					}
				}
			}
			return count;
		}

		// update state regarding exhaustion.
		public void updateExhausted(PerfTask task) {
			if (task is ResetInputsTask) {
				exhausted = false;
				resetExhausted = true;
			} else if (task is TaskSequence) {
				TaskSequence t = (TaskSequence) task;
				if (t.resetExhausted) {
					exhausted = false;
					resetExhausted = true;
					t.resetExhausted = false;
				} else {
					exhausted |= t.exhausted;
				}
			}
		}

		public int doParallelTasks()
		{
			initTasksArray();
			int[] count = {0};
			Thread[] t = null;
			
			// prepare threads
			int indx = 0;
			t = new Thread[ repetitions*tasksArray.Length ];
			for( int k = 0; k < repetitions; k++ )
			{
				for (int i = 0; i < tasksArray.Length; i++)
				{
					PerfTask task = (PerfTask) tasksArray[i].clone();
					RunWithThread rwt = new RunWithThread(task, count, this);
					t[indx++] = new Thread(new ThreadStart(rwt.Run));
				}
			}
			// run threads
			startThreads( t );
			// wait for all threads to complete
			for( int i = 0; i < t.Length; i++ ) {
				t[ i ].Join();
			}
			// return total count
			return count[0];
		}

		// We don't have anonymized thread constructors in C#, so
		// we'll create a little class here that allows us to pass the necessary
		// parameters

		public class RunWithThread
		{
			private readonly PerfTask task;
			private readonly int[] count;
			private readonly TaskSequence ts;

			public RunWithThread (PerfTask task, int[] count, TaskSequence ts)
			{
				this.task = task;
				this.count = count;
				this.ts = ts;
			}

			public void Run() {
				try {
					int n = task.runAndMaybeStats( ts.letChildReport );
					if( ts.anyExhaustibleTasks )
						ts.updateExhausted( task );

					lock( count ) {
						count[ 0 ] += n;
					}
				}
				catch( NoMoreDataException e ) {
					ts.exhausted = true;
				}
				catch( Exception e ) {
					throw new ApplicationException( "Run in PerfTask has encountered an exception", e );
				}
			}
		}
		

		// run threads
		public void startThreads(Thread[] t) {
			if (rate > 0) {
				startlThreadsWithRate(t);
				return;
			}
			for (int i = 0; i < t.Length; i++) {
				t[i].Start();
			}
		}

		// run threads with rate
		public void startlThreadsWithRate(Thread[] t) {
			TimeSpan delayStep = TimeSpan.FromMilliseconds((perMin ? 60000 : 1000) /rate);
			DateTime nextStartTime = DateTime.Now;
			for (int i = 0; i < t.Length; i++) {
				TimeSpan waitMore = nextStartTime - DateTime.Now;
				if (waitMore.TotalMilliseconds > 0) {
					//Console.WriteLine("thread wait: "+waitMore+" for rate: "+ratePerMin+" (delayStep="+delayStep+")");
					Thread.Sleep(waitMore);
				}
				nextStartTime += delayStep; // this aims at average rate of starting threads. 
				t[i].Start();
			}
			//  throws InterruptedException
		}

		public void addTask(PerfTask task) {
			tasks.Add(task);
			task.setDepth(getDepth()+1);
		}
  
		/* (non-Javadoc)
		* @see java.lang.Object#toString()
		*/
		public override String toString() {
			String padd = getPadding();
			StringBuilder sb = new StringBuilder(base.toString());
			sb.Append(parallel ? " [" : " {");
			sb.Append(NEW_LINE);
			foreach (PerfTask task in tasks)
			{
				sb.Append(task.toString());
				sb.Append(NEW_LINE);
			}
			sb.Append(padd);
			sb.Append(!letChildReport ? ">" : (parallel ? "]" : "}"));
			if (fixedTime) {
				sb.Append(" " + String.Format("0:0.0000", runTimeSec) + "s");
			} else if (repetitions>1) {
				sb.Append(" * " + repetitions);
			} else if (repetitions==REPEAT_EXHAUST) {
				sb.Append(" * EXHAUST");
			}
			if (rate>0) {
				sb.Append(",  rate: " + rate+"/"+(perMin?"min":"sec"));
			}
			return sb.ToString();
		}

		/**
		* Execute child tasks in a way that they do not report their time separately.
		*/
		public void setNoChildReport() {
			letChildReport  = false;
			foreach (PerfTask task in tasks)
			{
				if (task is TaskSequence)
				{
					((TaskSequence)task).setNoChildReport();
				}
			}
		}

		/**
		* Returns the rate per minute: how many operations should be performed in a minute.
		* If 0 this has no effect.
		* @return the rate per min: how many operations should be performed in a minute.
		*/
		public int getRate() {
			return (perMin ? rate : 60*rate);
		}

		/**
		* @param rate The rate to set.
		*/
		public void setRate(int rate, bool perMin) {
			this.rate = rate;
			this.perMin = perMin;
			setSequenceName();
		}

		public void setSequenceName() {
			seqName = base.getName();
			if (repetitions==REPEAT_EXHAUST) {
				seqName += "_Exhaust";
			} else if (repetitions>1) {
				seqName += "_"+repetitions;
			}
			if (rate>0) {
				seqName += "_" + rate + (perMin?"/min":"/sec"); 
			}
			if (parallel && seqName.ToLower().IndexOf("par")<0) {
				seqName += "_Par";
			}
		}

		public override String getName() {
			return seqName; // override to include more info 
		}

		/**
		* @return Returns the tasks.
		*/
		public ArrayList getTasks() {
			return tasks;
		}

		/* (non-Javadoc)
		* @see java.lang.Object#clone()
		*/
		public override Object clone() {
			TaskSequence res = (TaskSequence) base.clone();
			res.tasks = new ArrayList();
			for (int i = 0; i < tasks.Count; i++) {
				res.tasks.Add(((PerfTask)tasks[i]).clone());
			}
			return res;
		}

		/**
		* Return true if can be collapsed in case it is outermost sequence
		*/
		public bool isCollapsable() {
			return collapsable;
		}
	}
}
