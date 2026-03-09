using System.Collections.Generic;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }

    public ApiResponse()
    {
        Errors = new List<string>();
    }

    public ApiResponse(bool success, string message, T data)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = new List<string>();
    }

    public ApiResponse(bool success, string message, List<string> errors)
    {
        Success = success;
        Message = message;
        Errors = errors;
    }
}
