using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{

    public class CalcCountryApportionmentService : ICalcCountryApportionmentService
    {
        private readonly ApplicationDBContext context;
        public CalcCountryApportionmentService(ApplicationDBContext context)
        {
            this.context = context; 
        }

        public async Task SaveChangesAsync(CalcCountryApportionmentServiceDto countryApportionmentServiceDto)
        {
            this.context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = countryApportionmentServiceDto.RunId,
                CountryId = countryApportionmentServiceDto.Countries.Single(x => x.Name == "England").Id,
                CostTypeId = countryApportionmentServiceDto.CostTypeId,
                Apportionment = countryApportionmentServiceDto.EnglandCost,
            });

            this.context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = countryApportionmentServiceDto.RunId,
                CountryId = countryApportionmentServiceDto.Countries.Single(x => x.Name == "Wales").Id,
                CostTypeId = countryApportionmentServiceDto.CostTypeId,
                Apportionment = countryApportionmentServiceDto.WalesCost,
            });

            this.context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = countryApportionmentServiceDto.RunId,
                CountryId = countryApportionmentServiceDto.Countries.Single(x => x.Name == "Northern Ireland").Id,
                CostTypeId = countryApportionmentServiceDto.CostTypeId,
                Apportionment = countryApportionmentServiceDto.NorthernIrelandCost,
            });

            this.context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = countryApportionmentServiceDto.RunId,
                CountryId = countryApportionmentServiceDto.Countries.Single(x => x.Name == "Scotland").Id,
                CostTypeId = countryApportionmentServiceDto.CostTypeId,
                Apportionment = countryApportionmentServiceDto.ScotlandCost,
            });

            await this.context.SaveChangesAsync();
        }   
    }
}
