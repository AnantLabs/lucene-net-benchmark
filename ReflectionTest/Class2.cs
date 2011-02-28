using System;
using System.Reflection;
using System.Globalization;

namespace ReflectionTest.Namespace {
	class Class2 {
		DateTime[] dateTimes = new DateTime[ 10 ];
		public DateTime this[ int index ] {
			get { return dateTimes[ index ]; }
			set { dateTimes[ index ] = value; }
		}


		private DateTime dateOfBirth;
		public DateTime DateOfBirth {
			get { return dateOfBirth; }
			set { dateOfBirth = value; }
		}

		public void Test() {
			Console.WriteLine( "Test method called" );
		}


		private string field;

		public string Property {
			get { return field; }
			set { field = value; }
		}

	}
}