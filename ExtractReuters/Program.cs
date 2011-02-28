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
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ExtractReuters {
	class ExtractReuters {

		 private DirectoryInfo reutersDir;
		private DirectoryInfo outputDir;
		private String LINE_SEPARATOR = Environment.NewLine;
		const string EXTRACTION_PATTERN							= @"<TITLE>(.*?)</TITLE>|<DATE>(.*?)</DATE>|<BODY>((.|\n)*?)</BODY>";


		private String[] META_CHARS					= { "&", "<", ">", "\"", "'" };

		private String[] META_CHARS_SERIALIZATION	= { "&amp;", "&lt;", "&gt;", "&quot;", "&apos;" };

		static void Main( string[] args ) {

			if (args.Length == 0) {
				Console.WriteLine("Enter the path to the reuters directory containing segment files...");
				string reutersDirName = Console.ReadLine();
				Console.WriteLine("Enter the path to the directory you reuters documents output to...");
				string outputDirName = Console.ReadLine();
				DirectoryInfo reutersDir = new DirectoryInfo(reutersDirName);
				if (reutersDir.Exists)
				{
					DirectoryInfo outputDir = new DirectoryInfo( outputDirName );
					outputDir.Create();
					ExtractReuters er = new ExtractReuters( reutersDir, outputDir );
					er.extract();
				}
			} else if (args.Length == 1 || args.Length > 2) {
				printUsage();
				Console.WriteLine( "Press any key to close..." );
				Console.ReadKey();
			} else {
				DirectoryInfo reutersDir = new DirectoryInfo( args[ 0 ] );
				if (reutersDir.Exists)
				{
					DirectoryInfo outputDir = new DirectoryInfo( args[1] );
					outputDir.Create();
					ExtractReuters er = new ExtractReuters( reutersDir, outputDir );
					er.extract();

				}
				else
				{
					printUsage();
					Console.WriteLine( "Press any key to close..." );
					Console.ReadKey();
				}
			}
		}

		    public ExtractReuters(DirectoryInfo reutersDir, DirectoryInfo outputDir)
			{
				this.reutersDir = reutersDir;
				this.outputDir = outputDir;
				Console.WriteLine("Deleting all files in " + outputDir.FullName);
				FileInfo [] files = outputDir.GetFiles();
				for (int i = 0; i < files.Length; i++)
				{
					files[i].Delete();
				}

			}

			public void extract()
			{
				FileInfo [] sgmFiles = reutersDir.GetFiles("*.sgm");
				if (sgmFiles != null && sgmFiles.Length > 0)
				{
					for (int i = 0; i < sgmFiles.Length; i++)
					{
						FileInfo sgmFile = sgmFiles[i];
						extractFile(sgmFile);
					}
				}
				else
				{
					StreamWriter tempWriter = new StreamWriter( Console.OpenStandardError() );
					tempWriter.WriteLine("No .sgm files in " + reutersDir);
				}
			}

			

			/**
			 * Override if you wish to change what is extracted
			 *
			 * @param sgmFile
			 */
			protected void extractFile(FileInfo sgmFile)
			{
				try
				{
					TextReader reader = File.OpenText(sgmFile.FullName);
					

					StringBuilder buffer = new StringBuilder(1024);
					StringBuilder outBuffer = new StringBuilder(1024);

					String line = null;
					int index = -1;
					int docNumber = 0;
					while ((line = reader.ReadLine()) != null)
					{
						//when we see a closing reuters tag, flush the file

						if ((index = line.IndexOf("</REUTERS")) == -1)
						{
							//Replace the SGM escape sequences

							buffer.Append(line).Append(' ');//accumulate the strings for now, then apply regular expression to get the pieces,
						}
						else
						{
							//Extract the relevant pieces and write to a file in the output dir
							MatchCollection matcher = Regex.Matches(buffer.ToString(), EXTRACTION_PATTERN, RegexOptions.Singleline);						

								foreach (Match match in matcher)
								{
									for (int i=1; i <= match.Groups.Count; i++)
									{
										if (match.Groups[i].Value != "" && match.Groups[i].Value != ";")
										{
											//Console.WriteLine( i + " " + match.Groups[ i ].Value );
											outBuffer.Append(match.Groups[i].Value);
											outBuffer.Append( LINE_SEPARATOR ).Append( LINE_SEPARATOR );
										}
									}
								}
								
							String outString = outBuffer.ToString();
							for (int i = 0; i < META_CHARS_SERIALIZATION.Length ; i++)
							{
								outString = outString.Replace(META_CHARS_SERIALIZATION[i], META_CHARS[i]);
							}
							TextWriter outFile = File.CreateText(Path.Combine(outputDir.FullName, sgmFile.Name + "-" + (docNumber++) + ".txt"));
							outFile.Write( outString );
							outFile.Close();
							outBuffer.Clear();
							buffer.Clear();
						}
					}
					reader.Close();
				}

				catch (
						IOException e
						)

				{
					throw new ApplicationException("Caught an IO exception in writing out reuter's files", e);
				}
			}


			private static void printUsage()
			{
				Console.WriteLine("Usage: ExtractReuters <Path to Reuters SGM files> <Output Path>");
			}
	}
}
