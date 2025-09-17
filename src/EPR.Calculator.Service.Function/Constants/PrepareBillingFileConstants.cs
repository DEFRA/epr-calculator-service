using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Constants
{
    public class PrepareBillingFileConstants
    {        
        public const string CalculatorRunNotFound = "Prepare Billing File:Calculator run is null";
       
        public const string AcceptedProducerIdsAreNull = "Prepare Billing File: Accepted Producer Ids Count is zero";

        public const string BillingInstructionAccepted = "Accepted";

        public const string IsBillingFileGeneratingNotSet = "Is Billing File Generating is not set";

        public const string SuggestedBillingInstructionCancelBill = "Cancel";
    }
}
