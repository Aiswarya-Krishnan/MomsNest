using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.Models.ViewModels
{
	public class AddressViewModel
	{
		public string StreetAddress { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string PostalCode { get; set; }

		public string FormattedAddress => $"{StreetAddress}, {City}, {State} {PostalCode}";
	}
}
