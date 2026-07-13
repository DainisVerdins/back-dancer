using Application.Exceptions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/lookup")]
[ApiVersion("1.0")]
[Authorize]
public class LookupController : Controller
{
    public LookupController()
    {
    }

    /// <summary>
    /// Get enums values for dropdown lists
    /// </summary>
    /// <param name="enumName">enum name to whom get list of items</param>
    /// <returns>collection of SelectListItems</returns>
    [HttpGet("{enumName}")]
    [ProducesResponseType(typeof(List<SelectListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetEnumOptions(string enumName)
    {
        if (string.IsNullOrEmpty(enumName))
            throw new ArgumentException("enumValue was not provided.");

        var enumTypes = new Dictionary<string, Type>
        {
            { "gender", typeof(Domain.Enums.Gender) },
            { "size", typeof(Domain.Enums.AnimalSize) },
            { "temperament", typeof(Domain.Enums.Temperament) }
        };

        if (!enumTypes.TryGetValue(enumName.ToLower(), out var enumType))
            throw new NotFoundException($"Enum {enumName} was not found.");

        var selectListItems = Enum.GetValues(enumType)
            .Cast<Enum>()
            .Select(e => new SelectListItem
            {
                Value = Convert.ToInt32(e).ToString(),
                Text = e.ToString()
            })
            .ToList();

        var successResponse = new List<SelectListItem>(selectListItems);

        return Ok(successResponse);
    }
}
