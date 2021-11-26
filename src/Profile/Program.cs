using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Program
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new ProfileStack(app, "ProfileStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = "334575280890",
                    Region="ap-northeast-1"
                }
            });

            app.Synth();
        }
    }
}
