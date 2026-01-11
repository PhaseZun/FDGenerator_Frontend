namespace MyApp.Web.Models.FD
{
    public class FDModel
    {

        public int fdId { get; set; }
        public string? Username { get; set;}
        public string? CustomerName { get; set; }
        public double Amount { get; set; }
        public double InterestRate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string? PdfFileName { get; set; }
    }
}
