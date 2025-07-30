using Grpc.Core;
using Xperim;
using System.Threading.Tasks;


namespace GrpcServer.Services
{
    public class StudyService : Xperim.StudyService.StudyServiceBase
    {
        public override Task<GenerateFhirBundleResponse> GenerateFhirBundle(
            GenerateFhirBundleRequest request, 
            ServerCallContext context)
        {
            var response = new GenerateFhirBundleResponse
            {
                BundleJson = "{ \"bundle\": \"fhir data for " + request.FirstName + " " + request.LastName + "\" }"
            };

            return Task.FromResult(response);
        }
    }
}
