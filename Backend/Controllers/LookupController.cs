using Application.Entities.Common;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

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
    [ProducesResponseType(typeof(BaseResponse<List<SelectListItem>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetEnumOptions(string enumName)
    {
        if (string.IsNullOrEmpty(enumName))
            return NotFound($"enumValue was not provided.");

        var enumTypes = new Dictionary<string, Type>
        {
            { "gender", typeof(Domain.Enums.Gender) },
            { "size", typeof(Domain.Enums.AnimalSize) },
            { "temperament", typeof(Domain.Enums.Temperament) }
        };

        if (!enumTypes.TryGetValue(enumName.ToLower(), out var enumType))
            return NotFound($"Enum {enumName} not found.");

        var selectListItems = Enum.GetValues(enumType)
            .Cast<Enum>()
            .Select(e => new SelectListItem
            {
                Value = e.ToString(),
                Text = e.ToString()
            })
            .ToList();

        var successResponse = new BaseResponse<List<SelectListItem>>(selectListItems);

        return Ok(successResponse);
    }
}
