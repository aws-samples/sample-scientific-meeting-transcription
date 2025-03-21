// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Types;
using Common.Types.CustomModels;
using Common.Types.Meetings;
using Common.Types.Prompts;
using Common.Types.PromptSets;
using Common.Types.Teams;
using Xunit;
using Xunit.Abstractions;

namespace exscribo.Tests
{
    [TestCaseOrderer(
        ordererTypeName: "exscribo.Tests.AlphabeticalOrderer",
        ordererAssemblyName: "exscribo.Tests")]
    public class IntegrationTests
    {
        public static bool Test_1Called;
        public static bool Test_2Called;
        public static bool Test_3Called;
        public static bool Test_4Called;
        public static bool Test_5Called;
        public static bool Test_6Called;

        private string? _idToken;
        private Guid? _teamId;
        private Guid? _promptSetID;
        private ITestOutputHelper testOutputHelper;

        public IntegrationTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Test_1()
        {
            GlobalVariables.HttpClient = new HttpClient()
            {
                //BaseAddress = new Uri("https://sxu695fnrd.execute-api.us-east-1.amazonaws.com/prod/"),
                BaseAddress = new Uri("http://localhost:5154/")
            };
            // Arrange
            var loginRequest = new CognitoLoginRequestType
            {
                Username = "admin",
                Password = "Qwerty!234"
            };

            // Act

            var response = await GlobalVariables.HttpClient.PostAsync("auth",
                new StringContent(
                    JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8,
                    "application/json"));

            // Assert
            var test = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var authFlowResponse = await response.Content.ReadFromJsonAsync<AuthResponseType>();
            Assert.NotNull(authFlowResponse);
            Assert.NotNull(authFlowResponse.idToken);

            _idToken = authFlowResponse.idToken;
            GlobalVariables.HttpClient.DefaultRequestHeaders.Add("Authorization", _idToken);

            Test_1Called = true;
        }

