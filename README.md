## This application uses the following AWS services:

* [Lex](https://docs.aws.amazon.com/lex/latest/dg/what-is.html)

* [Polly](https://docs.aws.amazon.com/polly/latest/dg/what-is.html) 

* [Rekognition](https://docs.aws.amazon.com/rekognition/latest/dg/what-is.html)

* [Lambda](https://docs.aws.amazon.com/lambda/latest/dg/welcome.html)

* [CodePipeline](https://docs.aws.amazon.com/codepipeline/latest/userguide/welcome.html)

## Application Design:

`insert an image here`

## Follow these steps to create the application on your own

### Step 1 - Create Code Pipeline 

* Follow [this link](https://docs.aws.amazon.com/codepipeline/latest/userguide/tutorials-simple-codecommit.html) to setup the pipeline which we will use later
* Make sure you name the pipeline as _MyFirstPipeline_. If you name it something else, make sure you remember to make necessary changes to the Lambda function which we will create in future steps

### Step 2 - Create the Lambda function

* Create an IAM role called __Lambda_DeploymentBot__ and assign the following policies to it
    * AWSLambdaFullAccess
    * AWSCodePipelineFullAccess

* Clone [this](https://github.com/awsimaya/DeploymentBot) GitHub repo
* Open __DeploymentBotLambda.sln__ under _DeploymentBotLambda_ folder using Visual Studio
* Make sure the project compiles successfully
* Right click on the project and select __Publish to AWS Lambda...__
* Name the lambda __DeploymentBotLambda__ and click __Next__
* Select the role __Lambda_DeploymentBot__ and click __Upload__ which will publish the lambda to AWS


### Step 3 - Create the Lex Bot

* Go to Amazon Lex home page on your AWS Console
* Click on the __Create__ button and then click on __Custom bot__ in the next screen
* Name the bot as __DeploymentBot__ and select __None__ in the Output voice drop down.
* Enter __5__ for the timeout textbox
* Choose __Yes__ for COPPA and click __Create__
* Create a slot and name it __Environment__. Add __Alpha, Beta, Gamma, Production__ as values
* Click on __Create Intent__ button under the _Editor_ tab and give it a name in the popup screen
* Add the following utterances
    * Start Deployment
    * I want to start a deployment
* Add the following slots


| Name      | Slot Type | Prompt |
| ----------- | ----------- | ----|
| EnvironmentOption | Environment | Which environment do you want to deploy to? (Alpha, Beta, Gamma, Production) |
| DeploymentDate   | AMAZON.DATE        |What date do you want to schedule the deployment for? |
| DeploymentTime | AMAZON.TIME | What time do you want to schedule the deployment? |

* Uncheck the __Confirmation Prompt__ checkbox if it is selected already
* Select __AWS Lambda function__ under _Fulfillment_, select __DeploymentBotLambda__ in the _Lambda function_ dropdown and select __Latest__ for _Version or alias_ dropdown
* Now, __Buld__ and then __Publish__ the bot

### Step 4 - Create the Chatbot WebApp
* Create a Cognito Federated Identity Pool by following the below steps
    * From the AWS Console click Services.
    * Choose Services from the top menu bar and then search for "Cognito" and select it.
    * Choose Manage Identity Pools.
    * Choose Create New Identity Pool.
* Configure the Pool for the app by following the below steps
    * Choose a name for your pool 
    * Check the box that says Enable access to unauthenticated identities.
    * Hit the blue button that says Create Pool.
    * On the next screen, allow the default roles that AWS will create for you by clicking Allow.
    * Once the Amazon Cognito Federated Identity Pool has been created, Cognito will show sample code and the Pool ID. Copy the Pool ID and save it where you can easily retrieve it.
* Open __DeploymentBotWeb.sln__ under _DeploymentBotWeb_ from the cloned folder
* Set appropriate AWS credential values in _Startup.cs_ 

    `Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "<Your Amazon Access Key Id>");`

    `Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "<Your Amazon Secret Access Key>");`

    `Environment.SetEnvironmentVariable("AWS_REGION", "<Your Region Name>");`
    
* Set appropriate values for the Lexbot under _appsettings.json_

    `"AWSConfiguration": {
    "CognitoPoolID": "<Cognito Pool ID>",
    "LexBotName": "<Lex Bot Name>",
    "LexBotAlias": "<Lex Bot Alias>",
    "BotRegion": "<Your Region>"
    }`

* Build and run the Project

## How to use the app

* On the _Login_ screen, upload a picture of __Jeff Bezos__
* __Amazon Rekognition__ will identify him and respond with a confirmation. Also, the application will make an audio confirmation using __Amazon Polly __
* Click on the link that takes you to the _Chat_ Page
* Send a message saying __Start Deployment__
* Continue answering questions from the chatbot
* Once the chatbot has all the inputs, it will trigger the __Codepipeline__ right away. Go to the __Code Pipeline__ console to check the status of your deployment
