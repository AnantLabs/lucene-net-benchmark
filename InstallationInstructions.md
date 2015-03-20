# Lucene.Net Benchmark Tool Installation from Code Instructions #

## Get The Code ##

You can download the current version of the code via [Mercurial SCM](http://mercurial.selenic.com/downloads/) from the [Source tab](https://code.google.com/p/lucene-net-benchmark/source/checkout).  Or you can downloaded a zip version of the latest code on the [download](https://code.google.com/p/lucene-net-benchmark/downloads/list) page.

You'll also need to download some additional libraries and SDK's to be able to compile the code, but I will cover those as we go along.  (**note:** There is no binary distributable of the tool as it is assumed that Lucene.Net users will want to extend and customize their version of the tool to include code from their implementations of Lucene.Net.)

You will need a copy of [Visual Studio 2010](http://www.microsoft.com/visualstudio/en-us/products/2010-editions) to open and compile the code.  The FREE [Visual C# 2010 Express](http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-csharp-express) will work.

## Compiling "Simple OX" ##

After you have checked out the code or unzipped the archive, look for a directory in the root called "SimpleOX".  Inside this folder is a solution file for the SimpleOX dll; which benchmark uses for creating Excel spreadsheets of your tests.

In order to compile this project, you will need the Open XML SDK 2.0.  You can download the file  [OpenXMLSDKv2.msi](http://www.microsoft.com/downloads/en/details.aspx?FamilyId=C6E744E5-36E9-45F5-8D8C-331DF206E0D0&displaylang=en) from Microsoft.  (**note:** you do not need to download the OpenXMLSDKTool.msi file on the page.)

After you have installed the SDK, open the "SimpleOX.sln" file in Visual Studio.  The project will probably not be able to find the `DocumentFormat.OpenXml` reference.  You'll need to update that by finding the DLL under the ".NET" tab in add references.

Go ahead and build the dll.  We'll reference it later.

## Compiling RTools ##

Inside the root directory of the source you will find a directory called `RTools.utl`.  This project is based on the [RTools](http://www.codeproject.com/KB/string/rtoolsutil.aspx) found on Code Project and originally written by Ryan Seghers.  It provides support for the Java style StreamTokenizer; which is used to read the benchmark algorithm files.  However, I had to do some surgery to the tools to allow it to work with benchmark.  So, the updated project is provided here.

Open the `RTools.Util.sln` file in Visual Studio 2010.  go ahead and build this file; we'll reference the DLL later on.

## Get Kajabity Tools ##

Download the [Kajabity Tools](http://www.kajabity.com/Tools/Kajabity%20Tools-0.1.zip) from [Kajabity.com](http://www.kajabity.com/index.php/kajabity-tools/) and unzip the files.  In the root directory of the project you will find the `Kajabity Tools.dll`; we'll reference that in a minute.

## Get SAX for .NET ##

Download [SAX for .NET](http://sourceforge.net/projects/saxdotnet/files/saxdotnet/saxdotnet%202.0/sax_dotnet_2_0.zip/download) and unzip the file.  We'll reference the `sax.dll` file in the bin directory of this zip file in just a minute.

## Compile Lucene.Net DLL's ##

I won't go into details about compiling the and getting the Lucene.Net source code since you have presumably already done that and that is why you now want to use the benchmark tool.  You will need to compile the Lucene.Net demo as well as the Highlighter.Net and FastVectorHighlighter.Net DLL's found in the contrib directory of the Lucene.Net Source

## Compiling Benchmark ##

Now open the `Benchmark.sln` file found in the root directory of the source in VS2010.

You will see that the solution is broken up into three projects: Benchmark, ExtractReuters and ExtractWikipedia.  (**note:** as of this writing the ExtractWikipedia project is not functional.)  Open the references section for each one and update any references that need to be.  Build, and you should be able to run the Benchmark and ExtractReuters tools.  Look at the docs on usage for more information on how to do that.