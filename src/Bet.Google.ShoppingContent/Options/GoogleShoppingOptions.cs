namespace Bet.Google.ShoppingContent.Options
{
    public class GoogleShoppingOptions
    {
        public string? GoogleServiceAccountFile { get; set; }

        public byte[]? GoogleServiceAccount { get; set; }

        public int MaxListPageSize { get; set; } = 100;

        public ulong MerchantId { get; set; }
    }
}
