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
using RTools.Util;

// TODO: Need to look into either using the Lucene tokenizer or writing
// my own rather than using the RTools so it can be removed from
// the distribution

namespace Lucene.Net.Benchmark.ByTask.Utils {

	/// <summary>
	/// This is just a decorator class that adds a new method called
	/// PeekNextChar which allows you to see what the next char in 
	/// the stream will be.  This is needed for the algorithm class
	/// which sometimes needs to see what the next character will
	/// be in order to determine if it is finished with a logical loop.
	/// </summary>
	public class StreamTokenizerWithPeek : StreamTokenizer {

		public StreamTokenizerWithPeek()
			: base() {
			// do nothing
		}

		public StreamTokenizerWithPeek( TextReader sr )
			: base( sr ) {
			// do nothing
		}

		public StreamTokenizerWithPeek( String str )
			: base( str ) {
			// do nothing
		}

		public int PeekNextChar() {
			int c = 10;
			Char Peek;
			// consume from backString if possible
			if( backString.Length > 0 ) {
				c = backString[ 0 ];
				return ( c );
			}

			if( textReader == null )
				return ( Eof );


			Peek = (Char)textReader.Peek();

			
			// Check to see if nextTokenSb [ 0 ] is a non-whitespace character and return it if it is
			if( !Settings.IsCharType( nextTokenSb[ 0 ], CharTypeBits.Whitespace ) || ( nextTokenSb[0] == 10 ) ) {
			    return nextTokenSb[ 0 ];
			}
			else {
				
				// So, let's peek at the stream and see if we're lucky enough to get a non whitespace character
				if( !Settings.IsCharType( Peek, CharTypeBits.Whitespace ) || ( Peek == 10 ) ) {
					return Peek;
				}
				else {
					// unfortunately our next real char is farther on down the line, so we'll start looking fo rit
					do {
						try {
							//backString.Append( nextTokenSb[ 0 ] );
							// we'll get the next character
							nextTokenSb.Append( (Char)textReader.Read() );
							// then we'll peek again
							Peek = (Char)textReader.Peek();
						}
						catch( Exception ) {
							return ( Eof );
						}
					} while( Settings.IsCharType( Peek, CharTypeBits.Whitespace ) && ( Peek != 10 ) );
					return Peek;
				}
				
			}

			

			//if( c == 13 ) {
			//    backString.Append( Peek );
			//}

			//try {
			//    c = Peek;
			//}
			//catch( Exception ) {
			//    return ( Eof );
			//}

			//return ( c );
		}
	}
}
