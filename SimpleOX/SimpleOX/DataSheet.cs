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
	/// An instance of this class is needed for each sheet to be filled
	/// with data.  It doesn't allow for charts or pictures or anything
	/// fancy, just simple data types in cells.
	/// </summary>
	public class DataSheet : ISheet {

		// Summary: Hashtable of Cols for this sheet
		Hashtable _cols = new Hashtable();

		public Hashtable Cols {
			get { return _cols; }
			set { _cols = value; }
		}

		// TODO: adjust this so that it can handle more than 26 columsn
		// Summary: Column index used to keep track of the column we are working in
		Char _columnIndex = 'A';

		// Summary: ArrayList of rows for this sheet
		ArrayList _rowList = new ArrayList();

		// Summary: Row index is used to keep track of the row we are working in
		// it has to be in UInt32 because that is what the open XML SDK expects
		UInt32 _rowIndex = 0;

		public UInt32 RowIndex {
			get { return _rowIndex; }
			set { _rowIndex = value; }
		}

		// Summary: We need to keep track of wath is the farthest column actually
		// used, so we'll set this
		int _farthestCol;

		public int FarthestCol {
			get { return _farthestCol; }
		}

		// Summary: Title of the sheet
		public String Title { get; set; }

		// Summary: Reference to the coudment this belongs to
		Document _document;

		public DataSheet() {
		}

		public DataSheet(Document document, String title ) : this() {
			this.Title = title;
			this._document = document;
		}

		public void StartRow () {
			_rowIndex++;
			_columnIndex = 'A';
		}

		public void AddCell( String input ) {
			int key = _document.AddString( input );
			_cols[ _columnIndex.ToString() + _rowIndex.ToString() ] = key.ToString();
			_columnIndex++;
			if( _columnIndex > _farthestCol ) {
				// we subtract 65 from the column index becasue columnIndex
				// is an ASCII letter.  Since A is the first letter (which is
				// ASCII number 65) we subtract to get an actual column number
				// from a 1 index.
				_farthestCol = _columnIndex - 65;
			}
		}

		public void AddCell( int input ) {
			//int key = _document.AddString( input );
			_cols[ _columnIndex.ToString() + _rowIndex.ToString() ] = input;
			_columnIndex++;
			if( _columnIndex > _farthestCol ) {
				// we subtract 65 from the column index becasue columnIndex
				// is an ASCII letter.  Since A is the first letter (which is
				// ASCII number 65) we subtract to get an actual column number
				// from a 1 index.
				_farthestCol = _columnIndex - 65;
			}
		}

		public void AddCell( float input ) {
			//int key = _document.AddString( input );
			_cols[ _columnIndex.ToString() + _rowIndex.ToString() ] = input;
			_columnIndex++;
			if( _columnIndex > _farthestCol ) {
				// we subtract 65 from the column index becasue columnIndex
				// is an ASCII letter.  Since A is the first letter (which is
				// ASCII number 65) we subtract to get an actual column number
				// from a 1 index.
				_farthestCol = _columnIndex - 65;
			}
		}

		public void AddRowAndCell( String input ) {
			this.StartRow();
			this.AddCell( input );
		}

		public void AddRowAndCell( int input ) {
			this.StartRow();
			this.AddCell( input );
		}

		public void AddRowAndCell( float input ) {
			this.StartRow();
			this.AddCell( input );
		}

		public void AddRowsAndCellsSplit( String input, Char split = '\n', String prefix = "" ) {
			String[] lines = input.Split( split );
			foreach( String line in lines ) {
				this.AddRowAndCell(line);
			}
		}

		public void AddRowsAndCellsSplit( String input, String prefix ) {
			AddRowsAndCellsSplit( input, '\n', prefix );
		}

	}
}
