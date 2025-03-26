using FluentResults;
using FrontierSharp.HttpClient.Models;

namespace FrontierSharp.HttpClient;

public interface IFrontierSharpHttpClient {
    Task<IResult<TResponseModel>> Get<TRequestModel, TResponseModel>(TRequestModel requestModel, CancellationToken cancellationToken = default) where TRequestModel : GetRequestModel<TRequestModel>, new() where TResponseModel : class;
}