﻿using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EPR.Calculator.API.Wrapper
{
    public class OrgAndPomWrapper : IOrgAndPomWrapper
    {
        private readonly ApplicationDBContext context;
        public OrgAndPomWrapper(ApplicationDBContext context) { this.context = context; }

        public bool AnyOrganisationData()
        {
            return this.context.OrganisationData.Any();
        }

        public bool AnyPomData()
        {
            return this.context.PomData.Any();
        }

        public async Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken)
            => await this.context.Database.ExecuteSqlAsync(sql, cancellationToken);

        public async Task<IEnumerable<OrganisationData>> GetOrganisationDataAsync()
            => await this.context.OrganisationData.ToListAsync();

        public async Task<IEnumerable<PomData>> GetPomDataAsync()
            => await this.context.PomData.ToListAsync();
    }
}
