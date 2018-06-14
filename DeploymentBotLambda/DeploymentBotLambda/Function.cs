using System;

using Amazon.Lambda.LexEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DeploymentBotLambda
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public LexResponse FunctionHandler(LexEvent lexEvent, ILambdaContext context)
        {
            IIntentProcessor process;

            if (lexEvent.CurrentIntent.Name == "CreateDeployment")  
            {
                process = new DeploymentIntentProcessor();
            }
            else
            {
                throw new Exception($"Intent with name {lexEvent.CurrentIntent.Name} not supported");
            }

            return process.Process(lexEvent, context);
        }
    }
}
