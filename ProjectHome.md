Based on the original benchmark tool created by Grant Ingersoll and Andrzej Bialecki, this tool allows users of Lucene.Net to run benchmark tests based on standard test doucments to benchmark installations of Lucene.Net.  The benchmark tool may also be extended to test customizations of Lucene.Net.

This port tries to be a line-by-line port, rather than switching to C# idioms.  This is done on purpose to make it easier to keep the port current moving forward.  You should be able to open a class file in the Java version, and the same class file in the C# version and find the same code very close to the same line numbers.

You can visit the Source or Downloads pages to download the source code.  A great place to start is the at the [InstallationInstructions](InstallationInstructions.md) and the [QuickStartGuide](QuickStartGuide.md).

The current released version is 1.0.0.0.  Which you can find in [Downloads](Downloads.md).  This version is very Alpha, and there are a number of things that currently do not work...

  * he ExtractWikipedia is not functioning
  * he Quality Scorer is not functioning
  * here is nAnt script for building and downloading the appropriate files.
  * arious other bug fixes in the reporting needs to be completed (see the [Issues](Issues.md) page for more details).

... Other than that, this is an exact port of the Java with the only addition being that it saves its reporting data out to an Open XML (2007 and beyond) Excel worksheet for opening in MS Office.

Happy Testing!