using Amazon.CodePipeline;
using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static DeploymentBotLambda.DeploymentInfo;

namespace DeploymentBotLambda
{
    public class DeploymentIntentProcessor : AbstractIntentProcessor
    {
        public const string ENVIRONMENT_SLOT = "EnvironmentOption";
        public const string DEPLOYMENT_DATE_SLOT = "DeploymentDate";
        public const string DEPLOYMENT_TIME_SLOT = "DeploymentTime";
        public const string INVOCATION_SOURCE = "invocationSource";
        DeploymentEnvironments _chosenEnvironment = DeploymentEnvironments.Null;

        /// <summary>
        /// Performs dialog management and fulfillment for ordering flowers.
        /// 
        /// Beyond fulfillment, the implementation for this intent demonstrates the following:
        /// 1) Use of elicitSlot in slot validation and re-prompting
        /// 2) Use of sessionAttributes to pass information that can be used to guide the conversation
        /// </summary>
        /// <param name="lexEvent"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override LexResponse Process(LexEvent lexEvent, ILambdaContext context)
        {
            IDictionary<string, string> slots = lexEvent.CurrentIntent.Slots;
            IDictionary<string, string> sessionAttributes = lexEvent.SessionAttributes ?? new Dictionary<string, string>();

            //if all the values in the slots are empty return the delegate, theres nothing to validate or do.
            if (slots.All(x => x.Value == null))
            {
                return Delegate(sessionAttributes, slots);
            }
            
            //if the deployment environment slot has a value, validate that it is contained within the enum list available.
            if (slots[ENVIRONMENT_SLOT] != null)
            {
                var validateDeploymentEnvironment = ValidateDeploymentEnvironment(slots[ENVIRONMENT_SLOT]);

                if (!validateDeploymentEnvironment.IsValid)
                {
                    slots[validateDeploymentEnvironment.ViolationSlot] = null;
                    return ElicitSlot(sessionAttributes, lexEvent.CurrentIntent.Name, slots, validateDeploymentEnvironment.ViolationSlot, validateDeploymentEnvironment.Message);
                }
            }

            //now that enum has been parsed and validated, create the deployment
            DeploymentInfo deploymentInfo = CreateDeployment(slots);

            if (string.Equals(lexEvent.InvocationSource, "DialogCodeHook", StringComparison.Ordinal))
            {
                //validate the remaining slots.
                var validateResult = Validate(deploymentInfo);
                // If any slots are invalid, re-elicit for their value
                
                if (!validateResult.IsValid)
                {
                    slots[validateResult.ViolationSlot] = null;
                    return ElicitSlot(sessionAttributes, lexEvent.CurrentIntent.Name, slots, validateResult.ViolationSlot, validateResult.Message);
                }

                return Delegate(sessionAttributes, slots);
            }

            Task.WaitAll(InvokePipeline());

            return Close(
                        sessionAttributes,
                        "Fulfilled",
                        new LexResponse.LexMessage
                        {
                            ContentType = MESSAGE_CONTENT_TYPE,
                            Content = String.Format("Alright, your deployment in {0} environment has been scheduled at {1} on {2}.",deploymentInfo.DeploymentEnvironment.ToString(), deploymentInfo.DeploymentTime, deploymentInfo.DeploymentDate)
                        }
                    );
        }

        private static async Task InvokePipeline()
        {
            await new AmazonCodePipelineClient().StartPipelineExecutionAsync("MyFirstPipeline");
        }

        private DeploymentInfo CreateDeployment(IDictionary<string, string> slots)
        {
            DeploymentInfo deploymentInfo = new DeploymentInfo
            {
                DeploymentEnvironment = _chosenEnvironment,
                DeploymentDate = slots.ContainsKey(DEPLOYMENT_DATE_SLOT) ? slots[DEPLOYMENT_DATE_SLOT] : null,
                DeploymentTime = slots.ContainsKey(DEPLOYMENT_TIME_SLOT) ? slots[DEPLOYMENT_TIME_SLOT] : null
            };

            return deploymentInfo;
        }

        /// <summary>
        /// Verifies that any values for slots in the intent are valid.
        /// </summary>
        /// <param name="deploymentInfo"></param>
        /// <returns></returns>
        private ValidationResult Validate(DeploymentInfo deploymentInfo)
        {

            if (!string.IsNullOrEmpty(deploymentInfo.DeploymentDate))
            {
                DateTime deploymentDate = DateTime.MinValue;
                if (!DateTime.TryParse(deploymentInfo.DeploymentDate, out deploymentDate))
                {
                    return new ValidationResult(false, DEPLOYMENT_DATE_SLOT,
                        "I did not understand that, what date would you like to schedule the deployment?");
                }
                if (deploymentDate < DateTime.Today)
                {
                    return new ValidationResult(false, DEPLOYMENT_DATE_SLOT,
                        "Deployment date can only be in the future. What date do you want me to schedule the deployment?");
                }
            }

            if (!string.IsNullOrEmpty(deploymentInfo.DeploymentTime))
            {
                string[] timeComponents = deploymentInfo.DeploymentTime.Split(":");
                Double hour = Double.Parse(timeComponents[0]);
                Double minutes = Double.Parse(timeComponents[1]);

                if (Double.IsNaN(hour) || Double.IsNaN(minutes))
                {
                    return new ValidationResult(false, DEPLOYMENT_TIME_SLOT, null);
                }

                //if (hour < 10 || hour >= 17)
                //{
                //    return new ValidationResult(false, DEPLOYMENT_TIME_SLOT, "Our business hours are from ten a m. to five p m. Can you specify a time during this range?");
                //}

            }

            return ValidationResult.VALID_RESULT;
        }

        /// <summary>
        /// Verifies that any values for flower type slot in the intent is valid.
        /// </summary>
        /// <param name="flowertypeString"></param>
        /// <returns></returns>
        private ValidationResult ValidateDeploymentEnvironment(string deploymentEnvironment)
        {
            bool isFlowerTypeValid = Enum.IsDefined(typeof(DeploymentEnvironments), deploymentEnvironment.ToUpper());

            if (Enum.TryParse(typeof(DeploymentEnvironments), deploymentEnvironment, true, out object deploymentTypeObject))
            {
                _chosenEnvironment = (DeploymentEnvironments)deploymentTypeObject;
                return ValidationResult.VALID_RESULT;
            }
            else
            {
                return new ValidationResult(false, ENVIRONMENT_SLOT, String.Format("Sorry, '{0}' environment isn't supported. Pick one from Alpha, Beta, Gamma and Production", deploymentEnvironment));
            }
        }

    }
}
