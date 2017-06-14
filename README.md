# BCNReports
The service is used for:
 * create xlsx reports for blockchain address, save it to azure blobstorage and prodive http API to access reports. Reports are processed using background worker. Optional user can by notified at report done event via email.
 
# Deploy

Put .env file along with the docker-compose.yml
The .env file must define the following environment variables: - SETTINGSURL containing URL that the service must use to acquire settings - LISTENPORT containing port number that the container must expose

# Settings description


```
{
	"NinjaUrl":"", // url for nbitcoin ninja. For more info check this repo: https://github.com/MetacoSA/QBitNinja .
	"Network":"", // blockchain network type (mainnet, testnet, regtest)
	"BlockChainExplolerUrl":"", url for bcn exploler (check https://github.com/LykkeCity/BCNExplorer) .Used for retrieving colored asset metadata
	"Db": { //azure table storage connection strings
		"LogsConnString":"", // for logging purposes
		"DataConnString":"",// xlsx reports stored here
		"SharedConnString":"", // writes "is alive" messages for lykke monitoring system and produces slack notifications
	},
	"ServiceBusEmailSettings": { // settings for https://github.com/LykkeCity/EmailSenderProducer
		"Key": "",
		"QueueName": "",
		"NamespaceUrl": "",
		"PolicyName": ""							
	},	
	"NinjaTransactionsMaxConcurrentRequestCount": 10, // max requests count for concurrent retrieving transaction info (used in semaphore)
	"TimeoutMinutesOnGettingNinjaTransactionsList" : 5 // http request timeout for retrieving transaction id list for particular address.
}

```
