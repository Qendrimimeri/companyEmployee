using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System.Dynamic;

namespace Service;

internal sealed class EmployeeService : IEmployeeService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;

    public EmployeeService(IRepositoryManager repository, 
                           ILoggerManager logger,
                           IMapper mapper )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }


    public async Task<IEnumerable<EmployeeDto>> GetEmployees(Guid companyId, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);

        var employeesFromDb = _repository.Employee.GetEmployees(companyId, trackChanges);
        var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
        return employeesDto;
    }


    public async Task<EmployeeDto> GetEmployee(Guid companyId, Guid id, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);

        var employeeDb = _repository.Employee.GetEmployee(companyId, id, trackChanges) ?? throw new EmployeeNotFoundException(id);
        var employee = _mapper.Map<EmployeeDto>(employeeDb);
        return employee;
    }

    public async Task<EmployeeDto> CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);
        var employeeEntity = _mapper.Map<Employee>(employeeForCreation);
        _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
        await _repository.SaveAsync();
        var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
        return employeeToReturn;
    }

    public async Task DeleteEmployeeForCompany(Guid companyId, Guid id, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);
        var employeeForCompany = _repository.Employee.GetEmployee(companyId, id, trackChanges) ?? throw new EmployeeNotFoundException(id);
        _repository.Employee.DeleteEmployee(employeeForCompany);
        await _repository.SaveAsync();
    }

    public async Task UpdateEmployeeForCompany
        (Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChanges)
    {
        await CheckIfCompanyExists(companyId, compTrackChanges);

        var employeeEntity = _repository.Employee.GetEmployee(companyId, id, empTrackChanges) ?? throw new EmployeeNotFoundException(id);

        _mapper.Map(employeeForUpdate, employeeEntity);
        await _repository.SaveAsync();
    }


    public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatch
                 (Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
    {
        await CheckIfCompanyExists(companyId, compTrackChanges);
        var employeeEntity = _repository.Employee.GetEmployee(companyId, id, empTrackChanges) 
            ?? throw new EmployeeNotFoundException(companyId);
        var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
        return (employeeToPatch, employeeEntity);
    }
    public async Task SaveChangesForPatch(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
    {
        _mapper.Map(employeeToPatch, employeeEntity);
        await _repository.SaveAsync();
    }


    public async Task<(IEnumerable<EmployeeDto> employees, MetaData metaData)> GetEmployeesAsync
    (Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    {
        if (!employeeParameters.ValidAgeRange)
            throw new MaxAgeRangeBadRequestException();
        await CheckIfCompanyExists(companyId, trackChanges);
        var employeesWithMetaData = await _repository.Employee
        .GetEmployeesAsync(companyId, employeeParameters, trackChanges);
        var employeesDto =
        _mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetaData);
        return (employees: employeesDto, metaData: employeesWithMetaData.MetaData);
    }






    private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
    {
        var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges) 
            ?? throw new CompanyNotFoundException(companyId);
    }

    private Employee GetEmployeeForCompanyAndCheckIfItExists (Guid companyId, Guid id, bool trackChanges)
    {
        var employeeDb = _repository.Employee.GetEmployee(companyId, id,  trackChanges);
        return employeeDb is null ? throw new EmployeeNotFoundException(id) : employeeDb;
    }

}


