namespace MechanicShop.Application.SubcutaneousTests.Common;

[CollectionDefinition(CollectionName)]
public class SubcutaneousTestAppFactoryCollection : ICollectionFixture<SubcutaneousTestAppFactory>
{
  public const string CollectionName = "SubcutaneousTestAppFactory";
}