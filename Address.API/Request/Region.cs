namespace Address.API.Request
{
    public class RegionRequest
    {
        public string RegionSearch { get; set; }
        public int[] AddressObjectTypes { get; set; } = {1, 3, 4, 5, 6, 65};
        public int Limit { get; set; } = 10;
        public string AddressObjectId { get; set; }
    }
    public class Region
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Okato { get; set; }
        public bool RegionalCenter { get; set; }
        public int AddressObjectType { get; set; }
    }
}
