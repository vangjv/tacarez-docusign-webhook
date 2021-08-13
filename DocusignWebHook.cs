using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace tacarez_docusign_webhook
{
    public class DocusignWebHook
    {
        [FunctionName("DocusignWebHook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events")] HttpRequest req,
            ILogger log)
        {
            var headers = req.Headers;
            //StringValues authValue;
            //bool headerExist = headers.TryGetValue("Authorization", out authValue);
            //if (authValue.ToString().Contains(System.Environment.GetEnvironmentVariable("basicAuthToken")))
            //{
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                CosmosHelper ch = new CosmosHelper();
                try
                {
                    await ch.WriteEventDataToDatabase(data);
                }
                catch (Exception e)
                {

                    throw e;
                }
                try
                {
                    await ch.UpdateEnvelopeStatus(data);
                }
                catch (Exception e)
                {
                    throw e;
                }
                
            //}       
            return new OkObjectResult("");
        }
    }
}
