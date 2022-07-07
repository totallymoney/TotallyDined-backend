module AWS

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model
open Amazon.DynamoDBv2.DocumentModel
open Utils
open Types

module DynamoDB =
    open System

    let getClient =
        match System.Environment.GetEnvironmentVariable "ENVIRONMENT" with
        | "test" ->
            new AmazonDynamoDBClient(
                AmazonDynamoDBConfig(ServiceURL = System.Environment.GetEnvironmentVariable "DYNAMODB_URL")
            )
        | _ -> new AmazonDynamoDBClient()

    let put (client: AmazonDynamoDBClient) (tableName: string) record : Result<unit, ErrorType> =
        let table = Table.LoadTable(client, tableName)
        let json = serialize record

        try
            table.PutItemAsync(Document.FromJson(json))
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore

            Ok()
        with
        | (ex: Exception) -> Error(DynamoDBPutError ex.Message)


    let delete (client: AmazonDynamoDBClient) (deleteRequest: DeleteItemRequest) =
        try
            client.DeleteItemAsync(deleteRequest)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore

            Ok()
        with
        | (ex: Exception) -> Error(DynamoDBPutError ex.Message)


    let get<'a> (client: AmazonDynamoDBClient) (getRequest: GetItemRequest) =
        try
            let response =
                client.GetItemAsync(getRequest)
                |> Async.AwaitTask
                |> Async.RunSynchronously

            Document.FromAttributeMap(response.Item).ToJson()
            |> deserialize<'a>

        with
        | (ex: Exception) -> Error(DynamoDBPutError ex.Message)

    open System.Collections.Generic

    let query<'a> (client: AmazonDynamoDBClient) (tableName: string) (partitionKey: string) (sortKeyPrefix: string) =
        try
            let table = Table.LoadTable(client, tableName)

            let queryFilter =
                QueryFilter("PartitionKey", QueryOperator.Equal, [ AttributeValue(partitionKey) ] |> List)

            queryFilter.AddCondition("SortKey", QueryOperator.BeginsWith, [ AttributeValue(sortKeyPrefix) ] |> List)

            table.Query(queryFilter).GetNextSetAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> Seq.map (fun x -> x.ToJson() |> deserialize<'a>)
            |> List.ofSeq
            |> Ok
        with
        | (ex: Exception) -> Error(DynamoDBPutError ex.Message)

open Amazon.SQS
open Amazon.SQS.Model

module SQS =
    open System

    let getClient = new AmazonSQSClient()

    let sendBatch (client: AmazonSQSClient) (queueName: string) (messages: 'a list) =
        let entries =
            messages
            |> List.map (fun m -> SendMessageBatchRequestEntry(Guid.NewGuid().ToString(), serialize m))
            |> System.Collections.Generic.List

        let request = SendMessageBatchRequest(queueName, entries)

        client.SendMessageBatchAsync(request)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore

        Ok()
