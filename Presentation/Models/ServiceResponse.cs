namespace Presentation.Models;

public class ServiceResponse<T> : BaseResponse<T>
{
    public bool Success { get; set; } = false;
    public string? Message { get; set; }
    public string? Error { get; set; }
}
