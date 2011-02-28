using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleOX.Excel {
	class Program {
		static void Main( string[] args ) {
			Document doc = new Document();
			int sheet1Index = doc.CreateDataSheet("Log");
			DataSheet sheet = (DataSheet)doc.Sheets[ sheet1Index ];
			sheet.StartRow();
			sheet.AddCell( "my string" );
			sheet.AddCell( "second column" );
			sheet.AddCell( "rate" );
			sheet.AddCell( "docs" );
			sheet.AddCell( "totalmem" );
			sheet.StartRow();
			sheet.AddCell( "my string 2" );
			sheet.StartRow();
			sheet.AddCell( "Mary" );
			int sheet2Index = doc.CreateDataSheet("Bob");
			DataSheet sheet2 = (DataSheet)doc.Sheets[ sheet2Index ];
			sheet2.StartRow();
			sheet2.AddCell( "blah" );
			sheet2.StartRow();
			sheet2.AddCell( "blah 2" );
			doc.SaveDocument( "logworkbook2.xlsx" );
			LogWorkbook lwb = new LogWorkbook();
			lwb.addSharedString( "A1", "my string" );
			lwb.addSharedString( "A2", "my string 2" );
			lwb.CreatePackage( "logworkbook.xlsx" );
			Console.ReadKey();
		}
	}
}
