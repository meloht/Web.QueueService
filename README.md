
# a web request queue gateway service
# Architecture(Traffic control)
Control the number of requests executed simultaneously, set according to PoolSize, and process the number of requests at the same time
Example: PoolSize:100, number of connections: 1000, actual execution number is 100, the remaining 900 connections will be executed after resources are available

<img width="806" height="349" alt="Picture1112" src="https://github.com/user-attachments/assets/1e887df7-15de-438d-ae24-06b9d857bb95" />


## Queue API
The receiving connection puts the Request Json into the Request Queue, waits for the notification from the Response Queue that the execution is completed, obtains the result from the Response Storage, and sends it back to the front end

## Service Bus(Queue)
Store the Request & Response Json

## Queue Worker

Control the number of requests executed simultaneously. After obtaining the Request Json from the Request Queue, call the API (e.g., Backend API) and store the result in Response Storage

## API Management
Manage apis and verify authorizations

# Process
1. send request to QueueAPI
2. After the Queue API places the Request Json in the Request Queue, connect and wait for the notification that the Response Queue has completed its execution
3. The Queue Worker determines the number of requests to be processed simultaneously based on the Pool Size. First, it obtains the Request Json from the Request Queue and then transmits the Request Json to the API pointed to by the Target Url
4. The returned results are stored in Response Storage. The Storage method is determined based on the type of returned results: Json->CosmosDB, File->Blob Storage. At the same time, a Response Queue is sent to notify the completion of execution
5. The Queue API receives the completion notification of the Response Queue, then obtains the result from the Response Storage and returns it to the front end


