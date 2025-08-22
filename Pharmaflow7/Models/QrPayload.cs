namespace Pharmaflow7.Helpers
{
    public class QrPayload
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Description { get; set; }
        public string Signature { get; set; }
    }

    public class QrVerifyRequest
    {
        public string RawData { get; set; }
    }
}
