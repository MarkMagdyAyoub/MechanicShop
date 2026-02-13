using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers.Vehicles;


public static class VehicleErrors
{
  public static Error MakeRequired => Error.Validation(
    code: "Vehicle_Make_Required", 
    description: "Vehicle Make Is Required"
  );

  public static Error ModelRequired => Error.Validation(
    code: "Vehicle_Model_Required", 
    description: "Vehicle Model Is Required"
  );

  public static Error LicensePlateRequired => Error.Validation(
    code: "Vehicle_LicensePlate_Make_Required", 
    description: "Vehicle License Plate Is Required"
  );

  public static Error YearInvalid => Error.Validation(
    code: "Vehicle_Year_Invalid", 
    description: "Year Must Be Between 1886 And Next Year."
  );

  public static Error UniqueLicensePlateRequired => Error.Validation(
      code: "Vehicle_LicensePlate_AlreadyExists",
      description: "A vehicle With The Same License Plate Already Exists."
  );
}