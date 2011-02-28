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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Lucene.Net.Benchmark.ByTask.Utils;
using Org.System.Xml.Sax;
using Org.System.Xml.Sax.Helpers;

// TODO: Converting this is going to require me to break out some XML reader code and 
// do some major surgery on this class.  Since It isn't immediately neccessary for 
// snapstream, I am going to put off porting this file.
namespace Lucene.Net.Benchmark.ByTask.Feeds {


	public class EnwikiContentSource : ContentSource {

		private class Parser {
		    private Thread t;
			private bool threadDone;
		    private String[] tuple;
		    private NoMoreDataException nmde;
		    private StringBuilder contents = new StringBuilder();
		    private String title;
		    private String body;
		    private String _time;
		    private String id;
			private InputSource<FileStream> input = null;
    
		    public String[] next() {
				if( t == null ) {
					threadDone = false;
					t = new Thread(new  ThreadStart( this.run ) );
					t.Start();
				}

				String[] result;
				lock( this ) {
					while( tuple == null && nmde == null && !threadDone ) {
						try {
							Monitor.Wait(this);
						}
						catch( ThreadInterruptedException ie ) {
						}
					}
					if( nmde != null ) {
						// Set to null so we will re-start thread in case
						// we are re-used:
						t = null;
						throw nmde;
					}
					if( t != null && threadDone ) {
						// The thread has exited yet did not hit end of
						// data, so this means it hit an exception.  We
						// throw NoMorDataException here to force
						// benchmark to stop the current alg:
						throw new NoMoreDataException();
					}
					result = tuple;
					tuple = null;
					Monitor.Pulse(this);
				}
				return result;
		    }

			String time( String original ) {
				StringBuilder buffer = new StringBuilder();

				buffer.Append( original.Substring( 8, 10 ) );
				buffer.Append( '-' );
				buffer.Append( months[ int.Parse( original.Substring( 5, 7 ) ) - 1 ] );
				buffer.Append( '-' );
				buffer.Append( original.Substring( 0, 4 ) );
				buffer.Append( ' ' );
				buffer.Append( original.Substring( 11, 19 ) );
				buffer.Append( ".000" );

				return buffer.ToString();
			}

			public void characters( char[] ch, int start, int length ) {
				contents.Append( ch, start, length );
			}

			public void endElement(String name, String simple, String qualified) {
			  int elemType = getElementType(qualified);
			  switch (elemType) {
			    case PAGE:
			      // the body must be null and we either are keeping image docs or the
			      // title does not start with Image:
			      if (body != null && (keepImages || !title.StartsWith("Image:"))) {
			        String[] tmpTuple = new String[LENGTH];
			        tmpTuple[TITLE] = title.Replace('\t', ' ');
			        tmpTuple[DATE] = _time.Replace('\t', ' ');
			        tmpTuple[BODY] = body.Replace("[\t\n]", " ");
			        tmpTuple[ID] = id;
			        lock(this) {
			          while (tuple != null) {
			            try {
			              Monitor.Wait(this);
			            } catch (ThreadInterruptedException ie) {
			            }
			          }
			          tuple = tmpTuple;
			          Monitor.Pulse(this);
			        }
			      }
			      break;
			    case BODY:
			      body = contents.ToString();
			      //workaround that startswith doesn't have an ignore case option, get at least 20 chars.
			      String startsWith = body.Substring(0, Math.Min(10, contents.Length)).ToLower();
			      if (startsWith.StartsWith("#redirect")) {
			        body = null;
			      }
			      break;
			    case DATE:
			      _time = time(contents.ToString());
			      break;
			    case TITLE:
			      title = contents.ToString();
			      break;
			    case ID:
			      id = contents.ToString();
			      break;
			    default:
			      // this element should be discarded.
				  break;
			  }
			}

