using Azure.Analytics.Synapse.Artifacts;
using Azure.Core;

namespace EPR.Calculator.Service.Common.AzureSynapse
{
    public interface IPipelineClientFactory
    {
        public PipelineClient GetPipelineClient(Uri pipelineUrl, TokenCredential tokenCredential);
        public PipelineRunClient GetPipelineRunClient(Uri pipelineUrl, TokenCredential tokenCredential);
        public HttpClient GetStatusUpdateClient(Uri statusUpdateUrl);
    }
}