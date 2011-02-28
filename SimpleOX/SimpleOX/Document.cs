using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using Ap = DocumentFormat.OpenXml.ExtendedProperties;
using Vt = DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;

namespace SimpleOX.Excel {

	/// <summary>
	/// This is the class that needs to be instatiated in order to add sheets
	/// to an Excel document - this is the description of the excel document.
	/// </summary>
	public class Document {

		// Summary: Used to generate a unique ID for every string in the document
		int _sharedStringIndex = 0;
		// Summary: ArrayList of shared strings
		Hashtable _sharedStringList = new Hashtable();

		public Hashtable SharedStringList {
			get { return _sharedStringList; }
			set { _sharedStringList = value; }
		}
		// Summary: Used to generate a uniqe ID for every sheet
		int _sheetIndex = 5;
		// Summary: used to store the start of the index
		int _sheetIndexStart;
		// Summary: ArrayList of sheets to add
		Hashtable _sheets = new Hashtable();

		public Hashtable Sheets {
			get { return _sheets; }
			set { _sheets = value; }
		}

		public Hashtable sheetTitles {
			get {
				Hashtable _titles = new Hashtable();
				foreach( int key in Sheets.Keys ) {
					_titles[ key ] = ((ISheet)Sheets[ key ]).Title;
				}
				return _titles;
			}
		}

		// Summary: Default constructor - no arguments required
		public Document() {
			_sheetIndexStart = _sheetIndex;
		}

		/// <summary>
		/// Adds the string to the shared string list and returns the 
		/// index the string can be found at
		/// </summary>
		/// <param name="input">The string to add to the list of shared strings</param>
		/// <returns>Index to be used to reference the shared string</returns>
		public int AddString( String input ) {
			_sharedStringList[ _sharedStringIndex ] = input;
			return _sharedStringIndex++;
		}

		/// <summary>
		/// Adds a sheet to the document at the current index
		/// </summary>
		/// <param name="sheet">An ISheet to add</param>
		/// <returns>Index to be used to reference this sheet</returns>
		public int CreateDataSheet(String title = null) {
			if( title == null )
				title = "Sheet" + _sheetIndex;
			ISheet sheet = new DataSheet(this, title);
			_sheets[ _sheetIndex ] = sheet;
			return _sheetIndex++;
		}

		/// <summary>
		/// This takes the document defined in this class and saves it
		/// out to disk.
		/// </summary>
		/// <param name="filePath"></param>
		public void SaveDocument( String filePath ) {
			using( SpreadsheetDocument package = SpreadsheetDocument.Create( filePath, SpreadsheetDocumentType.Workbook ) ) {
				CreateParts( package );
			}
		}

		public void CreateParts( SpreadsheetDocument document ) {

			LogWorkbook lw = new LogWorkbook();
			ExtendedFilePropertiesPart extendedFilePropertiesPart = document.AddNewPart<ExtendedFilePropertiesPart>( "rId3" );
			SimpleExtendedFilePropertiesPart fileProperties = new SimpleExtendedFilePropertiesPart();
			fileProperties.Generate( sheetTitles, extendedFilePropertiesPart );

			WorkbookPart workbookPart = document.AddWorkbookPart();
			SimpleWorkbookPart fileWorkbook = new SimpleWorkbookPart();
			fileWorkbook.Generate( sheetTitles, workbookPart );

			WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>( "rId3" );
			SimpleWorkbookStylesPart fileStyles = new SimpleWorkbookStylesPart();
			fileStyles.Generate(workbookStylesPart);

			ThemePart themePart = workbookPart.AddNewPart<ThemePart>( "rId2" );
			SimpleThemePart fileTheme = new SimpleThemePart();
			fileTheme.Generate( themePart );

			foreach( int key in Sheets.Keys ) {
			    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>( "rId" + key.ToString() );
			    SimpleWorksheetPart workSheet = new SimpleWorksheetPart();
			    workSheet.Generate( (DataSheet)Sheets[ key ], worksheetPart );
			}

			SharedStringTablePart sharedStringTablePart = workbookPart.AddNewPart<SharedStringTablePart>( "rId4" );
			SimpleSharedStringTablePart fileSharedString = new SimpleSharedStringTablePart();
			fileSharedString.Generate( this, sharedStringTablePart );

			SetPackageProperties( document );

		}

		private void SetPackageProperties( OpenXmlPackage document ) {
			document.PackageProperties.Creator = "ramsey";
			document.PackageProperties.Created = System.Xml.XmlConvert.ToDateTime( "2011-02-16T17:22:01Z", System.Xml.XmlDateTimeSerializationMode.RoundtripKind );
			document.PackageProperties.Modified = System.Xml.XmlConvert.ToDateTime( "2011-02-16T17:31:26Z", System.Xml.XmlDateTimeSerializationMode.RoundtripKind );
			document.PackageProperties.LastModifiedBy = "ramsey";
		}
	}
}
