using System.Text.Json.Serialization;

namespace TasksApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskStatus
{
    [JsonStringEnumMemberName("pendiente")]
    Pendiente,
    [JsonStringEnumMemberName("en curso")]
    EnCurso,
    [JsonStringEnumMemberName("hecha")]
    Hecha
}
