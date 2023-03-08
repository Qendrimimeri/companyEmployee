using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System.Text.Json;

namespace CompanyEmployees.Presentation.Controllers;

[Route("api/companies/{companyId}/employees")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly IServiceManager _service;
    public EmployeesController(IServiceManager service) => _service = service;


    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
    {
        var pagedResult = await _service.EmployeeService.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);
        Response.Headers.Add("X-Pagination",JsonSerializer.Serialize(pagedResult.metaData));
        return Ok(pagedResult.employees);
    }



    [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
    public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
    {
        var employee = _service.EmployeeService.GetEmployee(companyId, id, trackChanges: false);
        return Ok(employee);
    }


    [HttpPost]
    public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
    {
        if (employee is null) return BadRequest("EmployeeForCreationDto object is null");
        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        var employeeToReturn = _service.EmployeeService.CreateEmployeeForCompany(companyId, employee, trackChanges:false);
        return CreatedAtRoute("GetEmployeeForCompany", new
        {
            companyId,
            id = employeeToReturn.Id
        }, employeeToReturn);
    }

    [HttpPut("{id:guid}")]
    public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid id,[FromBody] EmployeeForUpdateDto employee)
    {
        if (employee is null) return BadRequest("EmployeeForUpdateDto object is null");

        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        _service.EmployeeService.UpdateEmployeeForCompany(companyId, id, employee,
        compTrackChanges: false, empTrackChanges: true);
        return NoContent();
    }

}