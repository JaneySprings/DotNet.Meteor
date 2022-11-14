using DotNet.Mobile.Debug.Entities;

namespace DotNet.Mobile.Debug.Protocol;

public class ErrorResponseBody : ResponseBody {
    public Message error { get; }

    public ErrorResponseBody(Message error) {
        this.error = error;
    }
}