		    public void run() {

		      try {
		        IXmlReader reader = SaxReaderFactory.CreateReader(null);
				reader.ContentHandler = (IContentHandler)new Parser();
				reader.ErrorHandler = (IErrorHandler)new Parser();
		        while(true){
					InputSource<FileStream> localFileIS = input;
		          try {
		            reader.Parse(input);
		          } catch (IOException ioe) {
		            lock(this) {
		              if (localFileIS != input) {
		                // fileIS was closed on us, so, just fall
		                // through
		              } else
		                // Exception is real
		                throw ioe;
		            }
		          }
		          lock(this) {
		            if (!forever) {
		              nmde = new NoMoreDataException();
		              Monitor.Pulse(this);
		              return;
		            } else if (localFileIS == input) {
		              // If file is not already re-opened then re-open it now
					  input = new InputSource<FileStream>( new FileStream( _file.FullName, FileMode.Open, FileAccess.Read ) );
					  input .SystemId = input.Source.Name;
		            }
		          }
		        }
		      } catch (SaxException sae) {
		        throw new ApplicationException("SAX Exception caught in EnwikiContentSource", sae);
		      } catch (IOException ioe) {
		        throw new ApplicationException("Io Exception caught in EnwikiContentSource", ioe);
		      } finally {
		        lock(this) {
		          threadDone = true;
		          Monitor.Pulse(this);
		        }
		      }
		    }

		    public void startElement(String name, String simple, String qualified,
		                             AttributesImpl attributes) {
		      int elemType = getElementType(qualified);
		      switch (elemType) {
		        case PAGE:
		          title = null;
		          body = null;
		          _time = null;
		          id = null;
		          break;
		        // intentional fall-through.
		        case BODY:
		        case DATE:
		        case TITLE:
		        case ID:
		          contents.Clear();
		          break;
		        default:
		          // this element should be discarded.
				  break;
		      }
		    }
		  } // end class

		  private static Dictionary<String, int> ELEMENTS = new Dictionary<String, int>();
		  private const int TITLE = 0;
		  private const int DATE = TITLE + 1;
		  private const int BODY = DATE + 1;
		  private const int ID = BODY + 1;
		  private const int LENGTH = ID + 1;
		  // LENGTH is used as the size of the tuple, so whatever constants we need that
		  // should not be part of the tuple, we should define them after LENGTH.
		  private const int PAGE = LENGTH + 1;

		  private static readonly String[] months = {"JAN", "FEB", "MAR", "APR",
										  "MAY", "JUN", "JUL", "AUG",
										  "SEP", "OCT", "NOV", "DEC"};
		  private static FileInfo _file;
		  private static bool keepImages = true;
		  public static FileStream fs;
		  private Parser parser = new Parser();

		 public EnwikiContentSource () : base() {
			ELEMENTS.Add("page", PAGE);
			ELEMENTS.Add("text", BODY);
			ELEMENTS.Add("timestamp", DATE);
			ELEMENTS.Add("title", TITLE);
			ELEMENTS.Add("id", ID);
		 }

  
		  /**
		   * Returns the type of the element if defined, otherwise returns -1. This
		   * method is useful in startElement and endElement, by not needing to compare
		   * the element qualified name over and over.
		   */
		  private static int getElementType(String elem) {
			int val = (int) ELEMENTS[elem];
			return val == null ? -1 : val;
		  }
  
		  public override void close() {
			lock (this) {
			  if (fs != null) {
				fs.Close();
				fs = null;
			  }
			}
		  }
  
		  [MethodImpl(MethodImplOptions.Synchronized)]
		  public override DocData getNextDocData(DocData docData) {
			String[] tuple = parser.next();
			docData.clear();
			docData.setName(tuple[ID]);
			docData.setBody(tuple[BODY]);
			docData.setDate(tuple[DATE]);
			docData.setTitle(tuple[TITLE]);
			return docData;
		  }

		  public override void resetInputs() {
			base.resetInputs();
			fs = (FileStream)getInputStream(_file);
		  }
  
		  public override void setConfig(Config config) {
			base.setConfig(config);
			keepImages = config.get("keep.image.only.docs", true);
			String fileName = config.get("docs.file", null);
			if (fileName == null) {
			  throw new ArgumentException("docs.file must be set");
			}
			_file = new FileInfo(fileName);
		  }
	}
}
