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

	public class SimpleSharedStringTablePart {

		public SimpleSharedStringTablePart() {
		}

		public void Generate( Document document, SharedStringTablePart sharedStringTablePart ) {

			SharedStringTable sharedStringTable1 = new SharedStringTable() { Count = (UInt32Value)4U, UniqueCount = (UInt32Value)4U };

			for (int i = 0; i < document.SharedStringList.Count; i++) {
				Text sharedText = new Text();
				sharedText.Text = (String)document.SharedStringList[ i ];
				SharedStringItem sharedStringItem = new SharedStringItem();
				sharedStringItem.Append( sharedText );
				sharedStringTable1.Append( sharedStringItem );
			}

			sharedStringTablePart.SharedStringTable = sharedStringTable1;
		}

	}
}
