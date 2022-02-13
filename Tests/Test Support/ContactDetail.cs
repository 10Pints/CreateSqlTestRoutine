using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
   internal class ContactDetail
   {
      public int contactDetail_id { get; set; }
      public int person_id { get; set; }
      public string name { get; set; } = "";
      public string? email { get; set; }
      public string? phone { get; set; }
      public string? mobile { get; set; }
      public string address_1 { get; set; } = "";
      public string? address_2 { get; set; }
      public string address_3 { get; set; } = "";
      public string town { get; set; } = "";
      public string province { get; set; } = "";
      public string region { get; set; } = "";
      public int country_id { get; set; }
      public string postcode { get; set; } = "";
      public int contactDetailType_id { get; set; }
      public DateTime? date_created { get; set; }
      public DateTime? last_update { get; set; }
   }
}
