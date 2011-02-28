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

	public class SimpleExtendedFilePropertiesPart {

		public SimpleExtendedFilePropertiesPart() {
		}

		/// <summary>
		/// Generates the ExtendedFilePropertiesPart and adds it to the 
		/// Microsoft Document
		/// </summary>
		/// <param name="Titles">A Name[] of titles for each sheet</param>
		/// <param name="extendedFilePropertiesPart">The ExtendedFilePropertiesPart generate for the doucment</param>
		public void Generate( Hashtable Titles, ExtendedFilePropertiesPart extendedFilePropertiesPart ) {

			Ap.Properties properties1 = new Ap.Properties();
			properties1.AddNamespaceDeclaration( "vt", "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes" );
			Ap.Application application1 = new Ap.Application();
			application1.Text = "Microsoft Excel";
			Ap.DocumentSecurity documentSecurity1 = new Ap.DocumentSecurity();
			documentSecurity1.Text = "0";
			Ap.ScaleCrop scaleCrop1 = new Ap.ScaleCrop();
			scaleCrop1.Text = "false";

			Ap.HeadingPairs headingPairs1 = new Ap.HeadingPairs();

			Vt.VTVector vTVector1 = new Vt.VTVector() { BaseType = Vt.VectorBaseValues.Variant, Size = (UInt32Value)2U };

			Vt.Variant variant1 = new Vt.Variant();
			Vt.VTLPSTR vTLPSTR1 = new Vt.VTLPSTR();
			vTLPSTR1.Text = "Worksheets";

			variant1.Append( vTLPSTR1 );

			Vt.Variant variant2 = new Vt.Variant();
			Vt.VTInt32 vTInt321 = new Vt.VTInt32();
			vTInt321.Text = Titles.Count.ToString(); // The number of sheets their are

			variant2.Append( vTInt321 );

			vTVector1.Append( variant1 );
			vTVector1.Append( variant2 );

			headingPairs1.Append( vTVector1 );

			Ap.TitlesOfParts titlesOfParts1 = new Ap.TitlesOfParts();

			Vt.VTVector vTVector2 = new Vt.VTVector() { BaseType = Vt.VectorBaseValues.Lpstr, Size = new UInt32Value((uint)Titles.Count) }; // size needs to be the number of sheets
			foreach( int key in Titles.Keys ) {
				Vt.VTLPSTR vTLPSTR2 = new Vt.VTLPSTR();
				vTLPSTR2.Text = (String)Titles[key];
				vTVector2.Append( vTLPSTR2 );
			}
			//Vt.VTLPSTR vTLPSTR2 = new Vt.VTLPSTR();
			//vTLPSTR2.Text = "Log";
			//Vt.VTLPSTR vTLPSTR3 = new Vt.VTLPSTR(); // ad now vector for sheet titles
			//vTLPSTR3.Text = "Sheet1";

			//vTVector2.Append( vTLPSTR2 );
			//vTVector2.Append( vTLPSTR3 ); // append each of them

			titlesOfParts1.Append( vTVector2 );
			Ap.LinksUpToDate linksUpToDate1 = new Ap.LinksUpToDate();
			linksUpToDate1.Text = "false";
			Ap.SharedDocument sharedDocument1 = new Ap.SharedDocument();
			sharedDocument1.Text = "false";
			Ap.HyperlinksChanged hyperlinksChanged1 = new Ap.HyperlinksChanged();
			hyperlinksChanged1.Text = "false";
			Ap.ApplicationVersion applicationVersion1 = new Ap.ApplicationVersion();
			applicationVersion1.Text = "12.0000";

			properties1.Append( application1 );
			properties1.Append( documentSecurity1 );
			properties1.Append( scaleCrop1 );
			properties1.Append( headingPairs1 );
			properties1.Append( titlesOfParts1 );
			properties1.Append( linksUpToDate1 );
			properties1.Append( sharedDocument1 );
			properties1.Append( hyperlinksChanged1 );
			properties1.Append( applicationVersion1 );

			extendedFilePropertiesPart.Properties = properties1;
		}
	}
}
