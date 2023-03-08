using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Presentation.Extensions;
using CompanyEmployees.Presentation.ModelBinders;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace CompanyEmployees.Presentation.Controllers;

[Route("api/companies")]
[ApiController]
[ResponseCache(CacheProfileName = "120SecondsDuration")]
public class CompaniesController : ApiControllerBase
{
    private readonly IServiceManager _service;
    public CompaniesController(IServiceManager service) => _service = service;


    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies()
    {
        var baseResult = await _service.CompanyService.GetAllCompanies(trackChanges:false);
        var companies = baseResult.GetResult<IEnumerable<CompanyDto>>();
        return Ok(companies);
    }


    [HttpGet("company/{id:guid}")]
    public async Task<IActionResult> GetCompany(Guid id)
    {
        var baseResult = await _service.CompanyService.GetCompany(id, trackChanges: false);

        if (!baseResult.Success)
            return ProcessError(baseResult);

        var company = baseResult.GetResult<CompanyDto>();
        return Ok(company);
    }




    [HttpPost(Name = "CreateCompany")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
    {
        if (company is null)  return BadRequest("CompanyForCreationDto object is null");

        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        var createdCompany = await _service.CompanyService.CreateCompanyAsync(company);
        return CreatedAtRoute("CompanyById", new { id = createdCompany.Id },
        createdCompany);
    }



    [HttpGet("collection/({ids})", Name = "CompanyCollection")]
    public async Task<IActionResult> GetCompanyCollection ([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
    {
        var companies = await _service.CompanyService.GetByIdsAsync(ids, trackChanges: false);
        return Ok(companies);
    }


    [HttpPost("collection")]
    public async Task<IActionResult> CreateCompanyCollection ([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
    {
        var result = await _service.CompanyService.CreateCompanyCollectionAsync(companyCollection);
        return CreatedAtRoute("CompanyCollection", new { result.ids },
        result.companies);
    }



    //[HttpDelete("{id:guid}")]
    //public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
    //{
    //    _service.EmployeeService.DeleteEmployeeForCompany(companyId, id, trackChanges: false);
    //    return NoContent();
    //}


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        await _service.CompanyService.DeleteCompanyAsync(id, trackChanges: false);
        return NoContent();
    }


    [HttpPut("{id:guid}")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company)
    {
        if (company is null) return BadRequest("CompanyForUpdateDto object is null");
        await _service.CompanyService.UpdateCompanyAsync(id, company, trackChanges: true);
        return NoContent();
    }


    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
    {
        if (patchDoc is null) return BadRequest("patchDoc object sent from client is null.");

        var (employeeToPatch, employeeEntity) = await _service.EmployeeService.GetEmployeeForPatch(companyId, id, compTrackChanges: false, empTrackChanges: true);

        patchDoc.ApplyTo(employeeToPatch, ModelState);

        TryValidateModel((employeeToPatch, employeeEntity).employeeToPatch);

        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        await _service.EmployeeService.SaveChangesForPatch(employeeToPatch, employeeEntity);
        return NoContent();
    }


    [HttpOptions]
    public IActionResult GetCompaniesOptions()
    {
        Response.Headers.Add("Allow", "GET, OPTIONS, POST");
        return Ok();
    }


}
