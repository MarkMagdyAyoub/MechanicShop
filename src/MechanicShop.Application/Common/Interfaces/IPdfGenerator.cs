using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Common.Interfaces;

public interface IPdfGenerator
{
  byte[] Generate(Invoice invoice);
}