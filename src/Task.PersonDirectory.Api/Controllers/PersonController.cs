using Mediator;
using Microsoft.AspNetCore.Mvc;
using Task.PersonDirectory.Application.Commands.AddRelatedPerson;
using Task.PersonDirectory.Application.Commands.CreatePerson;
using Task.PersonDirectory.Application.Commands.DeletePerson;
using Task.PersonDirectory.Application.Commands.UpdatePerson;
using Task.PersonDirectory.Application.Commands.UploadPersonImage;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Queries.GetConnectionReport;
using Task.PersonDirectory.Application.Queries.GetPerson;
using Task.PersonDirectory.Application.Queries.GetPersons;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Api.Controllers;

[ApiController]
[Route("api/person")]
public class PersonController(
    IMediator mediator,
    IResourceLocalizer resourceLocalizer
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> All([FromQuery] GetPersonsQuery query)
    {
        var persons = await mediator.Send(query);
        return Ok(persons);
    }

    [HttpGet("{personId:int}")]
    public async Task<IActionResult> Get(int personId)
    {
        var result = await mediator.Send(new GetPersonByIdQuery(personId));
        return result.Match<IActionResult>(
            _ => Problem(statusCode: 404, title: resourceLocalizer.Localize(ResourceKeys.PersonNotFound)),
            Ok
        );
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePersonCommand command)
    {
        var result = await mediator.Send(command);
        return result.Match<IActionResult>(
            _ => Problem(statusCode: 404, title: resourceLocalizer.Localize(ResourceKeys.CityNotFound)),
            re => CreatedAtAction("Get", new { personId = re.Result }, re)
        );
    }

    [HttpPut("{personId:int}")]
    public async Task<IActionResult> Update(int personId, [FromBody] UpdatePersonCommand command)
    {
        var result = await mediator.Send(command);
        return result.Match<IActionResult>(
            _ => Problem(statusCode: 404, title: resourceLocalizer.Localize(ResourceKeys.PersonNotFound)),
            _ => Problem(statusCode: 404, title: resourceLocalizer.Localize(ResourceKeys.CityNotFound)),
            _ => Accepted("Get", new { id = personId })
        );
    }

    [HttpDelete("{personId:int}")]
    public async Task<IActionResult> Delete(int personId)
    {
        var result = await mediator.Send(new DeletePersonCommand(personId));
        return result.Match<IActionResult>(
            _ => Problem(statusCode: 404, title: resourceLocalizer.Localize(ResourceKeys.PersonNotFound)),
            _ => NoContent()
        );
    }

    [HttpPost("{personId:int}/upload-image")]
    public async Task<IActionResult> UploadImage(int personId, [FromForm(Name = "image")] IFormFile image)
    {
        await using var stream = image.OpenReadStream();
        var fileDto = new FileUploadDto(
            Content: stream,
            FileName: image.FileName,
            ContentType: image.ContentType,
            Length: image.Length
        );

        var result = await mediator.Send(new UploadPersonImageCommand(personId, fileDto));

        return result.Match<IActionResult>(
            _ => Problem(statusCode: 404, title: resourceLocalizer.Localize(ResourceKeys.PersonNotFound)),
            _ => NoContent()
        );
    }

    [HttpPost("{personId:int}/connect")]
    public async Task<IActionResult> AddRelatedPerson(int personId, [FromBody] AddRelationPersonDto request)
    {
        var result = await mediator.Send(new AddRelatedPersonCommand(personId, request));

        return result.Match<IActionResult>(
            _ => Problem(statusCode: 404, title: resourceLocalizer.Localize(ResourceKeys.PersonNotFound)),
            relationResult => Ok(relationResult
                ? new ResponseResult<string>(resourceLocalizer.Localize(ResourceKeys.RelationAdded))
                : new ResponseResult<string>(resourceLocalizer.Localize(ResourceKeys.RelationDropped))
            )
        );
    }

    [HttpGet("connections-report")]
    public async Task<IActionResult> GetConnectionReport()
    {
        var result = await mediator.Send(new GetConnectionReportQuery());
        return Ok(result);
    }
}