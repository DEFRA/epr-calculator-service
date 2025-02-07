﻿using EPR.Calculator.Service.Function.Data.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Data.DataModels
{
    [Table("calculator_run_csvfile_metadata")]

    public class CalculatorRunCsvFileMetadata
    {
        [Column("id")]
        [Required]
        public int Id { get; set; }

        [Column("filename")]
        [StringLength(500)]
        [Required]
        public required string FileName { get; set; }

        [Column("blob_uri")]
        [StringLength(2000)]
        public required string BlobUri { get; set; }

        [Column("calculator_run_id")]
        public required int CalculatorRunId { get; set; }

        public virtual CalculatorRun? CalculatorRun { get; set; }
    }
}
