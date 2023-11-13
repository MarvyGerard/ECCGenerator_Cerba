using GenerateECCDocument.Domain;
using Nancy.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharp.Drawing;
using System.IO;

namespace GenerateECCDocument
{
	class Program
	{
		static void Main(string[] args)
		{
			//***********************************************************************
			//   Configuration parameters
			//***********************************************************************

			if (Debugger.IsAttached)
			{
				List<string> list = new List<string>();
				list.Add("Cerca");
				list.Add("284");
				list.Add("false"); //disable proxy
				args = list.ToArray();
			}

			string BaseAddress = "https://dev.azure.com/barclab/";

			List<string> allowedProjects = new List<string>();
			allowedProjects.Add("CTMS");
			allowedProjects.Add("Covid19");
			allowedProjects.Add("Cerca");

			//PAT Token valid until: 14/12/2023 - When expired ask lead developer for new Azure devops PAT token on account Service VSTS Online.
			//Access level scopes: Work items(Read, write & manage) and Test Management(Read & write)

			string PATToken = "danohyrgr6lkeenrmnfg4l7wpyr2lwe3a2x4gc4tttab5t7t3qha";



			//***********************************************************************
			//   END Configuration parameters
			//***********************************************************************




			//Check if the expected arguments are present, for debugging add "|| false" at the end of the if clause
			if (args.Length < 2 || args.Length > 4)
			{
				throw new Exception("Please enter both parameter values. ApplicationUnderTest and RealeaseName");
			}
			else
			{
				//Arguments are given in commandline

				string ApplicationUnderTest = args[0];
				string RealeaseName = args[1];
				bool.TryParse(args[2], out var enableProxy);

				//Arguments are populated with the following

				//string ApplicationUnderTest = "Cerca";
				//string RealeaseName = "240";

				//PDF only is an optional argument, when set to 'yes' only the pdf is exported and not the html with attached test reports
				string pdfOnly = "no";
				if (args.Length == 4)
				{
					pdfOnly = args[3].ToLower();
				}

				bool validApplicationUnderTest = false;

				foreach (var allowedApplication in allowedProjects)
				{
					if (ApplicationUnderTest == allowedApplication)
					{
						validApplicationUnderTest = true;
					}
				}


				if (validApplicationUnderTest)
				{
					//Connecting to the API, contains base address and PAT authorization
					ApiHelper.InitializeClient(BaseAddress, ApplicationUnderTest, PATToken, enableProxy);

					//Get all the information from the APIs and add them to a nested object
					TestPlan testPlanForExport = new TestPlan();
					testPlanForExport = GetDataFromAzureDevops.GetAllInfo(RealeaseName);

					//Safety that existing exports are not overwritten
					if (Directory.Exists($@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\\GeneratedEccDocuments\\{ApplicationUnderTest}\\{testPlanForExport.Name.Replace("/", " ")}") && pdfOnly != "yes")
					{
						throw new Exception("The export folder already exists, to prevent overwriting test reports with empty data the user should manually delete the previous export. The current report folder can be found at " + $@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\\GeneratedEccDocuments\\{ApplicationUnderTest}\\{testPlanForExport.Name.Replace("/", " ")}");
					}
					else
					{
						//Generate the HTML and Css for exporting to PDF
						var cssData = PdfGenerator.ParseStyleSheet(GenerateHTML.GetCssString());
						PdfDocument pdf = PdfGenerator.GeneratePdf(GenerateHTML.GetStringForPDFExport(testPlanForExport), PageSize.A4, 60, cssData);

						// Make a font and a brush to draw the page counter.
						XFont font = new XFont("Arial", 8);
						XBrush brush = XBrushes.Black;
						XImage logo = XImage.FromFile("cerbalogo.png");

						// Add the page counter.
						string noPages = pdf.Pages.Count.ToString();
						for (int i = 0; i < pdf.Pages.Count; ++i)
						{
							PdfPage page = pdf.Pages[i];

							// Make a layout rectangle.
							XRect layoutRectangle = new XRect(0/*X*/, page.Height - font.Height - 10/*Y*/, page.Width/*Width*/, font.Height/*Height*/);

							using (XGraphics gfx = XGraphics.FromPdfPage(page))
							{
								//Add page numbers in footer
								gfx.DrawString(
									"Page " + (i + 1).ToString() + " of " + noPages,
									font,
									brush,
									layoutRectangle,
									XStringFormats.Center);


								//Draw logo
								//Make logo bigger on first page
								if (i == 0)
								{
									gfx.DrawImage(logo, 60, 20, 80, 40);
								}
								else
								{
									gfx.DrawImage(logo, 60, 20, 60, 30);
								}
							}
						}

						//Save the actual PDF and HTML reports
						if (pdfOnly == "yes")
						{
							pdf.Save($@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\\GeneratedEccDocuments\\{ApplicationUnderTest}\\OQ_CERBA CERCA_TestResults_{testPlanForExport.Name.Replace("/", " ")}.pdf");
						}
						else
						{
							string folderName = $@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\\GeneratedEccDocuments\\{ApplicationUnderTest}\\{testPlanForExport.Name.Replace("/", " ")}";
							System.IO.Directory.CreateDirectory(folderName);

							pdf.Save($@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\\GeneratedEccDocuments\\{ApplicationUnderTest}\\{testPlanForExport.Name.Replace("/", " ")}\\SYSTEM_TESTING_CERBA CERCA_TestResults_{testPlanForExport.Name.Replace("/", " ")}.pdf");

							System.IO.File.WriteAllText($@"\\barcapp.org\SharedFiles\Cerca\IT_Automation\\GeneratedEccDocuments\\{ApplicationUnderTest}\\{testPlanForExport.Name.Replace("/", " ")}\\SYSTEM_TESTING_CERBA CERCA_TestResults_{testPlanForExport.Name.Replace("/", " ")}.html", GenerateHTML.GetStringForHTMLExport(testPlanForExport, ApplicationUnderTest));
						}

					}
				}
				else
				{
					throw new Exception("Please enter a valid ApplicationUnderTest. Either enter a correct application name in the parameters or add is to the list in Program.cs");
				}


			}
		}




	}
}
