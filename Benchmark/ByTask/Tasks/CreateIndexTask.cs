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
using System.IO;
using System.Reflection;
using Lucene.Net.Benchmark.ByTask.Utils;
using Lucene.Net.Index;


namespace Lucene.Net.Benchmark.ByTask.Tasks {

	/**
	 * Create an index. <br>
	 * Other side effects: index writer object in perfRunData is set. <br>
	 * Relevant properties: <code>merge.factor, max.buffered,
	 *  max.field.length, ram.flush.mb [default 0], autocommit
	 *  [default true]</code>.
	 * <p>
	 * This task also supports a "writer.info.stream" property with the following
	 * values:
	 * <ul>
	 * <li>SystemOut - sets {@link IndexWriter#setInfoStream(java.io.PrintStream)}
	 * to {@link System#out}.
	 * <li>SystemErr - sets {@link IndexWriter#setInfoStream(java.io.PrintStream)}
	 * to {@link System#err}.
	 * <li>&lt;file_name&gt; - attempts to create a file given that name and sets
	 * {@link IndexWriter#setInfoStream(java.io.PrintStream)} to that file. If this
	 * denotes an invalid file name, or some error occurs, an exception will be
	 * thrown.
	 * </ul>
	 */
	public class CreateIndexTask : PerfTask {
		
		public CreateIndexTask(PerfRunData runData) : base(runData) {
			// do nothing

		}

		public static void setIndexWriterConfig(IndexWriter writer, Config config) {

			String mergeScheduler = config.get("merge.scheduler", "Lucene.Net.Index.ConcurrentMergeScheduler");
			// TODO: assembly stuff
			Assembly assembly = Assembly.LoadFrom("Lucene.Net.dll");

			try
			{
				
				writer.SetMergeScheduler((MergeScheduler) assembly.CreateInstance(mergeScheduler));
			} catch (Exception e) {
				throw new ApplicationException("unable to instantiate class '" + mergeScheduler + "' as merge scheduler", e);
			}

			String mergePolicy = config.get("merge.policy", "Lucene.Net.Index.LogByteSizeMergePolicy");
			try {
				Type[] IndexWriterType = new Type[] { typeof( IndexWriter ) };
				Type MergePolicyType = assembly.GetType( mergePolicy );
				ConstructorInfo cnstr = MergePolicyType.GetConstructor(IndexWriterType);
				writer.SetMergePolicy((MergePolicy) cnstr.Invoke(new Object[] { writer }));
			} catch (Exception e) {
				throw new Exception("unable to instantiate class '" + mergePolicy + "' as merge policy", e);
			}

			writer.SetUseCompoundFile(config.get("compound",true));
			writer.SetMergeFactor((int)config.get("merge.factor", OpenIndexTask.DEFAULT_MERGE_PFACTOR));
			writer.SetMaxFieldLength((int)config.get("max.field.length",OpenIndexTask.DEFAULT_MAX_FIELD_LENGTH));

			double ramBuffer = config.get("ram.flush.mb",OpenIndexTask.DEFAULT_RAM_FLUSH_MB);
			int maxBuffered = config.get("max.buffered",OpenIndexTask.DEFAULT_MAX_BUFFERED);
			if (maxBuffered == IndexWriter.DISABLE_AUTO_FLUSH) {
				writer.SetRAMBufferSizeMB(ramBuffer);
				writer.SetMaxBufferedDocs(maxBuffered);
			} else {
				writer.SetMaxBufferedDocs(maxBuffered);
				writer.SetRAMBufferSizeMB(ramBuffer);
			}
    
			String infoStreamVal = config.get("writer.info.stream", null);
			if (infoStreamVal != null) {
				if (infoStreamVal.Equals("SystemOut")) {
					System.IO.StreamWriter temp_writer;
					temp_writer = new System.IO.StreamWriter( Console.OpenStandardOutput(), System.Console.Out.Encoding );
					temp_writer.AutoFlush = true;
					writer.SetInfoStream(temp_writer);
				} else if (infoStreamVal.Equals("SystemErr")) {
					System.IO.StreamWriter temp_writer;
					temp_writer = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
					temp_writer.AutoFlush = true;
					writer.SetInfoStream(temp_writer);
				} else {
					String f = new FileInfo( infoStreamVal ).FullName;
					writer.SetInfoStream(new StreamWriter(new FileStream(f, FileMode.Open)));
				}
			}

		}
  
		public static IndexDeletionPolicy getIndexDeletionPolicy(Config config) {
			String deletionPolicyName = config.get("deletion.policy", "Lucene.Net.Index.KeepOnlyLastCommitDeletionPolicy");
			IndexDeletionPolicy indexDeletionPolicy = null;
			ApplicationException err = null;
			try {
				Assembly assembly = Assembly.LoadFrom( "Lucene.Net.dll" );
				indexDeletionPolicy = ((IndexDeletionPolicy) assembly.CreateInstance(deletionPolicyName));
			} catch (System.Security.SecurityException iae) {
				err = new ApplicationException("unable to instantiate class '" + deletionPolicyName + "' as IndexDeletionPolicy");
				//err.(iae);
			} catch (ArgumentException ie) {
				err = new ApplicationException("unable to instantiate class '" + deletionPolicyName + "' as IndexDeletionPolicy");
				//err.initCause(ie);
			} catch (FileNotFoundException cnfe) {
				err = new ApplicationException("unable to load class '" + deletionPolicyName + "' as IndexDeletionPolicy");
				//err.initCause(cnfe);
			}
			if (err != null)
				throw err;
			return indexDeletionPolicy;
		}
  
		public override int doLogic() {
			PerfRunData runData = getRunData();
			Config config = runData.getConfig();
    
			IndexDeletionPolicy indexDeletionPolicy = getIndexDeletionPolicy(config);
    
			IndexWriter writer = new IndexWriter(runData.getDirectory(),
													runData.getConfig().get("autocommit", OpenIndexTask.DEFAULT_AUTO_COMMIT),
													runData.getAnalyzer(),
													true, indexDeletionPolicy);
			setIndexWriterConfig(writer, config);
			runData.setIndexWriter(writer);
			return 1;
		}
	}
}
