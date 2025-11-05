namespace MyApp.Web.Models.FD
{
    public class FDModel
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public double Amount { get; set; }
        public double InterestRate { get; set; }
        public string? MaturityDate { get; set; }
        public string? PdfFileName { get; set; }
    }
}
