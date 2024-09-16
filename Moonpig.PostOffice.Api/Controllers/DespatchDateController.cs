namespace Moonpig.PostOffice.Api.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Microsoft.AspNetCore.Mvc;
using Model;

[Route("api/[controller]")]
public class DespatchDateController : Controller
{
    private readonly IDbContext _dbContext;

    public DespatchDateController(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    //Calculate the time for dispatch products based on orderDate
    public DespatchDate Get(List<int> productIds, DateTime orderDate)
    {
        var mlt = orderDate; // max lead time

        foreach (var ID in productIds)
        {
            var supplierId = _dbContext.Products.Single(x => x.ProductId == ID).SupplierId;

            var leadTime = _dbContext.Suppliers.Single(x => x.SupplierId == supplierId).LeadTime;

            //Get the maximum time it takes a supplier to handle a product
            var orderLeadTime = orderDate.AddDays(leadTime); 
            if (orderLeadTime > mlt) 
            {
                //time it will take the supplier to process the product by skipping weekends
                /* EX: if the day is Wednesday and the delivery time is four days, 
                then it serves 3 days in the first week and skips Saturday and Sunday 
                and continues on Monday  */
                var weeksCount = leadTime / 5;
                if (orderDate.DayOfWeek + leadTime == DayOfWeek.Saturday)
                    orderLeadTime = orderLeadTime.AddDays(1 * weeksCount);
                else if (orderDate.DayOfWeek + leadTime - 1 > DayOfWeek.Friday)
                    orderLeadTime = orderLeadTime.AddDays(2 * weeksCount);

                mlt = orderLeadTime;
            }
        }
        if (mlt.DayOfWeek == DayOfWeek.Saturday)
            return new DespatchDate { Date = mlt.AddDays(2) };

        else if (mlt.DayOfWeek == DayOfWeek.Sunday) 
            return new DespatchDate { Date = mlt.AddDays(1) };

        else 
            return new DespatchDate { Date = mlt };
    }
}
