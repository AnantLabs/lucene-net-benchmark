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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Diagnostics;

using Lucene.Net.Benchmark.ByTask.Tasks;

namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Statistics for a task run. 
	 * <br>The same task can run more than once, but, if that task records statistics, 
	 * each run would create its own TaskStats.
	 */
	public class TaskStats {
		/** task for which data was collected */
		private PerfTask task; 

		/** round in which task run started */
		private int round;

		/** task start time */
		private DateTime start;
  
		/** task elapsed time.  elapsed >= 0 indicates run completion! */
		private TimeSpan elapsed;
  
		/** max tot mem during task */
		private long maxTotMem;
  
		/** max used mem during task */
		private long maxUsedMem;
  
		/** serial run number of this task run in the perf run */
		private int taskRunNum;
  
		/** number of other tasks that started to run while this task was still running */ 
		private int numParallelTasks;
  
		/** number of work items done by this task.
		* For indexing that can be number of docs added.
		* For warming that can be number of scanned items, etc. 
		* For repeating tasks, this is a sum over repetitions.
		*/
		private int count;

		/** Number of similar tasks aggregated into this record.   
		* Used when summing up on few runs/instances of similar tasks.
		*/
		private int numRuns = 1;		
  
		/**
		* Create a run data for a task that is starting now.
		* To be called from Points.
		*/
		public TaskStats (PerfTask task, int taskRunNum, int round) {
			// TODO: I need to do some experiments to see what the most
			// accurate way of determining memory usage is.
			this.task = task;
			this.taskRunNum = taskRunNum;
			this.round = round;
			PerformanceCounter counter = new PerformanceCounter("Memory", "Available Bytes");
			// Let's get some system stats from management
			ObjectQuery winQuery = new ObjectQuery( "SELECT * FROM Win32_ComputerSystem" );
			long TotalPhysicalMemory = 0;
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(winQuery);
			foreach (ManagementObject item in searcher.Get())
			{
				TotalPhysicalMemory = long.Parse( item[ "TotalPhysicalMemory" ].ToString() );
			}
			winQuery = new ObjectQuery( "SELECT * FROM Win32_OperatingSystem" );
			long FreePhysicalMemory = 0;
			long FreeVirtualMemory = 0;
			long TotalVirtualMemorySize = 0;
			long TotalVisibleMemorySize = 0;
			searcher = new ManagementObjectSearcher(winQuery);
			foreach( ManagementObject item in searcher.Get() )
			{
				FreePhysicalMemory = long.Parse(item["FreePhysicalMemory"].ToString());
				FreeVirtualMemory = long.Parse( item[ "FreeVirtualMemory" ].ToString() );
				TotalVirtualMemorySize = long.Parse( item[ "TotalVirtualMemorySize" ].ToString() );
				TotalVisibleMemorySize = long.Parse( item[ "TotalVisibleMemorySize" ].ToString() );
			}
			maxTotMem = TotalPhysicalMemory;
			maxUsedMem = maxTotMem - (long)Convert.ToInt64(counter.NextValue());

			start = DateTime.Now;
		}
  
		/**
		* mark the end of a task
		*/
		public void markEnd (int numParallelTasks, int count) {
			PerformanceCounter counter = new PerformanceCounter("Memory", "Available Bytes");
			elapsed = DateTime.Now  - start;
			long totMem = (long)Convert.ToInt64(counter.NextValue());
			if (totMem > maxTotMem) {
				maxTotMem = totMem;
			}
			long usedMem = totMem - (long)Convert.ToInt64(counter.NextValue());
			if (usedMem > maxUsedMem) {
				maxUsedMem = usedMem;
			}
			this.numParallelTasks = numParallelTasks;
			this.count = count;
		}

		/**
		* @return the taskRunNum.
		*/
		public int getTaskRunNum() {
			return taskRunNum;
		}

		/* (non-Javadoc)
		* @see java.lang.Object#toString()
		*/
		public String toString() {
			StringBuilder res = new StringBuilder(task.getName());
			res.Append(" ");
			res.Append(count);
			res.Append(" ");
			res.Append(elapsed);
			return res.ToString();
		}

		/**
		* @return Returns the count.
		*/
		public int getCount() {
			return count;
		}

		/**
		* @return elapsed time.
		*/
		public TimeSpan getElapsed() {
			return elapsed;
		}

		/**
		* @return Returns the maxTotMem.
		*/
		public long getMaxTotMem() {
			return maxTotMem;
		}

		/**
		* @return Returns the maxUsedMem.
		*/
		public long getMaxUsedMem() {
			return maxUsedMem;
		}

		/**
		* @return Returns the numParallelTasks.
		*/
		public int getNumParallelTasks() {
			return numParallelTasks;
		}

		/**
		* @return Returns the task.
		*/
		public PerfTask getTask() {
			return task;
		}

		/**
		* @return Returns the numRuns.
		*/
		public int getNumRuns() {
			return numRuns;
		}

		/**
		* Add data from another stat, for aggregation
		* @param stat2 the added stat data.
		*/
		public void add(TaskStats stat2) {
			numRuns += stat2.getNumRuns();
			elapsed += stat2.getElapsed();
			maxTotMem += stat2.getMaxTotMem();
			maxUsedMem += stat2.getMaxUsedMem();
			count += stat2.getCount();
			if (round != stat2.round) {
				round = -1; // no meaning if aggregating tasks of different round. 
			}
		}

		/* (non-Javadoc)
		* @see java.lang.Object#clone()
		*/
		public Object clone() {
			return base.MemberwiseClone();
			//  throws CloneNotSupportedException
			throw new NotImplementedException( "There is no clone() method in TaskStats" );
		}

		/**
		* @return the round number.
		*/
		public int getRound() {
			return round;
		}
	}
}
