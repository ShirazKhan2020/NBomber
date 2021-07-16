using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBomber.Contracts;
using NBomber.CSharp;

namespace StringLibraryTest
{
    [TestClass]
    public partial class NBomber
    {

        private string uri = "https://kwshiraz-eval-test.apigee.net/spikearresttest831pm";
        public void LoadRequest()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            var scenario = BuildScenarioMultiRequest(methodName)
            .WithoutWarmUp()

            .WithLoadSimulations(new[]
            {
                        Simulation.InjectPerSec(rate: 2000,during: TimeSpan.FromSeconds(30))
            }
            );
            Task.Delay(TimeSpan.FromSeconds(60-DateTime.Now.Second)).Wait();
            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }

        [TestMethod]
        public void Z_VerifySpikeArrestPolicyInvoked()
        {
            // Task.Delay(TimeSpan.FromSeconds(60-DateTime.Now.Second)).Wait();
            var client = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, uri);
            //LoadRequest();
            var response = client.Send(msg);
            response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        }

        Scenario BuildScenarioMultiRequest(string scenarioName)
        {

            var step = Step.Create(scenarioName, async context =>
            {
                var client = new HttpClient();
                var response = await client.GetAsync(uri);

                return response.IsSuccessStatusCode
                    ? Response.Ok(statusCode: (int)response.StatusCode)
                    : Response.Fail(statusCode: (int)response.StatusCode);
            });
            return ScenarioBuilder.CreateScenario(scenarioName, step);
        }
    }
}
