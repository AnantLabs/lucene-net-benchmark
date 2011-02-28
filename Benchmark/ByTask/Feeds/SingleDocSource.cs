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
using System.Runtime.CompilerServices;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * Creates the same document each time {@link #getNextDocData(DocData)} is called.
	 */
	public class SingleDocSource : ContentSource {

		private int docID = 0;

			 public const String DOC_TEXT =  
				"Well, this is just some plain text we use for creating the " +
				"test documents. It used to be a text from an online collection " +
				"devoted to first aid, but if there was there an (online) lawyers " +
				"first aid collection with legal advices, \"it\" might have quite " +
				"probably advised one not to include \"it\"'s text or the text of " +
				"any other online collection in one's code, unless one has money " +
				"that one don't need and one is happy to donate for lawyers " +
				"charity. Anyhow at some point, rechecking the usage of this text, " +
				"it became uncertain that this text is free to use, because " +
				"the web site in the disclaimer of he eBook containing that text " +
				"was not responding anymore, and at the same time, in projGut, " +
				"searching for first aid no longer found that eBook as well. " +
				"So here we are, with a perhaps much less interesting " +
				"text for the test, but oh much much safer. ";
  
			  // return a new docid
			[MethodImpl(MethodImplOptions.Synchronized)]
			  public int newdocid() {
				if (docID > 0 && !forever) {
				  throw new NoMoreDataException();
				}
				return docID++;
			  }

			  public override void close() {}
  
			  public override DocData getNextDocData(DocData docData) {
				int id = newdocid();
				addBytes(DOC_TEXT.Length);
				docData.clear();
				docData.setName("doc" + id);
				docData.setBody(DOC_TEXT);
				return docData;
			  }

				[MethodImpl(MethodImplOptions.Synchronized)]
			  public override void resetInputs() {
				base.resetInputs();
				docID = 0;
			  }

	}
}
