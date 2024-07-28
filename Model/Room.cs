using System;
using System.Collections.Generic;

namespace FIAS
{
    public partial class Room
    {
        public string Roomguid { get; set; }
        public string Flatnumber { get; set; }
        public int Flattype { get; set; }
        public string Roomnumber { get; set; }
        public int? Roomtype { get; set; }
        public string Regioncode { get; set; }
        public string Postalcode { get; set; }
        public DateTime Updatedate { get; set; }
        public string Houseguid { get; set; }
        public string Roomid { get; set; }
        public string Previd { get; set; }
        public string Nextid { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
        public int Livestatus { get; set; }
        public string Normdoc { get; set; }
        public int Operstatus { get; set; }
        public string Cadnum { get; set; }
        public string Roomcadnum { get; set; }
    }
}
