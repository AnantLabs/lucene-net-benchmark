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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Kajabity.Tools.Java;
using Lucene.Net.Benchmark.ByTask.Utils;
using Lucene.Net.Documents;

namespace Lucene.Net.Benchmark.ByTask.Feeds {

	/**
	 * Creates {@link Document} objects. Uses a {@link ContentSource} to generate
	 * {@link DocData} objects. Supports the following parameters:
	 * <ul>
	 * <li><b>content.source</b> - specifies the {@link ContentSource} class to use
	 * (default <b>SingleDocSource</b>).
	 * <li><b>doc.stored</b> - specifies whether fields should be stored (default
	 * <b>false</b>).
	 * <li><b>doc.tokenized</b> - specifies whether fields should be tokenized
	 * (default <b>true</b>).
	 * <li><b>doc.tokenized.norms</b> - specifies whether norms should be stored in
	 * the index or not. (default <b>false</b>).
	 * <li><b>doc.body.tokenized.norms</b> - specifies whether norms should be
	 * stored in the index for the body field. This can be set to true, while
	 * <code>doc.tokenized.norms</code> is set to false, to allow norms storing just
	 * for the body field. (default <b>true</b>).
	 * <li><b>doc.term.vector</b> - specifies whether term vectors should be stored
	 * for fields (default <b>false</b>).
	 * <li><b>doc.term.vector.positions</b> - specifies whether term vectors should
	 * be stored with positions (default <b>false</b>).
	 * <li><b>doc.term.vector.offsets</b> - specifies whether term vectors should be
	 * stored with offsets (default <b>false</b>).
	 * <li><b>doc.store.body.bytes</b> - specifies whether to store the raw bytes of
	 * the document's content in the document (default <b>false</b>).
	 * <li><b>doc.reuse.fields</b> - specifies whether Field and Document objects
	 * should be reused (default <b>true</b>).
	 * <li><b>doc.index.props</b> - specifies whether the properties returned by
	 * <li><b>doc.random.id.limit</b> - if specified, docs will be assigned random
	 * IDs from 0 to this limit.  This is useful with UpdateDoc
	 * for testing performance of IndexWriter.updateDocument.
	 * {@link DocData#getProps()} will be indexed. (default <b>false</b>).
	 * </ul>
	 */
	public class DocMaker {

		 public class LeftOver {
			internal DocData docdata;
			internal int cnt;
		}

		protected Random r;
		protected int updateDocIDLimit;

		public class DocState {
    
			protected Hashtable fields;
			protected bool reuseFields;
			internal Document doc;
			internal DocData docData = new DocData();

			protected DocState(){
				// do nothing
			}