        [Fact]
        public async Task Test_2()
        {
            var createTeamRequest = new TeamRequestType
            {
                Team = "Test Team",
                IdpGroup = "idpgroup1"
            };

            var createResponse = await GlobalVariables.HttpClient.PostAsync("teams",
                new StringContent(
                    JsonSerializer.Serialize(createTeamRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var teamInformation = await createResponse.Content.ReadFromJsonAsync<TeamResponseType>();
            Assert.NotNull(teamInformation);

            GlobalVariables.SessionTeamId = teamInformation.Id;


            // Test GET team
            var getResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var teamresponse = await getResponse.Content.ReadFromJsonAsync<TeamResponseType>();
            Assert.NotNull(teamresponse);
            Assert.Equal(GlobalVariables.SessionTeamId, teamresponse.Id);

            // Test GET all teams
            var getAllResponse = await GlobalVariables.HttpClient.GetAsync("teams?pageIndex=3&pageSize=10");


            Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
            var getAllTeamObject = await getAllResponse.Content.ReadFromJsonAsync<PaginatedResponseList<TeamResponseType>>();
            Assert.NotNull(getAllTeamObject);

            // Test UPDATE team
            var updateTeamRequest = new TeamRequestType
            {
                Team = "Updated Test Team",
            };

            var updateResponse = await GlobalVariables.HttpClient.PutAsync($"teams/{GlobalVariables.SessionTeamId}",
                new StringContent(
                    JsonSerializer.Serialize(updateTeamRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            Test_2Called = true;
        }

        [Fact]
        public async Task Test_3()
        {
            Assert.NotNull(GlobalVariables.SessionTeamId);

            // Test CREATE custom model
            var createModelRequest = new CustomModelRequestType
            {
                ModelName = "Test Model",
                Description = "Test custom model",
            };

            var createResponse = await GlobalVariables.HttpClient.PostAsync($"teams/{GlobalVariables.SessionTeamId}/customModels",
                new StringContent(
                    JsonSerializer.Serialize(createModelRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var createdModel = await createResponse.Content.ReadFromJsonAsync<CustomModelResponseType>();
            Assert.NotNull(createdModel);

            // Test GET all custom models
            var getAllResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/customModels?pageIndex=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

            var test = getAllResponse.Content.ReadAsStringAsync().Result;


            var models = await getAllResponse.Content.ReadFromJsonAsync<PaginatedResponseList<CustomModelResponseType>>();
            Assert.NotNull(models);

            // Test GET specific custom model
            var getResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/customModels/{createdModel.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var model = await getResponse.Content.ReadFromJsonAsync<CustomModelResponseType>();
            Assert.NotNull(model);

            // Test UPDATE custom model
            var updateModelRequest = new CustomModelRequestType
            {
                ModelName = "Updated Test Model",
                Description = "Updated test custom model",
            };

            var updateResponse = await GlobalVariables.HttpClient.PutAsync($"teams/{GlobalVariables.SessionTeamId}/customModels/{createdModel.Id}",
                new StringContent(
                    JsonSerializer.Serialize(updateModelRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            // var createSignedUrlResponse = await GlobalVariables.httpClient.GetAsync($"teams/{GlobalVariables.sessionTeamId}/customModels/{createdModel.Id}/create_signed_url");
            // Assert.Equal(HttpStatusCode.OK, createSignedUrlResponse.StatusCode);
            //
            // var url = await createSignedUrlResponse.Content.ReadFromJsonAsync<CustomModelResponseType>();
            // testOutputHelper.WriteLine($"Pre-Signed URL: {url.PreSignedUrl}");


            var startTrainingResponse = await GlobalVariables.HttpClient.PutAsync($"teams/{GlobalVariables.SessionTeamId}/customModels/{createdModel.Id}/start_model_training", null);
            Assert.Equal(HttpStatusCode.OK, startTrainingResponse.StatusCode);

            // Test DELETE custom model
            var deleteResponse = await GlobalVariables.HttpClient.DeleteAsync($"teams/{GlobalVariables.SessionTeamId}/customModels/{createdModel.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);


            Test_3Called = true;
        }

        [Fact]
        public async Task Test_4()
        {
            Assert.NotNull(GlobalVariables.SessionTeamId);

            // Test CREATE prompt set
            var createPromptSetRequest = new PromptSetRequestType
            {
                PromptSetName = "Test Prompt Set",
                Description = "Test prompt set for integration testing",
            };

            var createResponse = await GlobalVariables.HttpClient.PostAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets",
                new StringContent(
                    JsonSerializer.Serialize(createPromptSetRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var createdPromptSet = await createResponse.Content.ReadFromJsonAsync<PromptSetResponseType>();

            Assert.NotNull(createdPromptSet);

            GlobalVariables.PromptSetId = createdPromptSet.Id;

            // Test GET all prompt sets
            var getAllResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets?pageIndex=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
            var promptSets = await getAllResponse.Content.ReadFromJsonAsync<PaginatedResponseList<PromptSetResponseType>>();
            Assert.NotNull(promptSets);

            // Test GET specific prompt set
            var getResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets/{createdPromptSet.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var promptSet = await getResponse.Content.ReadFromJsonAsync<PromptSetResponseType>();
            Assert.NotNull(promptSet);

            // Test UPDATE prompt set
            var updatePromptSetRequest = new PromptSetRequestType
            {
                PromptSetName = "Updated Test Prompt Set",
                Description = "Updated test prompt set",
            };

            var updateResponse = await GlobalVariables.HttpClient.PutAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets/{createdPromptSet.Id}",
                new StringContent(
                    JsonSerializer.Serialize(updatePromptSetRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updatedPromptSet = await updateResponse.Content.ReadFromJsonAsync<PromptSetResponseType>();
            Assert.NotNull(updatedPromptSet);
            Assert.Equal(updatedPromptSet.PromptSetName, updatePromptSetRequest.PromptSetName);


            Test_4Called = true;
        }

        [Fact]
        public async Task Test_5()
        {
            Assert.NotNull(GlobalVariables.SessionTeamId);
            Assert.NotNull(GlobalVariables.PromptSetId);

            // Test CREATE prompt set
            var createPromptRequest = new PromptRequestType()
            {
                Prompt = "What is the actions items of this meeting",
                Description = "Prompt to gather all the action items of the meeting",
            };

            var createResponse = await GlobalVariables.HttpClient.PostAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets/{GlobalVariables.PromptSetId}/prompts",
                new StringContent(
                    JsonSerializer.Serialize(createPromptRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var createdPrompt = await createResponse.Content.ReadFromJsonAsync<PromptResponseType>();

            Assert.NotNull(createdPrompt);

            // Test GET all prompt sets
            var getAllResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets/{GlobalVariables.PromptSetId}/prompts?pageIndex=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
            var prompts = await getAllResponse.Content.ReadFromJsonAsync<PaginatedResponseList<PromptResponseType>>();
            Assert.NotNull(prompts.Records);

            // Test GET specific prompt set
            var getResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets/{GlobalVariables.PromptSetId}/prompts/{createdPrompt.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var prompt = await getResponse.Content.ReadFromJsonAsync<PromptResponseType>();
            Assert.NotNull(prompt);

            // Test UPDATE prompt set
            var updatePromptRequest = new PromptRequestType
            {
                Prompt = "What is the actions items of this meeting - Updated",
            };

            var updateResponse = await GlobalVariables.HttpClient.PutAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets/{GlobalVariables.PromptSetId}/prompts/{createdPrompt.Id}",
                new StringContent(
                    JsonSerializer.Serialize(updatePromptRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updatedPrompt = await updateResponse.Content.ReadFromJsonAsync<PromptResponseType>();
            Assert.NotNull(updatedPrompt.Prompt);
            Assert.Equal(updatedPrompt.Prompt, updatePromptRequest.Prompt);

            // Test DELETE prompt set
            var deleteResponse = await GlobalVariables.HttpClient.DeleteAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets/{GlobalVariables.PromptSetId}/prompts/{createdPrompt.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);


            //CLeanup
            // Test DELETE prompt set
            var deletePrompSetResponse = await GlobalVariables.HttpClient.DeleteAsync($"teams/{GlobalVariables.SessionTeamId}/promptSets/{GlobalVariables.PromptSetId}");
            Assert.Equal(HttpStatusCode.NoContent, deletePrompSetResponse.StatusCode);

            Test_5Called = true;
        }

        [Fact]
        public async Task Test_6()
        {
            Assert.NotNull(GlobalVariables.SessionTeamId);

            // Test CREATE prompt set
            var createMeetingtRequest = new MeetingRequestType()
            {
                Title = "This is a test meeting",
                Description = "Prompt to gather all the action items of the meeting",
            };

            var createResponse = await GlobalVariables.HttpClient.PostAsync($"teams/{GlobalVariables.SessionTeamId}/meetings",
                new StringContent(
                    JsonSerializer.Serialize(createMeetingtRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            var createdMeeting = await createResponse.Content.ReadFromJsonAsync<MeetingResponseType>();
            Assert.NotNull(createdMeeting);
            var createSignedUrlResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/meetings/{createdMeeting.Id}/create_signed_url");
            var url = createSignedUrlResponse.Content.ReadAsStringAsync().Result;
            testOutputHelper.WriteLine($"PrSignedURL: {url}");
            Assert.Equal(HttpStatusCode.OK, createSignedUrlResponse.StatusCode);


            // Test GET all prompt sets
            var getAllResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/meetings?pageIndex=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
            var meetings = await getAllResponse.Content.ReadFromJsonAsync<PaginatedResponseList<MeetingResponseType>>();
            Assert.NotNull(meetings.Records);

            // Test GET specific prompt set
            var getResponse = await GlobalVariables.HttpClient.GetAsync($"teams/{GlobalVariables.SessionTeamId}/meetings/{createdMeeting.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var prompt = await getResponse.Content.ReadFromJsonAsync<MeetingResponseType>();
            Assert.NotNull(prompt);

            // Test UPDATE prompt set
            var updateMeetingRequest = new MeetingRequestType()
            {
                Title = "This is a test meeting - Updated",
            };

            var updateResponse = await GlobalVariables.HttpClient.PutAsync($"teams/{GlobalVariables.SessionTeamId}/meetings/{createdMeeting.Id}",
                new StringContent(
                    JsonSerializer.Serialize(updateMeetingRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updatedPrompt = await updateResponse.Content.ReadFromJsonAsync<MeetingResponseType>();
            Assert.NotNull(updatedPrompt.Title);
            Assert.Equal(updatedPrompt.Title, updateMeetingRequest.Title);

            // Test DELETE prompt set
            // var deleteResponse = await GlobalVariables.httpClient.DeleteAsync($"teams/{GlobalVariables.sessionTeamId}/meetings/{createdMeeting.Id}");
            // Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);


            Test_6Called = true;
        }
    }
}