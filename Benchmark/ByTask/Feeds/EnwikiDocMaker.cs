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
using Lucene.Net.Benchmark.ByTask.Utils;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * A {@link DocMaker} which reads the English Wikipedia dump. Uses
	 * {@link EnwikiContentSource} as its content source, regardless if a different
	 * content source was defined in the configuration.
	 * @deprecated Please use {@link DocMaker} instead, with content.source=EnwikiContentSource
	 */
	public class EnwikiDocMaker : DocMaker {

		System.IO.StreamWriter outWriter;

		public EnwikiDocMaker () : base()
		{
			outWriter = new System.IO.StreamWriter( Console.OpenStandardOutput(), System.Console.Out.Encoding );
			outWriter.AutoFlush = true;
		}

		public override void setConfig(Config config) {
			base.setConfig(config);
			// Override whatever content source was set in the config
			source = new EnwikiContentSource();
			source.setConfig(config);
			outWriter.WriteLine("NOTE: EnwikiDocMaker is deprecated; please use DocMaker instead (which is the default if you don't specify doc.maker) with content.source=EnwikiContentSource");
		  }
	}
}