			public DocState(bool reuseFields, Field.Store store, Field.Index index, Field.Index bodyIndex, Field.TermVector termVector) {

				this.reuseFields = reuseFields;
      
				if (reuseFields) {
					fields =  new Hashtable();
        
					// Initialize the map with the default fields.
					fields.Add(BODY_FIELD, new Field(BODY_FIELD, "", store, bodyIndex, termVector));
					fields.Add(TITLE_FIELD, new Field(TITLE_FIELD, "", store, index, termVector));
					fields.Add(DATE_FIELD, new Field(DATE_FIELD, "", store, index, termVector));
					fields.Add(ID_FIELD, new Field(ID_FIELD, "", Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
					fields.Add(NAME_FIELD, new Field(NAME_FIELD, "", store, index, termVector));
        
					doc = new Document();
				} else {
					fields = null;
					doc = null;
				}
			}

			/**
				* Returns a field corresponding to the field name. If
				* <code>reuseFields</code> was set to true, then it attempts to reuse a
				* Field instance. If such a field does not exist, it creates a new one.
				*/
			internal Field getField(String name, Field.Store store, Field.Index index, Field.TermVector termVector) {
				if (!reuseFields) {
				return new Field(name, "", store, index, termVector);
				}
      
				Field f = (Field) fields[name];
				if (f == null) {
				f = new Field(name, "", store, index, termVector);
				fields.Add(name, f);
				}
				return f;
			}
		}
  
		protected int numDocsCreated = 0;
		protected bool storeBytes = false;

		// leftovers are thread local, because it is unsafe to share residues between threads
		[ThreadStatic]
		public static LeftOver leftovr;

		[ThreadStatic]
		protected static DocState ds;

		public const String BODY_FIELD = "body";
		public const String TITLE_FIELD = "doctitle";
		public const String DATE_FIELD = "docdate";
		public const String ID_FIELD = "docid";
		public const String BYTES_FIELD = "bytes";
		public const String NAME_FIELD = "docname";

		private Config config;

		protected Field.Store storeVal = Field.Store.NO;
		protected Field.Index indexVal = Field.Index.ANALYZED_NO_NORMS;
		protected Field.Index bodyIndexVal = Field.Index.ANALYZED;
		protected Field.TermVector termVecVal = Field.TermVector.NO;
  
		protected ContentSource source;
		protected bool reuseFields;
		protected bool indexProperties;
  
		private int lastPrintedNumUniqueTexts = 0;

		private long lastPrintedNumUniqueBytes = 0;

		private int printNum = 0;

		// create a doc
		// use only part of the body, modify it to keep the rest (or use all if size==0).
		// reset the docdata properties so they are not added more than once.
		public Document createDocument(DocData docData, int size, int cnt) {

			ds = getDocState();
			Document doc = reuseFields ? ds.doc : new Document();
			doc.GetFields().Clear();
    
			// Set ID_FIELD
			Field idField = ds.getField(ID_FIELD, storeVal, Field.Index.NOT_ANALYZED_NO_NORMS, termVecVal);
			idField.SetValue("doc" + (r != null ? r.Next(updateDocIDLimit) : incrNumDocsCreated()));
			doc.Add(idField);
    
			// Set NAME_FIELD
			String name = docData.getName();
			if (name == null) name = "";
			name = cnt < 0 ? name : name + "_" + cnt;
			Field nameField = ds.getField(NAME_FIELD, storeVal, indexVal, termVecVal);
			nameField.SetValue(name);
			doc.Add(nameField);
    
			// Set DATE_FIELD
			DateTime date = docData.getDate();
			//if (date == null) {
			//	date = "";
			//}
			Field dateField = ds.getField(DATE_FIELD, storeVal, indexVal, termVecVal);
			dateField.SetValue(date.ToString());
			doc.Add(dateField);
    
			// Set TITLE_FIELD
			String title = docData.getTitle();
			Field titleField = ds.getField(TITLE_FIELD, storeVal, indexVal, termVecVal);
			titleField.SetValue(title == null ? "" : title);
			doc.Add(titleField);
    
			String body = docData.getBody();
			if (body != null && body.Length > 0) {
				String bdy;
				if (size <= 0 || size >= body.Length) {
					bdy = body; // use all
					docData.setBody(""); // nothing left
				} else {
					// attempt not to break words - if whitespace found within next 20 chars...
					for (int n = size - 1; n < size + 20 && n < body.Length; n++) {
						if (Char.IsWhiteSpace(body.ElementAt<char>(n))) {
							size = n;
							break;
						}
					}
					bdy = body.Substring(0, size); // use part
					docData.setBody(body.Substring(size)); // some left
				}
				Field bodyField = ds.getField(BODY_FIELD, storeVal, bodyIndexVal, termVecVal);
				bodyField.SetValue(bdy);
				doc.Add(bodyField);
      
				if (storeBytes) {
					Field bytesField = ds.getField(BYTES_FIELD, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);
					byte[] byteArray = Encoding.UTF8.GetBytes( bdy );
					bytesField.SetValue( byteArray );
					doc.Add(bytesField);
				}
			}

			if (indexProperties) {
				JavaProperties props = docData.getProps();
				if (props != null) {
					foreach (string key in props.Keys)
					{
						Field f = ds.getField(key, storeVal, indexVal, termVecVal);
						f.SetValue(props.GetProperty(key));
						doc.Add(f);
					}
					docData.setProps(null);
				}
			}
    
			//System.out.println("============== Created doc "+numDocsCreated+" :\n"+doc+"\n==========");
			return doc;

			//  throws UnsupportedEncodingException 
		}

		public void resetLeftovers() {
			leftovr = null;
		}

		public virtual DocState getDocState() {
			if (ds == null) {
				ds = new DocState(true, storeVal, indexVal, bodyIndexVal, termVecVal);
			}
			return ds;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected int incrNumDocsCreated() {
			return numDocsCreated++;
		}

		/**
		* Closes the {@link DocMaker}. The base implementation closes the
		* {@link ContentSource}, and it can be overridden to do more work (but make
		* sure to call super.close()).
		*/
		public void close() {
			source.close();
		}
  
		/**
		* Returns the number of bytes generated by the content source since last
		* reset.
		*/
		[MethodImpl(MethodImplOptions.Synchronized)]
		public long getBytesCount() {
			return source.getBytesCount();
		}

		/**
		* Returns the total number of bytes that were generated by the content source
		* defined to that doc maker.
		*/ 
		public long getTotalBytesCount() {
			return source.getTotalBytesCount();
		}

		/**
		* Creates a {@link Document} object ready for indexing. This method uses the
		* {@link ContentSource} to get the next document from the source, and creates
		* a {@link Document} object from the returned fields. If
		* <code>reuseFields</code> was set to true, it will reuse {@link Document}
		* and {@link Field} instances.
		*/
		public virtual Document makeDocument() {
			resetLeftovers();
			DocData docData = source.getNextDocData(getDocState().docData);
			Document doc = createDocument(docData, 0, -1);
			return doc;
		}

		/**
		* Same as {@link #makeDocument()}, only this method creates a document of the
		* given size input by <code>size</code>.
		*/
		public Document makeDocument(int size) {
			LeftOver lvr = leftovr;
			if (lvr == null || lvr.docdata == null || lvr.docdata.getBody() == null
				|| lvr.docdata.getBody().Length == 0) {
				resetLeftovers();
			}
			DocData docData = getDocState().docData;
			DocData dd = (lvr == null ? source.getNextDocData(docData) : lvr.docdata);
			int cnt = (lvr == null ? 0 : lvr.cnt);
			while (dd.getBody() == null || dd.getBody().Length < size) {
				DocData dd2 = dd;
				dd = source.getNextDocData(new DocData());
				cnt = 0;
				dd.setBody(dd2.getBody() + dd.getBody());
			}
			Document doc = createDocument(dd, size, cnt);
			if (dd.getBody() == null || dd.getBody().Length == 0) {
				resetLeftovers();
			} else {
				if (lvr == null) {
				lvr = new LeftOver();
				leftovr = lvr;
				}
				lvr.docdata = dd;
				lvr.cnt = ++cnt;
			}
			return doc;
		}
  
		public void printDocStatistics() {
			bool print = false;
			String col = "                  ";
			StringBuilder sb = new StringBuilder();
			String newline = Environment.NewLine;
			sb.Append("------------> ").Append(Format.simpleName(this.GetType())).Append(" statistics (").Append(printNum).Append("): ").Append(newline);
			int nut = source.getTotalDocsCount();
			if (nut > lastPrintedNumUniqueTexts) {
				print = true;
				sb.Append("total count of unique texts: ").Append(Format.format(0,nut,col)).Append(newline);
				lastPrintedNumUniqueTexts = nut;
			}
			long nub = getTotalBytesCount();
			if (nub > lastPrintedNumUniqueBytes) {
				print = true;
				sb.Append("total bytes of unique texts: ").Append(Format.format(0,nub,col)).Append(newline);
				lastPrintedNumUniqueBytes = nub;
			}
			if (source.getDocsCount() > 0) {
				print = true;
				sb.Append("num docs added since last inputs reset:   ").Append(Format.format(0,source.getDocsCount(),col)).Append(newline);
				sb.Append("total bytes added since last inputs reset: ").Append(Format.format(0,getBytesCount(),col)).Append(newline);
			}
			if (print) {
				Benchmark.LogSheet.AddRowsAndCellsSplit( sb.Append( newline ).ToString() );
				Console.WriteLine(sb.Append(newline).ToString());
				printNum++;
			}
		}
  
		/** Reset inputs so that the test run would behave, input wise, as if it just started. */
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void resetInputs() {
			printDocStatistics();
			// re-initiate since properties by round may have changed.
			setConfig(config);
			source.resetInputs();
			numDocsCreated = 0;
			resetLeftovers();
		}
  
		/** Set the configuration parameters of this doc maker. */
		public virtual void setConfig(Config config) {
			this.config = config;
			try
			{
				String sourceClass = config.get("content.source", "Lucene.Net.Benchmark.ByTask.Feeds.SingleDocSource");
				Assembly assembly = Assembly.LoadFrom("Benchmark.exe");
				source = (ContentSource)assembly.CreateInstance( sourceClass );
				source.setConfig(config);
			} catch (Exception e) {
				// Should not get here. Throw runtime exception.
				throw new ApplicationException("Unknown error in DocMaker.", e );
			}

			bool stored = config.get("doc.stored", false);
			bool tokenized = config.get("doc.tokenized", true);
			bool norms = config.get("doc.tokenized.norms", false);
			bool bodyNorms = config.get("doc.body.tokenized.norms", true);
			bool termVec = config.get("doc.term.vector", false);
			storeVal = (stored ? Field.Store.YES : Field.Store.NO);
			if (tokenized) {
				indexVal = norms ? Field.Index.ANALYZED : Field.Index.ANALYZED_NO_NORMS;
				bodyIndexVal = bodyNorms ? Field.Index.ANALYZED : Field.Index.ANALYZED_NO_NORMS;
			} else {
				indexVal = norms ? Field.Index.NOT_ANALYZED : Field.Index.NOT_ANALYZED_NO_NORMS;
				bodyIndexVal = bodyNorms ? Field.Index.NOT_ANALYZED : Field.Index.NOT_ANALYZED_NO_NORMS;
			}
			bool termVecPositions = config.get("doc.term.vector.positions", false);
			bool termVecOffsets = config.get("doc.term.vector.offsets", false);
			if (termVecPositions && termVecOffsets) {
				termVecVal = Field.TermVector.WITH_POSITIONS_OFFSETS;
			} else if (termVecPositions) {
				termVecVal = Field.TermVector.WITH_POSITIONS;
			} else if (termVecOffsets) {
				termVecVal = Field.TermVector.WITH_OFFSETS;
			} else if (termVec) {
				termVecVal = Field.TermVector.YES;
			} else {
				termVecVal = Field.TermVector.NO;
			}
			storeBytes = config.get("doc.store.body.bytes", false);
    
			reuseFields = config.get("doc.reuse.fields", true);

			// In a multi-rounds run, it is important to reset DocState since settings
			// of fields may change between rounds, and this is the only way to reset
			// the cache of all threads.
			// was docState new thread
			ds = null;

			indexProperties = config.get("doc.index.props", false);

			updateDocIDLimit = config.get("doc.random.id.limit", -1);
			if (updateDocIDLimit != -1) {
				r = new Random(179);
			}
		}
	}
}
