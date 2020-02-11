using Microsoft.Azure.CognitiveServices.FormRecognizer;
using Microsoft.Azure.CognitiveServices.FormRecognizer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestFormRecognizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // // Add your Azure Form Recognizer subscription key and endpoint to your environment variables.
        private static string _subscriptionKey = "";
        private static string _formRecognizerEndpoint = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// モデルの一覧を取得する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonList_Click(object sender, RoutedEventArgs e)
        {
            var client = BuildFormRecognizerClient();
            textBox.Text = await GetListOfModelsAsync(client);
        }

        /// <summary>
        /// カスタムトレーニングモデルを作成します
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonTrain_Click(object sender, RoutedEventArgs e)
        {
            var client = BuildFormRecognizerClient();
            var guid = await TrainModelAsync(client, textDataUrl.Text);
            textBox.AppendText("\n Train Model Id : " + guid.ToString());
        }

        /// <summary>
        /// JPEGファイルを解析します
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void buttonAnalyze_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                var client = BuildFormRecognizerClient();
                await AnalyzeForm(client, new Guid(textGuid.Text), ofd.FileName);
            } 
        }

        /// <summary>
        /// FormRecognizerClientを構築します。
        /// プロキシが必要な場合はここで構築する。
        /// </summary>
        /// <returns></returns>
        private IFormRecognizerClient BuildFormRecognizerClient()
        {
            return new FormRecognizerClient(new ApiKeyServiceClientCredentials(_subscriptionKey))
            {
                Endpoint = _formRecognizerEndpoint
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formClient"></param>
        /// <param name="trainingDataUrl"></param>
        /// <returns></returns>
        private async Task<Guid> TrainModelAsync(IFormRecognizerClient formClient, string trainingDataUrl)
        {
            if (!Uri.IsWellFormedUriString(trainingDataUrl, UriKind.Absolute))
            {
                textBox.AppendText($"\nInvalid trainingDataUrl:\n{trainingDataUrl} \n");
                return Guid.Empty;
            }

            try
            {
                TrainResult result = await formClient.TrainCustomModelAsync(new TrainRequest(trainingDataUrl));

                ModelResult model = await formClient.GetCustomModelAsync(result.ModelId);
                DisplayModelStatus(model);

                return result.ModelId;
            }
            catch (ErrorResponseException e)
            {
                textBox.AppendText($"\nTrain Model : " + e.Message);
                return Guid.Empty;
            }
        }

        // Display model status
        private void DisplayModelStatus(ModelResult model)
        {
            textBox.AppendText("\nModel :");
            textBox.AppendText("\n\tModel id: " + model.ModelId);
            textBox.AppendText("\n\tStatus:  " + model.Status);
            textBox.AppendText("\n\tCreated: " + model.CreatedDateTime);
            textBox.AppendText("\n\tUpdated: " + model.LastUpdatedDateTime);
        }

        private async Task<string> GetListOfModelsAsync(IFormRecognizerClient formClient)
        {
            var sb = new StringBuilder();

            try
            {
                ModelsResult models = await formClient.GetCustomModelsAsync();
                sb.AppendLine("--------GetListOfModels--------");
                foreach (ModelResult m in models.ModelsProperty)
                {
                    sb.AppendLine(m.ModelId + " " + m.Status + " " + m.CreatedDateTime + " " + m.LastUpdatedDateTime);
                }
            }
            catch (ErrorResponseException e)
            {
                sb.AppendLine("Get list of models : " + e.Message);
            }

            return sb.ToString();
        }

        private async Task AnalyzeForm(IFormRecognizerClient formClient, Guid modelId, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                textBox.AppendText("\nInvalid filePath.");
                return;
            }

            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    AnalyzeResult result = await formClient.AnalyzeWithCustomModelAsync(modelId, stream, contentType: "image/jpeg");

                    textBox.AppendText("\nExtracted data from:" + filePath);
                    DisplayAnalyzeResult(result);
                }
            }
            catch (ErrorResponseException e)
            {
                textBox.AppendText("\nAnalyze form : " + e.Message);
            }
            catch (Exception ex)
            {
                textBox.AppendText("\nAnalyze form : " + ex.Message);
            }
        }

        private void DisplayAnalyzeResult(AnalyzeResult result)
        {
            foreach (var page in result.Pages)
            {
                textBox.AppendText("\tPage#: " + page.Number);
                textBox.AppendText("\n\tCluster Id: " + page.ClusterId);
                foreach (var kv in page.KeyValuePairs)
                {
                    if (kv.Key.Count > 0)
                        textBox.AppendText(kv.Key[0].Text);

                    if (kv.Value.Count > 0)
                        textBox.AppendText(" - " + kv.Value[0].Text);

                    textBox.AppendText("\n");
                }
                textBox.AppendText("\n");

                foreach (var t in page.Tables)
                {
                    textBox.AppendText("\nTable id: " + t.Id);
                    foreach (var c in t.Columns)
                    {
                        foreach (var h in c.Header)
                            textBox.AppendText(h.Text + "\t");

                        foreach (var e in c.Entries)
                        {
                            foreach (var ee in e)
                                textBox.AppendText(ee.Text + "\t");
                        }
                        textBox.AppendText("\n");
                    }
                    textBox.AppendText("\n");
                }
            }
        }
    }
}
