﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Service.Function.Data.DataModels
{
    [Table("calculator_run_organization_data_master")]
    public class CalculatorRunOrganisationDataMaster
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("calendar_year")]
        public required string CalendarYear { get; set; }

        [Column("effective_from")]
        public required DateTime EffectiveFrom { get; set; }

        [Column("effective_to")]
        public DateTime? EffectiveTo { get; set; }

        [Column("created_by")]
        public required string CreatedBy { get; set; }

        [Column("created_at")]
        public required DateTime CreatedAt { get; set; }

        public ICollection<CalculatorRunOrganisationDataDetail> Details { get; } = new List<CalculatorRunOrganisationDataDetail>();

        public ICollection<CalculatorRun>? RunDetails { get; }
    }
}
