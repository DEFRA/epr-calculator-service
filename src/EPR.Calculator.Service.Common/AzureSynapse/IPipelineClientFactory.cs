using Azure.Analytics.Synapse.Artifacts;
using Azure.Core;

public interface IPipelineClientFactory
{
    public PipelineClient GetPipelineClient(Uri pipelineUrl, TokenCredential tokenCredential);
    public PipelineRunClient GetPipelineRunClient(Uri pipelineUrl, TokenCredential tokenCredential);
    public HttpClient GetStatusUpdateClient(Uri statusUpdateUrl);
}