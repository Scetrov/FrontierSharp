// ReSharper disable UnusedTypeParameter

namespace FrontierSharp.HttpClient.Models;

public abstract class PostRequestModel<T> : RequestModelBase {
    public abstract HttpContent GetHttpContent();
}