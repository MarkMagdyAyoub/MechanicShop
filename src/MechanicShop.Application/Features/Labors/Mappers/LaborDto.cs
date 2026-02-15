using MechanicShop.Application.Features.Labors.DTOs;
using MechanicShop.Domain.Employees;

namespace MechanicShop.Application.Features.Labor.Mappers;

public static class LaborMapper
{
  public static LaborDto ToDto(this Employee employee)
  {
    ArgumentNullException.ThrowIfNull(employee);

    return new LaborDto
    {
      Id = employee.Id,
      Name = employee.FullName
    };
  }
}