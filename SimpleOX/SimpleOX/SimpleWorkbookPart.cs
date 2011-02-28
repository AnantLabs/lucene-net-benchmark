using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using DocumentFormat.OpenXml.Packaging;
using Ap = DocumentFormat.OpenXml.ExtendedProperties;
using Vt = DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;

namespace SimpleOX.Excel {

	public class SimpleWorkbookPart {

		public SimpleWorkbookPart() {
		}

		public void Generate (Hashtable Titles, WorkbookPart workbookPart ) {
			Workbook workbook1 = new Workbook();
			workbook1.AddNamespaceDeclaration( "r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships" );
			FileVersion fileVersion1 = new FileVersion() { ApplicationName = "xl", LastEdited = "4", LowestEdited = "4", BuildVersion = "4506" };
			WorkbookProperties workbookProperties1 = new WorkbookProperties() { DefaultThemeVersion = (UInt32Value)124226U };

			BookViews bookViews1 = new BookViews();
			WorkbookView workbookView1 = new WorkbookView() { XWindow = 0, YWindow = 90, WindowWidth = (UInt32Value)28755U, WindowHeight = (UInt32Value)12585U };

			bookViews1.Append( workbookView1 );

			Sheets sheets1 = new Sheets();

			UInt32Value sheetIndex = 1U;
			foreach( int key in Titles.Keys ) {
				Sheet sheet = new Sheet() { Name = (String)Titles[ key ], SheetId = sheetIndex, Id = "rId" + key.ToString() }; // create each sheet with it's title and a unique id
				sheets1.Append( sheet ); // then append the sheets
				sheetIndex++;
			}
			//Sheet sheet1 = new Sheet() { Name = "Log", SheetId = (UInt32Value)1U, Id = "rId1" };
			//Sheet sheet2 = new Sheet() { Name = "Sheet1", SheetId = (UInt32Value)2U, Id = "rId5" }; 

			//sheets1.Append( sheet1 );
			
			CalculationProperties calculationProperties1 = new CalculationProperties() { CalculationId = (UInt32Value)125725U };

			workbook1.Append( fileVersion1 );
			workbook1.Append( workbookProperties1 );
			workbook1.Append( bookViews1 );
			workbook1.Append( sheets1 );
			workbook1.Append( calculationProperties1 );

			workbookPart.Workbook = workbook1;
		}
	}
}
