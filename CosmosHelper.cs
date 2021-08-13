using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tacarez_docusign_webhook.Models;

namespace tacarez_docusign_webhook
{
    public class CosmosHelper
    {
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;
        // The name of the database and container we will create
        string databaseName = System.Environment.GetEnvironmentVariable("Database");
        string containerName = System.Environment.GetEnvironmentVariable("Container");
        string endpointUri = System.Environment.GetEnvironmentVariable("endpointUri");
        string pkey = System.Environment.GetEnvironmentVariable("pkey");
        public CosmosHelper()
        {
            
        }
        public async Task WriteEventDataToDatabase(dynamic docusignBody)
        {
            _cosmosClient = new CosmosClient(endpointUri, pkey, new CosmosClientOptions() { ApplicationName = "TacarEZ Docusign Webhook" });
            _database = _cosmosClient.GetDatabase(databaseName);
            _container = _database.GetContainer(containerName);
            string envelopeId = docusignBody.envelopeId;
            if (envelopeId != null)
            {
                //check if exist
                var sqlQueryText = "SELECT * FROM c WHERE c.type = 'docusign' AND c.envelopeId = '" + envelopeId + "'";
                //ItemResponse<dynamic> searchResponse = await _container.ReadItemAsync<dynamic>(docusignBody.envelopeId, new PartitionKey("docusign"));
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<dynamic> queryResultSetIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);

                List<dynamic> dsEvents = new List<dynamic>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<dynamic> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (dynamic dsevent in currentResultSet)
                    {
                        dsEvents.Add(dsevent);
                        Console.WriteLine("\tRead {0}\n", dsevent);
                    }
                }
               
                //envelope already exist
                 
                if (dsEvents.Count > 0)
                {
                    //add event
                    dynamic envelopeEvents = dsEvents[0];
                    envelopeEvents.eventHistory.Add(docusignBody);
                    var replaceItemResponse = await _container.ReplaceItemAsync<dynamic>(envelopeEvents, envelopeId, new PartitionKey("docusign"));
                }
                else
                {
                    DocuSignEnvelopeEvents newDSEnvelopeEvent = new DocuSignEnvelopeEvents
                    {
                        id = envelopeId,
                        envelopeId = envelopeId,
                        type = "docusign",
                        eventHistory = new List<dynamic>()
                    };
                    newDSEnvelopeEvent.eventHistory.Add(docusignBody);
                    await _container.CreateItemAsync<DocuSignEnvelopeEvents>(newDSEnvelopeEvent, new PartitionKey("docusign"));
                }
            }
            else
            {
                throw new Exception("No envelope id in docusign message");
            }
        }

        public async Task UpdateEnvelopeStatus(dynamic docusignBody)
        {
            _cosmosClient = new CosmosClient(endpointUri, pkey, new CosmosClientOptions() { ApplicationName = "TacarEZ Docusign Webhook" });
            _database = _cosmosClient.GetDatabase(databaseName);
            _container = _database.GetContainer("features");
            string envelopeId = docusignBody.envelopeId;
            //get merge request where envelope id matches
            var sqlQueryText = "SELECT * FROM c WHERE c.type = 'merge' AND c.StakeholderReview.EnvelopeId = '" + envelopeId + "'";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<MergeRequest> queryResultSetIterator = _container.GetItemQueryIterator<MergeRequest>(queryDefinition);
            List<MergeRequest> mergeRequests = new List<MergeRequest>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<MergeRequest> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (MergeRequest merge in currentResultSet)
                {
                    mergeRequests.Add(merge);
                    Console.WriteLine("\tRead {0}\n", merge);
                }
            }
            if (mergeRequests.Count > 0)
            {
                MergeRequest mergeRequest = mergeRequests[0];
                //create RecipientStatus
                List<RecipientStatus> recipientStatusList = new List<RecipientStatus>();
                IEnumerable<dynamic> signers = docusignBody.recipients.signers;
                List<dynamic> signersList = signers.ToList();
                signersList.ForEach(signer =>
                {
                    RecipientStatus newRecipientStatus = new RecipientStatus();
                    newRecipientStatus.RecipientName = signer.name;
                    newRecipientStatus.Email = signer.email;
                    newRecipientStatus.InitialSentDateTime = docusignBody.initialSentDateTime;
                    newRecipientStatus.SignedDateTime = signer.signedDateTime;
                    newRecipientStatus.DeliveredDateTime = signer.deliveredDateTime;
                    recipientStatusList.Add(newRecipientStatus);
                });

                mergeRequest.StakeholderReview.EnvelopeStatus = recipientStatusList;
                mergeRequest.LastModifiedDate = DateTime.Now;
                var replaceItemResponse = await _container.ReplaceItemAsync<MergeRequest>(mergeRequest, mergeRequest.Id, new PartitionKey("merge"));
            }
            else
            {
                throw new Exception("No envelope id in docusign message");
            }
        }
    }
}
