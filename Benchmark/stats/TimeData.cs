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
using System.Diagnostics;
using System.Management;
using System.Text;

namespace Lucene.Net.Benchmark.stats {

	/**
	 * This class holds a data point measuring speed of processing.
	 *
	 */
	public class TimeData {

		  /** Name of the data point - usually one of a data series with the same name */
		  public String name;
		  /** Number of records processed. */
		  public long count = 0;
		  /** Elapsed time in milliseconds. */
		  public long elapsed = 0L;

		  private DateTime delta = DateTime.Now;
		  /** Free memory at the end of measurement interval. */
		  public long freeMem = 0L;
		  /** Total memory at the end of measurement interval. */
		  public long totalMem = 0L;

		  public TimeData() {
			  // DO NOTHING
		  }

		  public TimeData(String name) {
			this.name = name;
		  }

		  /** Start counting elapsed time. */
		  public void start() {
			delta = DateTime.Now;
		  }

		  /** Stop counting elapsed time. */
		  public void stop() {
			count++;
			elapsed += (DateTime.Now - delta).Milliseconds;
		  }

		  /** Record memory usage. */
		  public void recordMemUsage() {
			  Process process = Process.GetCurrentProcess();
			  // Let's get some system stats from management
			  ObjectQuery winQuery = new ObjectQuery( "SELECT * FROM Win32_ComputerSystem" );
			  long TotalPhysicalMemory = 0;
			  ManagementObjectSearcher searcher = new ManagementObjectSearcher( winQuery );
			  foreach( ManagementObject item in searcher.Get() ) {
				  TotalPhysicalMemory = long.Parse( item[ "TotalPhysicalMemory" ].ToString() );
			  }
				freeMem = process.PrivateMemorySize64;
				totalMem = TotalPhysicalMemory;
		  }

		  /** Reset counters. */
		  public void reset() {
			count = 0;
			elapsed = 0L;
			delta = DateTime.Now;
		  }

		  public Object clone() {
			TimeData td = new TimeData(name);
			td.name = name;
			td.elapsed = elapsed;
			td.count = count;
			td.delta = delta;
			td.freeMem = freeMem;
			td.totalMem = totalMem;
			return td;
		  }

		  /** Get rate of processing, defined as number of processed records per second. */
		  public double getRate() {
			double rps = (double) count * 1000.0 / (double) (elapsed>0 ? elapsed : 1); // assume at least 1ms for any countable op
			return rps;
		  }

		  /** Get a short legend for toString() output. */
		  public static String getLabels() {
			return "# count\telapsed\trec/s\tfreeMem\ttotalMem";
		  }

		  public String toString() { return true.ToString(); }
		  /**
		   * Return a tab-separated string containing this data.
		   * @param withMem if true, append also memory information
		   * @return The String
		   */
		  public String toString(bool withMem) {
			StringBuilder sb = new StringBuilder();
			sb.Append(count + "\t" + elapsed + "\t" + getRate());
			if (withMem) sb.Append("\t" + freeMem + "\t" + totalMem);
			return sb.ToString();
		  }
	}
}
