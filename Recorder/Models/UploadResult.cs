using System.Text.Json.Serialization;

namespace Recorder.Models
{
    public class UploadResult
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("proofId")]
        public int ProofId { get; set; }
    }
}