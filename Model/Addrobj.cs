using System;
using System.Xml.Serialization;

namespace FIAS
{
    [Serializable]
    [XmlType("Object")]
    public partial class Addrobj
    {
        [XmlAttribute("AOGUID")]
        public string Aoguid { get; set; }
        [XmlAttribute("FORMALNAME")]
        public string Formalname { get; set; }
        [XmlAttribute("REGIONCODE")]
        public string Regioncode { get; set; }
        [XmlAttribute("AUTOCODE")]
        public string Autocode { get; set; }
        [XmlAttribute("AREACODE")]
        public string Areacode { get; set; }
        [XmlAttribute("CITYCODE")]
        public string Citycode { get; set; }
        [XmlAttribute("CTARCODE")]
        public string Ctarcode { get; set; }
        [XmlAttribute("PLACECODE")]
        public string Placecode { get; set; }
        [XmlAttribute("PLANCODE")]
        public string Plancode { get; set; }
        [XmlAttribute("STREETCODE")]
        public string Streetcode { get; set; }
        [XmlAttribute("EXTRCODE")]
        public string Extrcode { get; set; }
        [XmlAttribute("SEXTCODE")]
        public string Sextcode { get; set; }
        [XmlAttribute("OFFNAME")]
        public string Offname { get; set; }
        [XmlAttribute("POSTALCODE")]
        public string Postalcode { get; set; }
        [XmlAttribute("IFNSFL")]
        public string Ifnsfl { get; set; }
        [XmlAttribute("TERRIFNSFL")]
        public string Terrifnsfl { get; set; }
        [XmlAttribute("IFNSUL")]
        public string Ifnsul { get; set; }
        [XmlAttribute("TERRIFNSUL")]
        public string Terrifnsul { get; set; }
        [XmlAttribute("OKATO")]
        public string Okato { get; set; }
        [XmlAttribute("OKTMO")]
        public string Oktmo { get; set; }
        [XmlAttribute("UPDATEDATE")]
        public DateTime Updatedate { get; set; }
        [XmlAttribute("SHORTNAME")]
        public string Shortname { get; set; }
        [XmlAttribute("AOLEVEL")]
        public int Aolevel { get; set; }
        [XmlAttribute("PARENTGUID")]
        public string Parentguid { get; set; }
        [XmlAttribute("AOID")]
        public string Aoid { get; set; }
        [XmlAttribute("PREVID")]
        public string Previd { get; set; }
        [XmlAttribute("NEXTID")]
        public string Nextid { get; set; }
        [XmlAttribute("CODE")]
        public string Code { get; set; }
        [XmlAttribute("PLAINCODE")]
        public string Plaincode { get; set; }
        [XmlAttribute("ACTSTATUS")]
        public int Actstatus { get; set; }
        [XmlAttribute("CENTSTATUS")]
        public int Centstatus { get; set; }
        [XmlAttribute("OPERSTATUS")]
        public int Operstatus { get; set; }
        [XmlAttribute("CURRSTATUS")]
        public int Currstatus { get; set; }
        [XmlAttribute("STARTDATE")]
        public DateTime Startdate { get; set; }
        [XmlAttribute("ENDDATE")]
        public DateTime Enddate { get; set; }
        [XmlAttribute("NORMDOC")]
        public string Normdoc { get; set; }
        [XmlAttribute("LIVESTATUS")]
        public int Livestatus { get; set; }
        [XmlAttribute("DIVTYPE")]
        public int Divtype { get; set; }
    }
}
