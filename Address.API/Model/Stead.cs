using System;

namespace Address.API.Model
{
    public partial class Stead
    {
        public string Steadguid { get; set; }
        public string Number { get; set; }
        public string Regioncode { get; set; }
        public string Postalcode { get; set; }
        public string Ifnsfl { get; set; }
        public string Terrifnsfl { get; set; }
        public string Ifnsul { get; set; }
        public string Terrifnsul { get; set; }
        public string Okato { get; set; }
        public string Oktmo { get; set; }
        public DateTime Updatedate { get; set; }
        public string Parentguid { get; set; }
        public string Steadid { get; set; }
        public string Previd { get; set; }
        public string Nextid { get; set; }
        public int Operstatus { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
        public string Normdoc { get; set; }
        public int Livestatus { get; set; }
        public string Cadnum { get; set; }
        public int Divtype { get; set; }
    }
}
