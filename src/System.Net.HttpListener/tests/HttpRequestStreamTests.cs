// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Net.Tests
{
    public class HttpRequestStreamTests : IDisposable
    {
        private HttpListenerFactory _factory;
        private HttpListener _listener;
        private GetContextHelper _helper;

        public HttpRequestStreamTests()
        {
            Debug.WriteLine("HttpRequestStreamTests: CTOR");
            _factory = new HttpListenerFactory();
            _listener = _factory.GetListener();
            _helper = new GetContextHelper(_listener, _factory.ListeningUrl);
        }

        public void Dispose()
        {
            _factory.Dispose();
            _helper.Dispose();
            Debug.WriteLine("HttpRequestStreamTests: Dispose");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CanRead_Get_ReturnsTrue(bool chunked)
        {
            Debug.WriteLine("Start");
            Task<HttpListenerContext> contextTask = _listener.GetContextAsync();
            using (HttpClient client = new HttpClient())
            {
                Debug.WriteLine("step 1: clientTask.PostAsync({0})", _factory.ListeningUrl);
                client.DefaultRequestHeaders.TransferEncodingChunked = chunked;
                Task<HttpResponseMessage> clientTask = client.PostAsync(_factory.ListeningUrl, new StringContent("Hello"));

                Debug.WriteLine("step 2");
                HttpListenerContext context = await contextTask;
                Debug.WriteLine("step 2.1");
                HttpListenerRequest request = context.Request;
                Debug.WriteLine("step 2.2");
                using (Stream inputStream = request.InputStream)
                {
                    Debug.WriteLine("step 2.3");
                    Assert.True(inputStream.CanRead);
                }
                
                Debug.WriteLine("step 3");
                context.Response.Close();
                Debug.WriteLine("step 4");

                //await clientTask;
                //client.CancelPendingRequests();
            }

            Debug.WriteLine("step 5");
            _listener.Stop();
            Debug.WriteLine("End");
            Debug.WriteLine("===============================================");
        }

        //[Theory]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TaskTest(bool chunked)
        {
            TaskCompletionSource<int> listenerGetContextAsync = new TaskCompletionSource<int>();

            Debug.WriteLine("Start");

            Task<int> contextTask = listenerGetContextAsync.Task;

            Debug.WriteLine("step 1");
            Task<int> clientTask = Task.Factory.StartNew(() =>
            {
                if (listenerGetContextAsync.TrySetResult(1))
                {
                    return 1;
                }

                return 0;
            });

            Debug.WriteLine("step 2");
            int context = await contextTask;

            Debug.WriteLine("End");
            Debug.WriteLine("");
        }
    }
}
