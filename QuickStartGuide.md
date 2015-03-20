# Quick Start Guide #

## Introduction ##

If you haven't read the [Installation Instructions](https://code.google.com/p/lucene-net-benchmark/wiki/InstallationInstructions) first, do that.  This assumes you've gotten the Lucene.Net Benchmark to compile already.  This guide will then walk you through unpacking the Reuter's Corpus and running your first tests.

## Unpacking the Reuter's Copora ##

Included with the source you downloaded for the Benchmark tool, you find a folder called "Corpora" in the root directory.  This is a GZip file of the standard Reuter's corpus as described in the docs.  The file is `reuters21578.tar.gz`, and you can unzip it using whatever your favorite tool is.

The GZip will expand to a number of .sgm files.  Now start up the ExtractReuters program.  It will first ask you for the path to the folder that contains the .sgm files, and then for the path to the output folder you want the processed files to be stored in.  (**note:** It will delete any files in the output directory and create it if it doesn't exist.)  It will probably take a while for it to extract all the files, so just wait until it's done.

## Testing the Algorithm Files ##

In the root directory of the source you'll find a folder called "conf".  It contains the algorithm files used by the Benchmark tool to define the tests.  You can find a good explanation of how these algorithm files work in the [Java Docs](http://lucene.apache.org/java/2_2_0/api/org/apache/lucene/benchmark/byTask/package-summary.html) for the Java version of the tool.  The .NET version of the tool tries to do some smart translation so that it can read the original versions of the algorithm files without changing the namespaces.

Open the "analyzer.alg" file in the conf folder using WordPad (or some similar program).  Edit the value of the "doc.dir" so that it points to the location of your Reuters document directory (the one created by the ExtractReuters tool).  It should look something like...

`doc.dir=C:\\\\docs\\reuters`

... Notice that you have to escape every backslash in the ".alg" files.

You will then need to feed the location of the alogrithm file to Benchmark.NET.  If you run the tool from the command line, you can just pass the location as a path like this...

`Benchmark.exe C:\\conf\analyzer.alg`

... Or if you want to run it from within Visual Studio, just add the path as a command line argument in the project properties on the Debug tab in the window labeled "Command Line Arguments".

Go ahead and run the tool.  You will see output about the test in the command window.  When it is complete it will print out...

`####################`

&lt;BR&gt;


`###  D O N E !!! ###`

&lt;BR&gt;


`####################`

... Once it is done, look in the directory where Benchmark ran from (probably Debug if your running from Visual Studio) and you will find a file called "Stats.xlsx".  This is an Excel workbook that contains a spreadsheet logging every step the algorithm took, a spreadsheet showing the properties used, and a spreadsheet for each of the report summaries you defined in your algorithm.

You can look inside of the "work" directory defined in the algorithm file (it should be located in the directory where Benchmark ran if you didn't change the file).  Inside of it should be a directory called "index" where your index was stored.

This is useful for opening up your index in a tool like [Luke](http://code.google.com/p/luke/) to examine your index.  I often save screenshots from Luke after running a test.

Go back to the [Java Docs](http://lucene.apache.org/java/2_2_0/api/org/apache/lucene/benchmark/byTask/package-summary.html) for more info.  And look at the "Lucene.Net.Benchmark.ByTask.Feeds.DirContentSource.cs" for a way to load in your own documents to the Benchmark tool.