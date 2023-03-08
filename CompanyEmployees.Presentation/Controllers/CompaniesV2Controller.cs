using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace CompanyEmployees.Presentation.Controllers;


//[ApiVersion("2.0")]  beacuse we have configure in ConfigureVersioning
// [ApiVersion("2.0", Deprecated = true)]
[Route("api/v2/companies")]
[ApiController]
public class CompaniesV2Controller : ControllerBase
{
    private readonly IServiceManager _service;
    public CompaniesV2Controller(IServiceManager service) => _service = service;



    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _service.CompanyService.GetAllCompaniesAsync(trackChanges: false);
        var companiesV2 = companies.Select(x => $"{x.Name} V2");
        return Ok(companiesV2);
    }

}
