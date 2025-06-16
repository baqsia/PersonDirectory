using Mediator;
using Task.PersonDirectory.Application.DTOs;

namespace Task.PersonDirectory.Application.Queries.GetConnectionReport;

public record GetConnectionReportQuery : IRequest<ResponseResult<List<RelatedPersonTypeCountDto>>>;