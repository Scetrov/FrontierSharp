// ReSharper disable UnusedTypeParameter

namespace FrontierSharp.HttpClient.Models;

public abstract class GetRequestModel<T> : RequestModelBase where T : new() {
    public abstract string GetCacheKey();
}