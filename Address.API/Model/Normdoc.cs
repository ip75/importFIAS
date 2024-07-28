using System;

namespace Address.API.Model
{
    public partial class Normdoc
    {
        public string Normdocid { get; set; }
        public string Docname { get; set; }
        public DateTime? Docdate { get; set; }
        public string Docnum { get; set; }
        public int Doctype { get; set; }
        public string Docimgid { get; set; }
    }
}
