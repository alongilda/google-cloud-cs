using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using CommandLine;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using System.IO;
using System.Threading;

namespace Recognize
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //label1.Text = "Hello world!";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
    public class Recognize
    {

        // [START speech_streaming_mic_recognize]
        static async Task<object> StreamingMicRecognizeAsync(int seconds, Control ctrl)
        {
            ctrl.Text = "In func";
            ctrl.Refresh();
            if (NAudio.Wave.WaveIn.DeviceCount < 1)
            {
                Console.WriteLine("No microphone!");
                return -1;
            }
            var speech = SpeechClient.Create();
            var streamingCall = speech.StreamingRecognize();
            // Write the initial request with the config.
            await streamingCall.WriteAsync(
                new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding =
                            RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = "he-il",
                        },
                        InterimResults = true,
                    }
                });
            // Print responses as they arrive.
            Task printResponses = Task.Run(async () =>
            {
                ctrl.Text = "I aaaaaaaaaaa";
                ctrl.Refresh();
                while (await streamingCall.ResponseStream.MoveNext(
                    default(CancellationToken)))
                {
                    foreach (var result in streamingCall.ResponseStream
                        .Current.Results)
                    {
                        foreach (var alternative in result.Alternatives)
                        {
                            ctrl.Text = alternative.Transcript;
                            ctrl.Refresh();
                            Console.WriteLine(alternative.Transcript);
                        }
                    }
                }
            });
            // Read from the microphone and stream to API.
            object writeLock = new object();
            bool writeMore = true;
            var waveIn = new NAudio.Wave.WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
            waveIn.DataAvailable +=
                (object sender, NAudio.Wave.WaveInEventArgs args) =>
                {
                    lock (writeLock)
                    {
                        if (!writeMore) return;
                        streamingCall.WriteAsync(
                            new StreamingRecognizeRequest()
                            {
                                AudioContent = Google.Protobuf.ByteString
                                    .CopyFrom(args.Buffer, 0, args.BytesRecorded)
                            }).Wait();
                    }
                };
            waveIn.StartRecording();
            Console.WriteLine("Speak now.");
            ctrl.Text = "Speak now";
            ctrl.Refresh();
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            // Stop recording and shut down.
            waveIn.StopRecording();
            lock (writeLock) writeMore = false;
            await streamingCall.WriteCompleteAsync();
            await printResponses;
            return 0;
        }
        // [END speech_streaming_mic_recognize]

        static bool IsStorageUri(string s) => s.Substring(0, 4).ToLower() == "gs:/";

        public static int Main(string[] args)
        {
            Console.Write("Its a me!");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\WORK\\googleCSharp\\speechApi-3f6e3747e71a.json");


            var abcd = new Form1();
            abcd.Show();


            foreach (Control c in abcd.Controls)
            {
                c.Text = "FUCK ME";
                abcd.Refresh();
            }

            abcd.Text = "AAAA";
            //abcd.Refresh();
            // abcd.ShowDialog();
            //abcd.Controls = "FUCK";

            var label = abcd.Controls.Find("label1", true).FirstOrDefault();
            if (label != null)
                label.Text = "Bye world!";
            abcd.Refresh();


            return (int)StreamingMicRecognizeAsync(30, label).Result;


        }
    }

}
