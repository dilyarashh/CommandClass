using System.Text.Json.Serialization;

namespace PotteryClass.Data.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileType
{
    Image,
    Video,
    Document
}