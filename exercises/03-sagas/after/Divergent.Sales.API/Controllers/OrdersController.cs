﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Divergent.Sales.Data.Context;
using NServiceBus;
using Divergent.Sales.Messages.Commands;

namespace Divergent.Sales.API.Controllers
{
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private readonly IEndpointInstance _endpoint;

        public OrdersController(IEndpointInstance endpoint)
        {
            _endpoint = endpoint;
        }

        [HttpPost, Route("createOrder")]
        public async Task<dynamic> CreateOrder(dynamic payload)
        {
            var customerId = int.Parse((string)payload.customerId);
            var productIds = ((IEnumerable<dynamic>)payload.products)
                .Select(p => int.Parse((string)p.productId))
                .ToList();

            await _endpoint.Send(new SubmitOrderCommand
            {
                CustomerId = customerId,
                Products = productIds
            });

            return payload;
        }

        [HttpGet]
        public IEnumerable<dynamic> Get()
        {
            using (var _context = new SalesContext())
            {
                var orders = _context.Orders
                    .Include(i => i.Items)
                    .Include(i => i.Items.Select(x => x.Product))
                    .ToArray();

                return orders
                    .Select(o => new
                    {
                        o.Id,
                        o.CustomerId,
                        ProductIds = o.Items.Select(i => i.Product.Id),
                        ItemsCount = o.Items.Count
                    });
            }
        }
    }
}
