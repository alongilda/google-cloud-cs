/*
 * Copyright (c) 2017 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
 
using CommandLine;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoogleCloudSamples
{
    public class Recognize
    {

        // [START speech_streaming_mic_recognize]
        static async Task<object> StreamingMicRecognizeAsync(int seconds)
        {
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
                while (await streamingCall.ResponseStream.MoveNext(
                    default(CancellationToken)))
                {
                    foreach (var result in streamingCall.ResponseStream
                        .Current.Results)
                    {
                        foreach (var alternative in result.Alternatives)
                        {
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
        /*
                public static int Main(string[] args)
                {
                    Console.Write("Its a me!");
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\WORK\\googleCSharp\\speechApi-3f6e3747e71a.json");



                    /*return (int)Parser.Default.ParseArguments<
                        SyncOptions, AsyncOptions,
                        StreamingOptions, ListenOptions,
                        RecOptions, SyncOptionsWithCreds,
                        OptionsWithContext
                        >(args).MapResult(
                        (SyncOptions opts) => IsStorageUri(opts.FilePath) ?
                            SyncRecognizeGcs(opts.FilePath) : opts.EnableWordTimeOffsets ?
                            SyncRecognizeWords(opts.FilePath) : SyncRecognize(opts.FilePath),
                        (AsyncOptions opts) => IsStorageUri(opts.FilePath) ?
                            (opts.EnableWordTimeOffsets ? AsyncRecognizeGcsWords(opts.FilePath)
                            : AsyncRecognizeGcs(opts.FilePath))
                            : LongRunningRecognize(opts.FilePath),
                        (StreamingOptions opts) => StreamingRecognizeAsync(opts.FilePath).Result,
                        (ListenOptions opts) => StreamingMicRecognizeAsync(opts.Seconds).Result,
                        (RecOptions opts) => Rec(opts.FilePath, opts.BitRate, opts.Encoding),
                        (SyncOptionsWithCreds opts) => SyncRecognizeWithCredentials(
                            opts.FilePath, opts.CredentialsFilePath),
                        (OptionsWithContext opts) => RecognizeWithContext(opts.FilePath, ReadPhrases()),
                        errs => 1);

                    return (int)StreamingMicRecognizeAsync(30).Result;


                }
            }
        */
    }
}
