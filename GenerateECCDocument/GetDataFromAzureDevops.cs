using GenerateECCDocument.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GenerateECCDocument
{
	public class GetDataFromAzureDevops
	{
		public static TestPlan GetAllInfo(string testPlanName)
		{
			//In order to get all the needed information from a test plan multiple APIs need to be contacted. See the different module for every API call made.

			var testPlanForExport = LoadTestPlan(testPlanName).Result;
			testPlanForExport.SuitesList = LoadRootTestSuite(testPlanForExport).Result;
			testPlanForExport.RootSuite = LoadRootTestSuiteTreeView(testPlanForExport).Result;

			testPlanForExport = LoadTestCasesForAllTestSuites(testPlanForExport).Result;

			testPlanForExport.TestRuns = LoadTestRunsAndResultsFor(testPlanForExport).Result;

			testPlanForExport = AddTestRunsWithResultsToTestSuitesFromList(testPlanForExport);

			testPlanForExport = AddTestSuitesDataFromListToTreeView(testPlanForExport);

			return testPlanForExport;
		}

		public static async Task<TestPlan> LoadTestPlan(string testPlanName)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/test-plans/list
			//Get a list of all test plans and determine which one should be exported. If no unique test plan can be found with the given input, an error is thrown.

			try
			{
				string url = "test/plans?includePlanDetails=true";
				var response = ApiHelper.ApiClient.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{

					var jsonString = JObject.Parse(await response.Content.ReadAsStringAsync())["value"].ToString();

					List<TestPlan> testPlans = JsonConvert.DeserializeObject<List<TestPlan>>(jsonString);

					List<TestPlan> foundTestPlans = testPlans.FindAll(x => x.Name.Contains(testPlanName));

					if (foundTestPlans.Count() != 1)
					{
						Console.WriteLine("Could not select a unique test plan with the given release name");
						foreach (TestPlan foundTestPlan in foundTestPlans)
						{
							Console.WriteLine("Found test plan: " + foundTestPlan);
						}
						throw new Exception("Could not select a unique test plan with the given release name");
					}

					return foundTestPlans.First();
				}
			}
			catch (Exception e)
			{
				throw e;
			}
			return null;


		}

		public static async Task<List<TestSuite>> LoadRootTestSuite(TestPlan testPlanForExport)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/test-suites/get-test-suites-for-plan
			//Get a flat list of all test suites in a test plan

			string url = $"test/Plans/{testPlanForExport.Id}/suites";
			var response = ApiHelper.ApiClient.GetAsync(url).Result;

			if (response.IsSuccessStatusCode)
			{
				var jsonString = JObject.Parse(await response.Content.ReadAsStringAsync())["value"].ToString();

				List<TestSuite> testSuites = JsonConvert.DeserializeObject<List<TestSuite>>(jsonString);
				return testSuites;
			}
			else
			{
				throw new Exception(response.ReasonPhrase);
			}

		}

		public static async Task<string> DownloadAttachment(List<Attachment> attachments, string applicationUnderTest, TestPlan testPlan, TestCase testCase)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/attachments/get-test-result-attachments
			//Download the test result reports from automated test runs. Automated test runs have their test result linked as an attachment

			string url;
			var jsonString = "";
			foreach (Attachment attachment in attachments)
			{
				url = attachment.Url.Substring(attachment.Url.IndexOf("_apis/") + 6);
				var response = ApiHelper.ApiClient.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					jsonString += await response.Content.ReadAsStringAsync();
				}
				else
				{
					throw new Exception(response.ReasonPhrase);
				}


			}

			Guid guid = Guid.NewGuid();

			string saveLocation = $@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\\GeneratedEccDocuments\\{applicationUnderTest}\\{testPlan.Name.Replace("/", " ")}\\Reports\\{guid}.html";

			System.IO.File.WriteAllText(saveLocation, jsonString);

			string saveLocationRelative = saveLocation.Replace($"\\\\barcapp.org\\\\SharedFiles\\\\Cerca\\\\IT_Automation\\\\GeneratedEccDocuments\\\\{applicationUnderTest}", "..");

			return saveLocationRelative;
		}



		public static async Task<TestSuite> LoadRootTestSuiteTreeView(TestPlan testPlanForExport)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/test-suites/get-test-suites-for-plan
			//All test suites are loaded into an object with the tree view as they are set up in the test plan.

			string url = $"test/Plans/{testPlanForExport.Id}/suites?$asTreeView=true";
			var response = ApiHelper.ApiClient.GetAsync(url).Result;

			if (response.IsSuccessStatusCode)
			{
				var jsonString = JObject.Parse(await response.Content.ReadAsStringAsync())["value"].ToString();

				List<TestSuite> testSuite = JsonConvert.DeserializeObject<List<TestSuite>>(jsonString);

				return testSuite.First();
			}
			else
			{
				throw new Exception(response.ReasonPhrase);
			}

		}

		public static async Task<TestPlan> LoadTestCasesForAllTestSuites(TestPlan testPlanForExport)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/testplan/suite-test-case/get-test-case-list
			//For every suite in the test plan (flat list), all test cases are added to the suite object

			string url;
			HttpResponseMessage response;

			foreach (TestSuite suite in testPlanForExport.SuitesList)
			{
				url = $"testplan/Plans/{suite.TestPlan.Id}/Suites/{suite.Id}/TestCase?api-version=6.0";
				response = ApiHelper.ApiClient.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					var jsonString = JObject.Parse(await response.Content.ReadAsStringAsync())["value"].ToString();

					suite.TestCases = JsonConvert.DeserializeObject<List<TestCase>>(jsonString);

					foreach (TestCase testCase in suite.TestCases)
					{
						List<Step> testCaseSteps = new List<Step>();

						testCase.Id = testCase.WorkItem.Id;
						testCase.Name = testCase.WorkItem.Name;
						foreach (var item in testCase.WorkItem.workItemFields)
						{
							if (!String.IsNullOrEmpty(item.Steps))
							{
								var serializer = new XmlSerializer(typeof(Steps));
								var reader = new StringReader(item.Steps);

								var result = (Steps)serializer.Deserialize(reader);

								testCase.Steps = result;
							}
							if (!String.IsNullOrEmpty(item.Parameters))
							{
								XmlDocument doc = new XmlDocument();

								doc.LoadXml(Regex.Unescape(item.Parameters));

								testCase.ParameterCollectionPerIteration = new List<ParameterCollectionPerIteration>();

								if (doc.ChildNodes[1] != null)
								{
									foreach (XmlNode childNode in doc.ChildNodes[1])
									{
										if (childNode.Name == "Table1")
										{
											ParameterCollectionPerIteration ParameterCollection = new ParameterCollectionPerIteration();

											ParameterCollection.Parameter = new List<Parameter>();

											foreach (XmlNode child in childNode)
											{
												ParameterCollection.Parameter.Add(new Parameter(child.Name, child.InnerText));
											}

											testCase.ParameterCollectionPerIteration.Add(ParameterCollection);
										}
									}
								}

							}
						}

					}
				}
				else
				{
					throw new Exception(response.ReasonPhrase);
				}
			}

			return testPlanForExport;

		}

		public static async Task<List<TestRun>> LoadTestRunsAndResultsFor(TestPlan testPlanForExport)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/runs/list
			//Get all the test runs linked to the test plan

			string url = $"test/runs?api-version=6.0&planId={testPlanForExport.Id}";
			var response = ApiHelper.ApiClient.GetAsync(url).Result;

			testPlanForExport.Results = new List<Result>();

			if (response.IsSuccessStatusCode)
			{
				var jsonString = JObject.Parse(await response.Content.ReadAsStringAsync())["value"].ToString();

				List<TestRun> testRuns = JsonConvert.DeserializeObject<List<TestRun>>(jsonString);

				foreach (var testRun in testRuns)
				{
					testPlanForExport.Results.AddRange(LoadTestResultsFor(testRun).Result);
				}

				return testRuns;
			}
			else
			{
				throw new Exception(response.ReasonPhrase);
			}

		}

		public static async Task<List<Result>> LoadTestResultsFor(TestRun testRun)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/results/list
			//Get test results for each test run

			string url = $"test/Runs/{testRun.Id}/results?api-version=6.0&detailsToInclude=WorkItems";
			var response = ApiHelper.ApiClient.GetAsync(url).Result;

			if (response.IsSuccessStatusCode)
			{
				var jsonString = JObject.Parse(await response.Content.ReadAsStringAsync())["value"].ToString();

				List<Result> results = JsonConvert.DeserializeObject<List<Result>>(jsonString);

				foreach (var result in results)
				{
					result.Attachments = await LoadAttachmentsFor(testRun, result);
					result.Iterations = await LoadActionResultsWithAttachmentsFor(testRun, result);
					result.testRunId = testRun.Id;
				}

				return results;
			}
			else
			{
				throw new Exception(response.ReasonPhrase);
			}

		}

		private static async Task<List<Iteration>> LoadActionResultsWithAttachmentsFor(TestRun testRun, Result result)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/results/get
			//Get all the iterations with attachments for exporting test results from manual test runs.

			string url = $"test/Runs/{testRun.Id}/Results/{result.Id}/Iterations?api-version=6.0";
			var response = ApiHelper.ApiClient.GetAsync(url).Result;
			if (response.IsSuccessStatusCode)
			{
				var jsonString = JObject.Parse(await response.Content.ReadAsStringAsync())["value"].ToString();

				List<Iteration> iterations = JsonConvert.DeserializeObject<List<Iteration>>(jsonString);

				foreach (Iteration iteration in iterations)
				{
					foreach (Attachment attachment in iteration.Attachments)
					{
						attachment.Base64String = await LoadScreenshotBase64String(testRun, result, attachment);
					}
				}

				return iterations;
			}
			else
			{
				throw new Exception(response.ReasonPhrase);
			}
		}

		private static async Task<string> LoadScreenshotBase64String(TestRun testRun, Result result, Attachment attachment)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/attachments/get-test-result-attachment-zip
			//Get the base64 encoding for the attached screenshots from manual test runs

			string url = $"test/Runs/{testRun.Id}/Results/{result.Id}/attachments/{attachment.id}?api-version=7.1-preview.1";
			var response = ApiHelper.ApiClient.GetAsync(url).Result;

			if (response.IsSuccessStatusCode)
			{
				Guid guid = Guid.NewGuid();

				byte[] byteImage = response.Content.ReadAsByteArrayAsync().Result;
				string base64String = Convert.ToBase64String(byteImage);

				return base64String;
			}
			else
			{
				throw new Exception(response.ReasonPhrase);
			}
		}

		private static async Task<List<Attachment>> LoadAttachmentsFor(TestRun testRun, Result result)
		{
			//See detailed API info: https://docs.microsoft.com/en-us/rest/api/azure/devops/test/attachments/get-test-result-attachments
			//Get all the attachments from the automated test runs

			string url = $"test/Runs/{testRun.Id}/Results/{result.Id}/attachments?api-version=6.0";
			var response = ApiHelper.ApiClient.GetAsync(url).Result;

			if (response.IsSuccessStatusCode)
			{
				var jsonString = JObject.Parse(await response.Content.ReadAsStringAsync())["value"].ToString();

				List<Attachment> attachments = JsonConvert.DeserializeObject<List<Attachment>>(jsonString);

				return attachments;
			}
			else
			{
				throw new Exception(response.ReasonPhrase);
			}
		}

		private static TestPlan AddTestRunsWithResultsToTestSuitesFromList(TestPlan testPlanForExport)
		{
			//Iterate over every test case from the flat list and add the test results to the correct test case

			foreach (TestSuite testSuite in testPlanForExport.SuitesList)
			{
				foreach (TestCase testCase in testSuite.TestCases)
				{
					testCase.Results = new List<Result>();
					foreach (Result testResult in testPlanForExport.Results.OrderBy((i => i.CompletedDate)))
					{
						if (testCase.Id.Equals(testResult.TestCase.Id))
						{
							testCase.Results.Add(testResult);
							if (testResult.Outcome != "Aborted")
							{
								testCase.LatestResults = testResult;
							}
						}
					}
				}
			}

			return testPlanForExport;
		}

		private static TestPlan AddTestSuitesDataFromListToTreeView(TestPlan testPlanForExport)
		{
			//After all the needed data is loaded into the flat list of test cases, the metadata of these test cases is added to the test cases from the tree view

			foreach (TestSuite suite in testPlanForExport.SuitesList)
			{
				if (suite.Equals(testPlanForExport.RootSuite))
				{
					testPlanForExport.RootSuite.TestCases = suite.TestCases;
				}

				if (testPlanForExport.RootSuite.Children != null && testPlanForExport.RootSuite.Children.Any())
				{
					CopyDataToChildren(suite, testPlanForExport.RootSuite);
				}

			}

			return testPlanForExport;
		}

		private static void CopyDataToChildren(TestSuite suiteWithData, TestSuite suite)
		{
			//All metadata is copied over

			foreach (TestSuite childSuite in suite.Children)
			{
				if (suiteWithData.Equals(childSuite))
				{
					childSuite.TestCases = suiteWithData.TestCases;
				}
				if (childSuite.Children != null && childSuite.Children.Any())
				{
					CopyDataToChildren(suiteWithData, childSuite);
				}
			}
		}
	}
}
