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

	class SimpleWorkbookStylesPart {

		public SimpleWorkbookStylesPart() {
		}

		public void Generate( WorkbookStylesPart workbookStylesPart ) {

			Stylesheet stylesheet1 = new Stylesheet();

			Fonts fonts1 = new Fonts() { Count = (UInt32Value)1U };

			Font font1 = new Font();
			FontSize fontSize1 = new FontSize() { Val = 11D };
			Color color1 = new Color() { Theme = (UInt32Value)1U };
			FontName fontName1 = new FontName() { Val = "Calibri" };
			FontFamilyNumbering fontFamilyNumbering1 = new FontFamilyNumbering() { Val = 2 };
			FontScheme fontScheme1 = new FontScheme() { Val = FontSchemeValues.Minor };

			font1.Append( fontSize1 );
			font1.Append( color1 );
			font1.Append( fontName1 );
			font1.Append( fontFamilyNumbering1 );
			font1.Append( fontScheme1 );

			fonts1.Append( font1 );

			Fills fills1 = new Fills() { Count = (UInt32Value)2U };

			Fill fill1 = new Fill();
			PatternFill patternFill1 = new PatternFill() { PatternType = PatternValues.None };

			fill1.Append( patternFill1 );

			Fill fill2 = new Fill();
			PatternFill patternFill2 = new PatternFill() { PatternType = PatternValues.Gray125 };

			fill2.Append( patternFill2 );

			fills1.Append( fill1 );
			fills1.Append( fill2 );

			Borders borders1 = new Borders() { Count = (UInt32Value)1U };

			Border border1 = new Border();
			LeftBorder leftBorder1 = new LeftBorder();
			RightBorder rightBorder1 = new RightBorder();
			TopBorder topBorder1 = new TopBorder();
			BottomBorder bottomBorder1 = new BottomBorder();
			DiagonalBorder diagonalBorder1 = new DiagonalBorder();

			border1.Append( leftBorder1 );
			border1.Append( rightBorder1 );
			border1.Append( topBorder1 );
			border1.Append( bottomBorder1 );
			border1.Append( diagonalBorder1 );

			borders1.Append( border1 );

			CellStyleFormats cellStyleFormats1 = new CellStyleFormats() { Count = (UInt32Value)1U };
			CellFormat cellFormat1 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U };

			cellStyleFormats1.Append( cellFormat1 );

			CellFormats cellFormats1 = new CellFormats() { Count = (UInt32Value)2U };
			CellFormat cellFormat2 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };

			CellFormat cellFormat3 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyAlignment = true };
			Alignment alignment1 = new Alignment() { WrapText = true };

			cellFormat3.Append( alignment1 );

			cellFormats1.Append( cellFormat2 );
			cellFormats1.Append( cellFormat3 );

			CellStyles cellStyles1 = new CellStyles() { Count = (UInt32Value)1U };
			CellStyle cellStyle1 = new CellStyle() { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U };

			cellStyles1.Append( cellStyle1 );
			DifferentialFormats differentialFormats1 = new DifferentialFormats() { Count = (UInt32Value)0U };
			TableStyles tableStyles1 = new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium9", DefaultPivotStyle = "PivotStyleLight16" };

			stylesheet1.Append( fonts1 );
			stylesheet1.Append( fills1 );
			stylesheet1.Append( borders1 );
			stylesheet1.Append( cellStyleFormats1 );
			stylesheet1.Append( cellFormats1 );
			stylesheet1.Append( cellStyles1 );
			stylesheet1.Append( differentialFormats1 );
			stylesheet1.Append( tableStyles1 );

			workbookStylesPart.Stylesheet = stylesheet1;
		}
	}
}
