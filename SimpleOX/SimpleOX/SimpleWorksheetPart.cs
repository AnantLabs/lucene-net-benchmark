using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using Ap = DocumentFormat.OpenXml.ExtendedProperties;
using Vt = DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;

namespace SimpleOX.Excel {

	public class SimpleWorksheetPart {

		public SimpleWorksheetPart() {
		}

		public void Generate(DataSheet Sheet, WorksheetPart worksheetPart1 ) {
			Worksheet worksheet1 = new Worksheet();
			worksheet1.AddNamespaceDeclaration( "r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships" );
			SheetDimension sheetDimension1 = new SheetDimension() { Reference = "A1" + ":" + ((Char)('A' + Sheet.FarthestCol -1)).ToString() + "4" };

			SheetViews sheetViews1 = new SheetViews();

			SheetView sheetView1 = new SheetView() { TabSelected = true, WorkbookViewId = (UInt32Value)0U };
			Selection selection1 = new Selection() { ActiveCell = "A8", SequenceOfReferences = new ListValue<StringValue>() { InnerText = "A8" } };

			sheetView1.Append( selection1 );

			sheetViews1.Append( sheetView1 );
			SheetFormatProperties sheetFormatProperties1 = new SheetFormatProperties() { DefaultRowHeight = 15D };

			Columns columns1 = new Columns();
			Column column1 = new Column() { Min = (UInt32Value)1U, Max = (UInt32Value)1U, Width = 109.28515625D, Style = (UInt32Value)1U, CustomWidth = true };

			columns1.Append( column1 );

			SheetData sheetData1 = new SheetData();
			for( UInt32Value i = 1; i < (Sheet.RowIndex + 1); i++ ) {
				Row row = new Row() { RowIndex = i, Spans = new ListValue<StringValue>() { InnerText = "1:" + Sheet.FarthestCol.ToString() } };
				for( Char x = 'A'; x < ('A' + 26); x++ ) {
					if( Sheet.Cols[ x.ToString() + (i).ToString() ] != null ) {
						Cell cell;
						CellValue cellValue;
						if( Sheet.Cols[ x.ToString() + i.ToString() ] is String ) {
							cell = new Cell() { CellReference = x.ToString() + ( i ).ToString(), StyleIndex = (UInt32Value)1U, DataType = CellValues.SharedString };
							cellValue = new CellValue();
							cellValue.Text = (String)Sheet.Cols[ x.ToString() + i.ToString() ];
						}
						else if( Sheet.Cols[ x.ToString() + i.ToString() ] is int || Sheet.Cols[ x.ToString() + i.ToString() ] is float ) {
							cell = new Cell() { CellReference = x.ToString() + ( i ).ToString(), StyleIndex = (UInt32Value)1U, DataType = CellValues.Number };
							cellValue = new CellValue();
							cellValue.Text = (String)Sheet.Cols[ x.ToString() + i.ToString() ].ToString();
						}
						else {
							cell = new Cell() { CellReference = x.ToString() + ( i ).ToString(), StyleIndex = (UInt32Value)1U, DataType = CellValues.Number };
							cellValue = new CellValue();
							cellValue.Text = "";
						}
						cell.Append( cellValue );
						row.Append( cell );
					}
				}
				sheetData1.Append( row );
			}
			//IEnumerator sharedEnumerator = _sharedStrings.GetEnumerator();
			//int sharedStringIndex = 0;
			//UInt32Value rowIndex = 1;
			//while( sharedEnumerator.MoveNext() ) {
			//    Row row = new Row() { RowIndex = rowIndex, Spans = new ListValue<StringValue>() { InnerText = "1:" + Sheet.FarthestCol.ToString() } };
			//    Cell cell = new Cell() { CellReference = "A" + rowIndex.ToString(), StyleIndex = (UInt32Value)1U, DataType = CellValues.SharedString };
			//    Cell cell2 = new Cell();
			//    Cell cell3 = new Cell();
			//    CellValue cellValue = new CellValue();
			//    cellValue.Text = sharedStringIndex.ToString();
			//    cell.Append( cellValue );
			//    row.Append( cell );
			//    row.Append( cell2 );
			//    row.Append( cell3 );
			//    sheetData1.Append( row );
			//    sharedStringIndex++;
			//    rowIndex++;
			//}

			PageMargins pageMargins1 = new PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D };
			PageSetup pageSetup1 = new PageSetup() { Orientation = OrientationValues.Portrait, HorizontalDpi = (UInt32Value)0U, VerticalDpi = (UInt32Value)0U, Id = "rId1" };

			worksheet1.Append( sheetDimension1 );
			worksheet1.Append( sheetViews1 );
			worksheet1.Append( sheetFormatProperties1 );
			worksheet1.Append( columns1 );
			worksheet1.Append( sheetData1 );
			worksheet1.Append( pageMargins1 );
			worksheet1.Append( pageSetup1 );

			worksheetPart1.Worksheet = worksheet1;
		
		}

	}
}
