namespace Bet.Google.ShoppingContent.Options
{
    public class AppsOptions
    {
        public string? GoogleServiceAccountFile { get; set; }

        public byte[]? GoogleServiceAccount { get; set; }

        public int MaxListPageSize { get; set; } = 50;

        public ulong MerchantId { get; set; }
    }
}
