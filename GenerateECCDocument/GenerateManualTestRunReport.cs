using GenerateECCDocument.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GenerateECCDocument
{
	public class GenerateManualTestRunReport
	{
		public static string GenerateReport(TestPlan testPlan, TestCase testCase, string applicationUnderTest)
		{
			StringBuilder htmlOutput = new StringBuilder();

			htmlOutput.Append("<!DOCTYPE html>");
			htmlOutput.Append("<html>");
			htmlOutput.Append("<head>");
			htmlOutput.Append("<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css' integrity='sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T' crossorigin='anonymous'>");
			htmlOutput.Append("</head>");
			htmlOutput.Append("<body style='margin:20px'>");
			htmlOutput.AppendFormat("<h1>{0}</h1>", testCase.Name);

			foreach (var result in testPlan.Results.AsEnumerable().Reverse())
			{
				if (result.TestCase.Id == testCase.Id)
				{
					testCase.ResultPrintedForTestCaseCount++;
					if (testCase.ResultPrintedForTestCaseCount > 1)
					{
						htmlOutput.Append("<br><hr><hr><br>");
						if (testCase.ResultPrintedForTestCaseCount == 2)
						{
							htmlOutput.Append("<h2>Previous runs:</h2><br>");
						}
					}
					foreach (var iteration in result.Iterations)
					{
						if (result.Iterations.Count > 1)
						{
							htmlOutput.AppendFormat("<br><br><h2>Iteration: {0}</h2>", iteration.Id);
							htmlOutput.Append("<i>Parameters:</i><br>");

							int parameterIndex = 0;
							foreach (ParameterCollectionPerIteration parameterCollectionPerIteration in testCase.ParameterCollectionPerIteration)
							{
								parameterIndex++;
								if (parameterIndex == iteration.Id)
								{
									foreach (Parameter parameter in parameterCollectionPerIteration.Parameter)
									{
										htmlOutput.AppendFormat("<i>@{0}: {1}</i><br>", parameter.ParameterName, parameter.Value);
									}
								}
							}
						}

						foreach (var step in testCase.Steps.Step)
						{
							var parameterizedString = step.ParameterizedString.FirstOrDefault();
							if (parameterizedString != null && parameterizedString.Content != null)
							{
								var formattedContent = parameterizedString.Content.Replace("<p>", "<span>").Replace("</p>", "</span>");
								htmlOutput.AppendFormat("<h4>{0} &nbsp; ", formattedContent);
							}
							else
							{
								// Handle the case where the content is null, perhaps with a placeholder
								htmlOutput.AppendFormat("<h4>{0} &nbsp; ", "[Content not available]");
							}

							foreach (var actionResult in iteration.ActionResults)
							{
								if (step.Id == actionResult.StepIdentifier)
								{
									if (actionResult.Outcome == "Passed")
									{
										htmlOutput.Append("<span class='badge badge-success'>Passed</span>");
									}
									else if (actionResult.Outcome == "Failed")
									{
										htmlOutput.Append("<span class='badge badge-danger'>Failed</span>");
									}

									htmlOutput.Append("</h4>");

									foreach (var attachment in iteration.Attachments)
									{
										if (actionResult.ActionPath == attachment.actionPath)
										{
											htmlOutput.AppendFormat("<img src='data:image/png;base64,{0}' width='50%' height='50%' /><br><br>", attachment.Base64String);
										}
									}
								}
							}
						}
					}
				}
			}

			string testCaseGuid = Guid.NewGuid().ToString("N");
			string uniqueFileName = $"{testCaseGuid}.html";

			// Use the test plan name as the main folder and create a Reports subfolder inside it
			string saveLocation = $@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\GeneratedEccDocuments\{applicationUnderTest}\{testPlan.Name.Replace("/", " ")}\Reports\{uniqueFileName}";

			// Ensure the directory exists before writing the file
			string directoryPath = System.IO.Path.GetDirectoryName(saveLocation);
			if (!System.IO.Directory.Exists(directoryPath))
			{
				System.IO.Directory.CreateDirectory(directoryPath);
			}

			System.IO.File.WriteAllText(saveLocation, htmlOutput.ToString());

			string saveLocationRelative = saveLocation.Replace($@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\GeneratedEccDocuments\{applicationUnderTest}", "..");

			return saveLocationRelative;
		}
	}
}
