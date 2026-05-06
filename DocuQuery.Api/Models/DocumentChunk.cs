using Microsoft.Extensions.VectorData;
namespace DocuQuery.Api.Models
{
    public class DocumentChunk
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [VectorStoreData]
        public string FileName { get; set; } = string.Empty;
        [VectorStoreData]
        public int PageNumber { get; set; }
        [VectorStoreData]
        public string Content { get; set; } = string.Empty;
        [VectorStoreData]
        public string SessionId { get; set; } = string.Empty;
        [VectorStoreVector(3072)]
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
