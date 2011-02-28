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

namespace Lucene.Net.Benchmark.ByTask.Utils {

	/**
	 * File utilities.
	 */
	public class FileUtils {
		/**
	   * Delete files and directories, even if non-empty.
	   *
	   * @param dir file or directory
	   * @return true on success, false if no or part of files have been deleted
	   * @throws java.io.IOException
	   */
	  public static bool fullyDelete(DirectoryInfo dir) {
		if (dir == null || !dir.Exists) return false;
		FileInfo[] contents = dir.GetFiles();
		if (contents != null)
		{
			for (int i = 0; i < contents.Length; i++)
			{
				try
				{
					contents[i].Delete();
				} catch(Exception e)
				{
					System.IO.StreamWriter errWriter;
					errWriter = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
					errWriter.AutoFlush = true;
					errWriter.WriteLine("Hey: FileUtils threw an exception: " + e.Message);
					// TODO: Obviously, I'm preventing the false from being returned by throwing an exception
					// and I'd rather use the exception handling than the current bool implementation.
					// but for now I don't want to delete it until I decide to either not rethrow the 
					// error or to stick with the bool checks in place.
					throw new Exception("Hey: FileUtils threw an exception", e);
					return false;
				}
			}
		}

		DirectoryInfo[] directories = dir.GetDirectories();
		if( directories != null ) {
			for( int i = 0; i < directories.Length; i++ ) {
				try {
					fullyDelete( directories[ i ] );
				}
				catch( Exception e ) {
					System.IO.StreamWriter errWriter;
					errWriter = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
					errWriter.AutoFlush = true;
					errWriter.WriteLine( "Hey: FileUtils threw an exception: " + e.Message );
					throw new Exception( "Hey: FileUtils threw an exception", e );
					return false;
				}
			}
		}

	  	try
	  	{
			dir.Delete();
	  	}
	  	catch (Exception e)
	  	{
			System.IO.StreamWriter errWriter;
			errWriter = new System.IO.StreamWriter( Console.OpenStandardError(), System.Console.Error.Encoding );
			errWriter.AutoFlush = true;
			errWriter.WriteLine( "Hey: File Utils threw an expection" + e.Message );
	  		throw new Exception("Hey: File Utils threw an expection", e);
			return false;
	  	}

		return true;
	  }// end of method
	} // end of class
} // end of namespace
