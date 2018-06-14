using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace DeploymentBotLambda
{
    public class DeploymentInfo
    {
        public DeploymentEnvironments? DeploymentEnvironment { get; set; }

        public string DeploymentTime { get; set; }

        public string DeploymentDate { get; set; }

        public enum DeploymentEnvironments
        {
            Alpha,
            Beta,
            Gamma,
            Production,
            Null
        }

        [JsonIgnore]
        public bool HasRequiredFields
        {
            get
            {
                return !string.IsNullOrEmpty(DeploymentDate)
                        && !string.IsNullOrEmpty(DeploymentTime)
                        && !string.IsNullOrEmpty(DeploymentEnvironment.ToString());

            }
        }
    }
}
