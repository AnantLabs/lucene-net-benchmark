// StreamTokenizerTestCase.cs
// 
// Copyright (C) 2002-2004 Ryan Seghers
//
// This software is provided AS IS. No warranty is granted, 
// neither expressed nor implied. USE THIS SOFTWARE AT YOUR OWN RISK.
// NO REPRESENTATION OF MERCHANTABILITY or FITNESS FOR ANY 
// PURPOSE is given.
//
// License to use this software is limited by the following terms:
// 1) This code may be used in any program, including programs developed
//    for commercial purposes, provided that this notice is included verbatim.
//    
// Also, in return for using this code, please attempt to make your fixes and
// updates available in some way, such as by sending your updates to the
// author.

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace RTools.Util
{
	/// <summary>
	/// This represents a test case for the StreamTokenizer.
	/// You instantiate one, set it up with a test string and
	/// some expected (truth) tokens, and Run() it to see if you get the
	/// expected Tokens.
	/// This has some static methods to run a large set of built-in
	/// tests.
	/// </summary>
	public class StreamTokenizerTestCase
	{
		// ---------------------------------------------------------------------
		#region Properties
		// ---------------------------------------------------------------------

		private StreamTokenizerSettings tokenizerSettings;

		/// <summary>The tokenizer settings for this test case.</summary>
		public StreamTokenizerSettings TokenizerSettings 
		{ 
			get { return(tokenizerSettings); }
			set { tokenizerSettings = value; } 
		}

		private Logger log;
		/// <summary>The verbosity, used during test case run.</summary>
		public VerbosityLevel Verbosity
		{
			get { return(log.Verbosity); }
			set { log.Verbosity = value; }
		}

		string name;
		/// <summary>The name of this test case.</summary>
		public string Name { get { return(name); }}

		ArrayList truthTokens;
		/// <summary>The "truth", which is a list of Tokens.</summary>
		public ArrayList TruthTokens 
		{
			get { return(truthTokens); }
		}

		/// <summary>The test input string to parse.</summary>
		private string testString;

		/// <summary>The test input string to parse.</summary>
		public string TestString
		{
			get { return(testString); }
			set { testString = value; }
		}

		private bool expectUntermQuote;
		/// <summary>
		/// Whether or not to expect an unterminated quote exception when
		/// this test case is run.
		/// </summary>
		public bool ExpectUntermQuote 
		{ 
			get { return(expectUntermQuote); } 
			set { expectUntermQuote = value; DropEofTruthToken(); }
		}

		private bool expectUntermComment;
		/// <summary>
		/// Whether or not to expect an unterminated comment exception when
		/// this test case is run.
		/// </summary>
		public bool ExpectUntermComment
		{ 
			get { return(expectUntermComment); } 
			set { expectUntermComment = value; DropEofTruthToken(); }
		}

		#endregion

		// ---------------------------------------------------------------------
		#region Constructors/Destructor
		// ---------------------------------------------------------------------

		/// <summary>
		/// Default constructor.
		/// </summary>
		public StreamTokenizerTestCase()
		{
			name = "unset";
			log = new Logger("StreamTokenizerTestCase");
			tokenizerSettings = new StreamTokenizerSettings();
			truthTokens = new ArrayList();
			expectUntermQuote = false;
			expectUntermComment = false;
		}

		/// <summary>
		/// Constructs with arguments.
		/// </summary>
		public StreamTokenizerTestCase(string name, string testString)
		{
			this.name = name;
			this.testString = testString;
			log = new Logger("StreamTokenizerTestCase");
			tokenizerSettings = new StreamTokenizerSettings();
			truthTokens = new ArrayList();
			expectUntermQuote = false;
			expectUntermComment = false;
		}

		#endregion

		// ---------------------------------------------------------------------
		#region Standard Methods
		// ---------------------------------------------------------------------

		/// <summary>
		/// Return a string representation of this object.
		/// </summary>
		/// <returns>The string representation (the name).</returns>
		public override string ToString()
		{
			return(name);
		}

		/// <summary>
		/// Display the state of this object.
		/// </summary>
		public void Display()
		{
			Display("");
		}

		/// <summary>
		/// Display the state of this object, with a per-line prefix.
		/// </summary>
		/// <param name="prefix">The pre-line prefix.</param>
		public void Display(string prefix)
		{
			Console.WriteLine(prefix + "StreamTokenizerTestCase display: {0}", name);
			tokenizerSettings.Display(prefix + "    ");
			Console.WriteLine(prefix + "expectUntermQuote: " + expectUntermQuote);
			Console.WriteLine(prefix + "expectUntermComment: " + expectUntermComment);
		}

		#endregion

		// ---------------------------------------------------------------------
		#region Main Methods
		// ---------------------------------------------------------------------

		/// <summary>
		/// Run this test case using the current settings, truth tokens,
		/// and test string.
		/// There are two bools involved here: the return value is whether
		/// or not we were able to run the test, and the out result is
		/// whether or not the test passed (if it was run).
		/// </summary>
		/// <param name="result">If the test case was run successfully,
		/// whether or not it passed.</param>
		/// <returns>Whether or not we were able to run the test case.
		/// This does not mean the test case passed or failed.</returns>
		public bool Run(out bool result)
		{
			result = true;

			// parse the string
			StreamTokenizer tokenizer = new StreamTokenizer();
			tokenizer.Verbosity = Verbosity;
			tokenizer.Settings.Copy(tokenizerSettings);
			ArrayList resultTokens = new ArrayList();

			//
			// string parse
			//
			try
			{
				if (!tokenizer.TokenizeString(testString, resultTokens))
				{
					log.Error("Run: Unable to parse from string.");
					return(false);
				}

				if (!CompareTokens(truthTokens, resultTokens))
				{
					log.Error("Run: Tokens differ in string parse.");
					result = false;
					return(true);
				}
			}
			catch (StreamTokenizerUntermQuoteException)
			{
				if (!expectUntermQuote) result = false;
			}
			catch (StreamTokenizerUntermCommentException)
			{
				if (!expectUntermComment) result = false;
			}

			//
			// file parse
			//
			resultTokens.Clear();

			// write to file
			string tmpFileName = "stTest.tmp";

			if (!WriteStringToFile(testString, tmpFileName))
			{
				log.Error("Run: Unable to write string to tmp file {0}, can't do file parse test.",
					tmpFileName);
				return(false);
			}

			try
			{
				if (!tokenizer.TokenizeFile(tmpFileName, resultTokens))
				{
					log.Error("Run: Unable to parse from file.");
					return(false);
				}

				if (!CompareTokens(truthTokens, resultTokens))
				{
					log.Error("Run: Tokens differ in file parse.");
					result = false;
				}
			}
			catch (StreamTokenizerUntermQuoteException)
			{
				if (!expectUntermQuote) result = false;
			}
			catch (StreamTokenizerUntermCommentException)
			{
				if (!expectUntermComment) result = false;
			}

			File.Delete(tmpFileName);
			return(true);
		}

		private bool WriteStringToFile(string s, string fileName)
		{
			StreamWriter writer = new StreamWriter(fileName);
			writer.Write(s);
			writer.Close();
			return(true);
		}

		/// <summary>
		/// Utility method to display two ArrayList's of tokens.
		/// </summary>
		/// <param name="truthTokens">The truth tokens.</param>
		/// <param name="resultTokens">The result tokens.</param>
		private void DisplayTokens(ArrayList truthTokens, ArrayList resultTokens)
		{
			// display the tokens side-by-side
			int len = Math.Max(truthTokens.Count, resultTokens.Count);
			Console.WriteLine("Truth            Result            ");
			Console.WriteLine("-----------------------------------------------------------");
			for (int i = 0; i < len; i++)
			{
				if (i < truthTokens.Count) 
				{
					Console.Write("{0}", ((Token)truthTokens[i]).ToLineString());
				}
				else Console.Write("<none>");
				Console.Write("			");
				if (i < resultTokens.Count) 
				{
					Console.Write("{0}", ((Token)resultTokens[i]).ToLineString());
				}
				else Console.Write("<none>");
				Console.WriteLine();
			}
			Console.WriteLine("-----------------------------------------------------------");
		}

		/// <summary>
		/// Compare the two lists of tokens, display mismatches.
		/// This also compares the token's line numbers. Line numbers
		/// are base 1 so 0 is an invalid line number.  If the truth
		/// token's line number is 0, this doesn't compare the line numbers
		/// for that token.
		/// </summary>
		/// <param name="truthTokens">The correct token list.</param>
		/// <param name="resultTokens">The actual token list.</param>
		/// <returns>Whether they matched.</returns>
		private bool CompareTokens(ArrayList truthTokens, ArrayList resultTokens)
		{
			if (truthTokens.Count != resultTokens.Count)
			{
				log.Error("CompareTokens: {0} truth tokens, and {1} result tokens",
					truthTokens.Count, resultTokens.Count);
				DisplayTokens(truthTokens, resultTokens);
				return(false);
			}

			for (int i = 0; i < truthTokens.Count; i++)
			{
				if (!truthTokens[i].Equals(resultTokens[i]))
				{
					log.Warn("CompareTokens: truth {0}   result {1}", truthTokens[i],
						resultTokens[i]);
					log.Error("CompareTokens: Token {0} differs.", i);
					DisplayTokens(truthTokens, resultTokens);
					return(false);
				}
				else if ((((Token)truthTokens[i]).LineNumber > 0)
					&& (((Token)truthTokens[i]).LineNumber != ((Token)resultTokens[i]).LineNumber))
				{
					log.Warn("CompareTokens: truth {0} line: {1}, result {2} line: {3}", 
						truthTokens[i], ((Token)truthTokens[i]).LineNumber, 
						resultTokens[i], ((Token)resultTokens[i]).LineNumber);
					log.Error("CompareTokens: Token {0} line number differs.", i);
					DisplayTokens(truthTokens, resultTokens);
					return(false);
				}
			}
			return(true);
		}

		/// <summary>
		/// Convenient way to add multiple Tokens to the truthTokens.
		/// This makes sure the last token is an EofToken, since that's
		/// always produced by StreamTokenizer.
		/// </summary>
		/// <param name="tokens">The tokens, params Token[].</param>
		public void AddTruthTokens(params Token[] tokens)
		{
			DropEofTruthToken();

			foreach(Token t in tokens) truthTokens.Add(t);

			// automatically add the EofToken
			// if we expect to get to Eof and there isn't already one theref
			if (!(expectUntermQuote || expectUntermComment)) 
			{
				if (!(truthTokens[truthTokens.Count - 1] is EofToken))
					truthTokens.Add(new EofToken());
			}
		}

		/// <summary>
		/// Drop the Eof token.
		/// </summary>
		private void DropEofTruthToken()
		{
			int lastIndex = truthTokens.Count - 1;
			if ((lastIndex >= 0) && (truthTokens[lastIndex] is EofToken))
				truthTokens.RemoveAt(lastIndex);
		}

		#endregion

		// ---------------------------------------------------------------------
		#region Static Test Methods
		// ---------------------------------------------------------------------

		/// <summary>
		/// Run all built-in tests.
		/// </summary>
		/// <param name="v">The VerbosityLevel to run at.</param>
		/// <returns>bool - true for all pass, false for one or more failed.</returns>
		public static bool RunTests(VerbosityLevel v)
		{
			Logger log = new Logger("StreamTokenizerTestCase: RunTests");
			log.Verbosity = v;
			bool overallResult = true;
			ArrayList testCases = new ArrayList();
			log.Info("Starting...");

			// build
			if (!BuildTestCases(testCases))
			{
				log.Error("Unable to build test cases.");
				return(false);
			}

			// run them
			foreach (StreamTokenizerTestCase tc in testCases)
			{
				tc.Verbosity = v;
				log.Debug("Running test case {0}", tc.ToString());
				bool res = true; // just this test

				if (!tc.Run(out res))
				{
					log.Error("Unable to run test case {0}", tc);
					return(false);
				}

				if (res) log.Info("Test case {0} PASSED", tc);
				else log.Info("Test case {0} FAILED", tc);

				overallResult &= res;
			}

			// overall result
			if (overallResult) log.Info("All tests PASSED");
			else log.Info("One or more tests FAILED");
			return(overallResult);
		}

		/// <summary>
		/// Run a built-in test, specified by name.
		/// </summary>
		/// <param name="testName">The name of the test to run.</param>
		/// <param name="v">The VerbosityLevel to run at.</param>
		/// <returns>bool - true for all pass, false for one or more failed.</returns>
		public static bool RunATest(string testName, VerbosityLevel v)
		{
			Logger log = new Logger("StreamTokenizerTestCase: RunATest");
			log.Verbosity = v;
			bool overallResult = true;
			ArrayList testCases = new ArrayList();
			log.Info("Starting...");

			// build (all, don't think this will be a performance problem)
			if (!BuildTestCases(testCases))
			{
				log.Error("Unable to build test cases.");
				return(false);
			}

			// run it
			foreach (StreamTokenizerTestCase tc in testCases)
			{
				if (tc.Name == testName)
				{
					tc.Verbosity = v;
					log.Debug("Running test case {0}", tc.ToString());
					bool res = true; // just this test

					if (!tc.Run(out res))
					{
						log.Error("Unable to run test case {0}", tc);
						return(false);
					}

					if (res) log.Info("Test case '{0}' PASSED", tc);
					else log.Info("Test case '{0}' FAILED", tc);

					overallResult &= res;
				}
			}

			// overall result
			if (overallResult) log.Info("All tests PASSED");
			else log.Info("One or more tests FAILED");
			return(overallResult);
		}

		#endregion

		// ---------------------------------------------------------------------
		#region Built-in Test Cases
		// ---------------------------------------------------------------------

		/// <summary>
		/// Build some hard-coded test cases, put them into the input list.
		/// </summary>
		/// <param name="list">The list to put the test cases into.</param>
		/// <returns>bool - true for success, false for failure.</returns>
		public static bool BuildTestCases(ArrayList list)
		{
			Logger log = new Logger("StreamTokenizerTestCase: BuildTestCases");
			StreamTokenizerTestCase tc;
			log.Info("Starting...");

			// -------------------------------------------------------------
			#region basic test cases
			// -------------------------------------------------------------

			// simple
			tc = new StreamTokenizerTestCase("simple", "simple test");
			tc.AddTruthTokens(new WordToken("simple", 1), new WordToken("test", 1));
			list.Add(tc);

			// simple with grabWhitespace
			tc = new StreamTokenizerTestCase("simple with GWS", "simple test");
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new WordToken("simple"), new WhitespaceToken(" "), new WordToken("test"));
			list.Add(tc);

			// Eol
			tc = new StreamTokenizerTestCase("Eol", "\n\nhello\n\nthere");
			tc.TokenizerSettings.GrabEol = true;
			tc.AddTruthTokens(new EolToken(1), new EolToken(2), 
				new WordToken("hello", 3), 
				new EolToken(3), 
				new EolToken(4), 
				new WordToken("there", 5), new EofToken(5));
			list.Add(tc);

			// Eol after space
			tc = new StreamTokenizerTestCase("Eol after space", "hello \nthere");
			tc.TokenizerSettings.GrabEol = true;
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new WordToken("hello", 1), new WhitespaceToken(" ", 1), 
				new EolToken(1), new WordToken("there", 2), new EofToken(2));
			list.Add(tc);

			// Eol after space 1
			tc = new StreamTokenizerTestCase("Eol after space 1", "hello there \n another");
			tc.TokenizerSettings.GrabEol = true;
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new WordToken("hello", 1), new WhitespaceToken(" ", 1), 
				new WordToken("there", 1), 
				new WhitespaceToken(" ", 1), new EolToken(1), new WhitespaceToken(" ", 2), 
				new WordToken("another", 2), new EofToken(2));
			list.Add(tc);

			// number space Eol
			tc = new StreamTokenizerTestCase("number space Eol", "a 123 \n");
			tc.TokenizerSettings.GrabEol = true;
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new WordToken("a", 1), new WhitespaceToken(" ", 1), 
				new IntToken(123,1), new WhitespaceToken(" ", 1), 
				new EolToken(1), new EofToken(2));
			list.Add(tc);

			// #define
			tc = new StreamTokenizerTestCase("define", "#define MAX 128 \n");
			tc.TokenizerSettings.GrabEol = true;
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new CharToken('#',1), new WordToken("define", 1), 
				new WhitespaceToken(" ", 1), new WordToken("MAX", 1), new WhitespaceToken(" ", 1), 
				new IntToken(128,1), new WhitespaceToken(" ", 1), 
				new EolToken(1), new EofToken(2));
			list.Add(tc);

			// #define no ws
			tc = new StreamTokenizerTestCase("define no ws", "#define MAX 128 \n");
			tc.TokenizerSettings.GrabEol = true;
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new CharToken('#',1), new WordToken("define", 1), 
				new WordToken("MAX", 1),
				new IntToken(128,1),
				new EolToken(1), new EofToken(2));
			list.Add(tc);

			// dashes
			tc = new StreamTokenizerTestCase("dashes", "-hello apple-pie -");
			tc.AddTruthTokens(new CharToken('-'), new WordToken("hello"), 
				new WordToken("apple"), new CharToken('-'),
				new WordToken("pie"), new CharToken('-'));
			list.Add(tc);

			// dashes as word char
			tc = new StreamTokenizerTestCase("dashes as word char", "-hello apple-pie -");
			tc.TokenizerSettings.WordChar('-');
			tc.AddTruthTokens(new WordToken("-hello"), 
				new WordToken("apple-pie"), new WordToken("-"));
			list.Add(tc);

			// single quotes
			tc = new StreamTokenizerTestCase("single quotes", "simple 'quote test'");
			tc.AddTruthTokens(new WordToken("simple"), new QuoteToken("'quote test'"));
			list.Add(tc);

			// escaped single quotes
			tc = new StreamTokenizerTestCase("escaped single quotes", "'simple \\'quote test'");
			tc.AddTruthTokens(new QuoteToken("'simple \\'quote test'"));
			list.Add(tc);

			// escaped single quotes
			tc = new StreamTokenizerTestCase("escaped backslashes single quotes", 
				@"'simple \\'quote");
			tc.AddTruthTokens(new QuoteToken(@"'simple \\'"),
				new WordToken("quote", 1));
			list.Add(tc);

			// quotes
			tc = new StreamTokenizerTestCase("double quotes", "simple \"quote test\"");
			tc.AddTruthTokens(new WordToken("simple"), new QuoteToken("\"quote test\""));
			list.Add(tc);

			// escaped quotes
			tc = new StreamTokenizerTestCase("escaped quotes", "'simple \\\"quote test'");
			tc.AddTruthTokens(new QuoteToken("'simple \\\"quote test'"));
			list.Add(tc);

			// some chars
			tc = new StreamTokenizerTestCase("some chars", ";/&<>");
			tc.AddTruthTokens(new CharToken(';'), new CharToken('/'), new CharToken('&'),
				new CharToken('<'), new CharToken('>'));
			list.Add(tc);

			// trailing ws with grabWhitespace
			tc = new StreamTokenizerTestCase("trailing WS with GWS", "simple test ");
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new WordToken("simple"), new WhitespaceToken(" "), 
				new WordToken("test"), new WordToken(" "));
			list.Add(tc);

			// tab with grabWhitespace
			tc = new StreamTokenizerTestCase("tab with GWS", "simple	test");
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new WordToken("simple"), new WhitespaceToken("	"), 
				new WordToken("test"));
			list.Add(tc);

			// numbers in words
			tc = new StreamTokenizerTestCase("numbers in words", "cool4u");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new WordToken("cool4u"));
			list.Add(tc);

			// dots
			tc = new StreamTokenizerTestCase("dots", "hello. apple.pear.");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new WordToken("hello"), new CharToken('.'),
				new WordToken("apple"), new CharToken('.'), new WordToken("pear"), 
				new CharToken('.'));
			list.Add(tc);

			#endregion

			// -------------------------------------------------------------
			#region comment test cases
			// -------------------------------------------------------------

			// block comment
			tc = new StreamTokenizerTestCase("block comment", 
				"statement; /* block comment */ anotherStatement;");
			tc.TokenizerSettings.SetupForCodeParse();
			tc.AddTruthTokens(new WordToken("statement"), new CharToken(';'), new WhitespaceToken(" "), 
				new CommentToken("/* block comment */"), new WhitespaceToken(" "), 
				new WordToken("anotherStatement"), new CharToken(';'));
			list.Add(tc);

			// no slash-star comments
			tc = new StreamTokenizerTestCase("no slash-star comments", 
				"statement/* block */");
			tc.TokenizerSettings.SetupForCodeParse();
			tc.TokenizerSettings.SlashStarComments = false;
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new WordToken("statement"), new CharToken('/'), new CharToken('*'), 
				new WordToken("block"), new CharToken('*'), new CharToken('/'));
			list.Add(tc);

			// line comment
			tc = new StreamTokenizerTestCase("line comment", 
				"statement # comment");
			tc.TokenizerSettings.SetupForCodeParse();
			tc.TokenizerSettings.CommentChar('#');
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new WordToken("statement"), new CommentToken("# comment"));
			list.Add(tc);

			// slash-slash comment
			tc = new StreamTokenizerTestCase("slash-slash comment", 
				"statement; // line comment\n anotherStatement;");
			tc.TokenizerSettings.SetupForCodeParse();
			tc.AddTruthTokens(new WordToken("statement"), new CharToken(';'), new WhitespaceToken(" "), 
				new CommentToken("// line comment"), new WhitespaceToken("\n "), 
				new WordToken("anotherStatement"), new CharToken(';'));
			list.Add(tc);

			// no slash-slash comments
			tc = new StreamTokenizerTestCase("no slash-slash", 
				"statement; // line");
			tc.TokenizerSettings.SetupForCodeParse();
			tc.TokenizerSettings.SlashSlashComments = false;
			tc.AddTruthTokens(new WordToken("statement"), new CharToken(';'), new WhitespaceToken(" "), 
				new CharToken('/'), new CharToken('/'), new WhitespaceToken(" "), 
				new WordToken("line"));
			list.Add(tc);

			// line comment no WS
			tc = new StreamTokenizerTestCase("line comment no WS", 
				"statement; // line comment\n anotherStatement;");
			tc.TokenizerSettings.SetupForCodeParse();
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new WordToken("statement"), new CharToken(';'),
				new CommentToken("// line comment"),
				new WordToken("anotherStatement"), new CharToken(';'));
			list.Add(tc);

			// line comment no WS no comments
			tc = new StreamTokenizerTestCase("line comment no WS no comments", 
				"statement; // line comment\n anotherStatement;");
			tc.TokenizerSettings.SetupForCodeParse();
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.TokenizerSettings.GrabComments = false;
			tc.AddTruthTokens(new WordToken("statement"), new CharToken(';'),
				new WordToken("anotherStatement"), new CharToken(';'));
			list.Add(tc);

			// slash-slash comment with quote
			tc = new StreamTokenizerTestCase("slash-slash comment with quote", 
				"statement; // line 'comment\n anotherStatement;");
			//tc.TokenizerSettings.SetupForCodeParse();
			tc.TokenizerSettings.SlashSlashComments = true;
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.TokenizerSettings.GrabComments = true;
			tc.AddTruthTokens(new WordToken("statement"), new CharToken(';'), new WhitespaceToken(" "), 
				new CommentToken("// line 'comment"), new WhitespaceToken("\n "), 
				new WordToken("anotherStatement"), new CharToken(';'));
			list.Add(tc);

			// slash-slash comment inside quote
			tc = new StreamTokenizerTestCase("slash-slash comment inside quote", 
				"statement; '// line '\n anotherStatement;");
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new WordToken("statement"), new CharToken(';'), new WhitespaceToken(" "), 
				new QuoteToken("'// line '"), new WhitespaceToken("\n "), 
				new WordToken("anotherStatement"), new CharToken(';'));
			list.Add(tc);

			// slash-slash comment inside double quote
			tc = new StreamTokenizerTestCase("slash-slash comment inside double quote", 
				"statement; \"// line \"\n anotherStatement;");
			tc.TokenizerSettings.GrabWhitespace = true;
			tc.AddTruthTokens(new WordToken("statement"), new CharToken(';'), new WhitespaceToken(" "), 
				new QuoteToken("\"// line \""), new WhitespaceToken("\n "), 
				new WordToken("anotherStatement"), new CharToken(';'));
			list.Add(tc);

			// slash-slash comment line numbers
			tc = new StreamTokenizerTestCase("slash-slash comment line numbers", 
				"\nstatement;\n// line comment\n anotherStatement;");
			tc.TokenizerSettings.SetupForCodeParse();
			tc.AddTruthTokens(new WhitespaceToken("\n", 2), 
				new WordToken("statement", 2), new CharToken(';', 2), 
				new WhitespaceToken("\n", 3), 
				new CommentToken("// line comment", 3), new WhitespaceToken("\n ", 4), 
				new WordToken("anotherStatement", 4), new CharToken(';', 4),
				new EofToken(4));
			list.Add(tc);

			#endregion

			// -------------------------------------------------------------
			#region unterminated token test cases
			// -------------------------------------------------------------

			// unterminated quote
			tc = new StreamTokenizerTestCase("unterminated quote", 
				" start 'quote ");
			tc.ExpectUntermQuote = true;
			tc.TokenizerSettings.SetupForCodeParse();
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.TokenizerSettings.DoUntermCheck = true;
			tc.AddTruthTokens(new WordToken("start"));
			list.Add(tc);

			// unterminated comment
			tc = new StreamTokenizerTestCase("unterminated comment", 
				" start /* comment");
			tc.ExpectUntermComment = true;
			tc.TokenizerSettings.SetupForCodeParse();
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.TokenizerSettings.DoUntermCheck = true;
			tc.AddTruthTokens(new WordToken("start"));
			list.Add(tc);

			#endregion

			// -------------------------------------------------------------
			#region integers test cases
			// -------------------------------------------------------------
			// single-digit integers
			tc = new StreamTokenizerTestCase("single-digit integers", "4 5 6");
			tc.AddTruthTokens(new IntToken(4), new IntToken(5), new IntToken(6));
			list.Add(tc);

			// multi-digit integers
			tc = new StreamTokenizerTestCase("multi-digit integers", "43 54 6234");
			tc.AddTruthTokens(new IntToken(43), new IntToken(54), new IntToken(6234));
			list.Add(tc);

			// negative integers
			tc = new StreamTokenizerTestCase("negative integers", "-4 -54 -643");
			tc.AddTruthTokens(new IntToken(-4), new IntToken(-54), new IntToken(-643));
			list.Add(tc);

			// no parse numbers
			tc = new StreamTokenizerTestCase("no parse numbers", "643");
			tc.TokenizerSettings.ParseNumbers = false;
			tc.TokenizerSettings.WordChars('0', '9');
			tc.AddTruthTokens(new WordToken("643"));
			list.Add(tc);

			// long integers
			tc = new StreamTokenizerTestCase("long integers", "931231232200000000");
			tc.AddTruthTokens(new IntToken(931231232200000000));
			list.Add(tc);

			// too-long integers (roll to float)
			tc = new StreamTokenizerTestCase("float integers", "1000000000000000000000000");
			tc.AddTruthTokens(new FloatToken(1e24));
			list.Add(tc);

			#endregion

			// -------------------------------------------------------------
			#region hex numbers
			// -------------------------------------------------------------

			// hex numbers
			tc = new StreamTokenizerTestCase("hex numbers", 
				"0x000f 0xFFF 0xFFE 0xA 0xABCDEF 0x7 0x0 0xf9");
			tc.TokenizerSettings.ParseHexNumbers = true;
			tc.AddTruthTokens(new IntToken(15), new IntToken(4095), new IntToken(4094),
				new IntToken(10), new IntToken(11259375), new IntToken(7), new IntToken(0),
				new IntToken(249));
			list.Add(tc);

			// tricky hex parse case
			tc = new StreamTokenizerTestCase("NonHex", "Bias");
			tc.TokenizerSettings.ParseHexNumbers = true;
			tc.AddTruthTokens(new WordToken("Bias"));
			list.Add(tc);

			// tricky hex parse case
			tc = new StreamTokenizerTestCase("NonHex1", "0xi");
			tc.TokenizerSettings.ParseHexNumbers = true;
			tc.AddTruthTokens(new IntToken(0), new WordToken("xi"));
			list.Add(tc);

			// tricky hex parse case
			tc = new StreamTokenizerTestCase("NonHex2", "buy0x.");
			tc.TokenizerSettings.ParseHexNumbers = true;
			tc.AddTruthTokens(new WordToken("buy0x"), new CharToken('.'));
			list.Add(tc);

			// not parsing hex
			tc = new StreamTokenizerTestCase("not parse hex", "0x2.");
			tc.TokenizerSettings.ParseHexNumbers = false;
			tc.AddTruthTokens(new IntToken(0), new WordToken("x2"), new CharToken('.'));
			list.Add(tc);

			// int64 hex numbers
			tc = new StreamTokenizerTestCase("int64 hex numbers", 
				"0xffffffffff");
			tc.TokenizerSettings.ParseHexNumbers = true;
			tc.AddTruthTokens(new IntToken((long)1099511627775));
			list.Add(tc);

			#endregion

			// -------------------------------------------------------------
			#region float test cases
			// -------------------------------------------------------------

			// simple floats
			tc = new StreamTokenizerTestCase("simple floats", 
				"1.2 0.01 12.3 932.123");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new FloatToken(1.2), new FloatToken(0.01), 
				new FloatToken(12.3), new FloatToken(932.123));
			list.Add(tc);

			// negative floats
			tc = new StreamTokenizerTestCase("negative floats", 
				"-0.1 -5.42 -64.321");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new FloatToken(-0.1), new FloatToken(-5.42), 
				new FloatToken(-64.321));
			list.Add(tc);

			// exponential floats
			tc = new StreamTokenizerTestCase("exponential floats", 
				"1e10 2e+11 1.3e2 -6.4e3");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new FloatToken(1e10), new FloatToken(2e+11), new FloatToken(1.3e2), 
				new FloatToken(-6.4e3));
			list.Add(tc);

			// comma-separated exponential floats
			tc = new StreamTokenizerTestCase("cs exponential floats", 
				"1e10,2e+11,1.3e2,-6.4e3");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new FloatToken(1e10), new CharToken(','), 
				new FloatToken(2e+11), new CharToken(','), 
				new FloatToken(1.3e2), new CharToken(','), 
				new FloatToken(-6.4e3));
			list.Add(tc);

			// neg exponential floats
			tc = new StreamTokenizerTestCase("neg exponential floats", 
				"1e-10 1.3e-2 -6.4e-3");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new FloatToken(1e-10), new FloatToken(1.3e-2), 
				new FloatToken(-6.4e-3));
			list.Add(tc);

			// mangled float cases
			tc = new StreamTokenizerTestCase("mangled floats", 
				"1e-a");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new IntToken(1), new WordToken("e"), new CharToken('-'),
				new WordToken("a"));
			list.Add(tc);

			// mangled float cases
			tc = new StreamTokenizerTestCase("mangled floats 2", 
				"1e 10e++4 1e+ 1e++");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new IntToken(1), new WordToken("e"), new IntToken(10), new WordToken("e"), 
				new CharToken('+'), new CharToken('+'), new IntToken(4),
				new IntToken(1), new WordToken("e"), new CharToken('+'),
				new IntToken(1), new WordToken("e"), new CharToken('+'), new CharToken('+')
				);
			list.Add(tc);

			// long exponential floats
			tc = new StreamTokenizerTestCase("long exponential floats", 
				"1.08406880000000000000e+002+i*2.77673530595170400000e+002");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new FloatToken(1.08406880000000000000e+002), 
				new CharToken('+'), new WordToken("i"), new CharToken('*'),
				new FloatToken(2.77673530595170400000e+002));
			list.Add(tc);

			// no digit before period
			tc = new StreamTokenizerTestCase("no digit before period", 
				".25 hello .123");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new FloatToken(0.25), 
				new WordToken("hello"),
				new FloatToken(.123));
			list.Add(tc);

			#endregion

			// -------------------------------------------------------------
			#region line number test cases
			// -------------------------------------------------------------

			// basic line number test case
			tc = new StreamTokenizerTestCase("basic line numbers", 
				"hello\nworld\n");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new WordToken("hello", 1), new WordToken("world", 2),
				new EofToken(3));
			list.Add(tc);

			// line numbers 2: starting newline, eof on same line as last token
			tc = new StreamTokenizerTestCase("line numbers 2", 
				"\na\nb");
			tc.AddTruthTokens(new WordToken("a", 2), new WordToken("b", 3),
				new EofToken(3));
			list.Add(tc);

			// line numbers 3: ending newline
			tc = new StreamTokenizerTestCase("line numbers 3", "\na\n");
			tc.AddTruthTokens(new WordToken("a", 2), new EofToken(3));
			list.Add(tc);

			// line numbers 4: floats and line numbers
			tc = new StreamTokenizerTestCase("line numbers 4", "1.12\n2.3\n4.5");
			tc.AddTruthTokens(new FloatToken(1.12, 1), new FloatToken(2.3, 2), 
				new FloatToken(4.5, 3), new EofToken(3));
			list.Add(tc);

			// line numbers 5: floats and line numbers
			tc = new StreamTokenizerTestCase("line numbers 5", "\n1.12\n");
			tc.AddTruthTokens(new FloatToken(1.12, 2), new EofToken(3));
			list.Add(tc);

			#endregion

			// -------------------------------------------------------------
			#region non-Ascii characters
			// -------------------------------------------------------------

			// non-Ascii characters
			tc = new StreamTokenizerTestCase("extended ascii 1", "Cyrille Chépélov");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new WordToken("Cyrille", 1), new WordToken("Chépélov", 1),
				new EofToken(1));
			list.Add(tc);

			// non-Ascii characters
			tc = new StreamTokenizerTestCase("extended ascii 2", "Ą˘ŁĽŚŤŹĽŚŠťź˝ž");
			tc.TokenizerSettings.GrabWhitespace = false;
			tc.AddTruthTokens(new WordToken("Ą˘ŁĽŚŤŹĽŚŠťź˝ž", 1), new EofToken(1));
			list.Add(tc);

			#endregion

			return(true);
		}
		#endregion
	}
}


