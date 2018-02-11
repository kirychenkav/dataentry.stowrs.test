using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace dataentry.stowrs.test
{
    public class ApiTest : IClassFixture<ApiTestFixture>
    {
        public ApiTest(ApiTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        private readonly ApiTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        [Fact]
        public async Task UnsupportedContentType()
        {
            //Arrange
            var request = _fixture.CreateRequest(_fixture.CreateUnsupportedStowRsContent(DicomMetadata.Single));

            //Act
            var result = await _fixture.HttpClient.SendAsync(request);
            
            //Assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, result.StatusCode);
        }

        [Fact]
        public async Task StowRsSingle()
        {
            //Arrange
            var file = JsonConvert.DeserializeObject<FileToStore>(File.ReadAllText(DicomFiles.Single));

            var request = _fixture.CreateRequest(_fixture.CreateStowRsContent(DicomMetadata.Single, new[] { file }));

            //Act
            var result = await _fixture.HttpClient.SendAsync(request);
            _output.WriteLine(await result.Content.ReadAsAsync<string>());
            
            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task StowRsMultiple()
        {
            //Arrange
            var files = JsonConvert.DeserializeObject<IEnumerable<FileToStore>>(File.ReadAllText(DicomFiles.Multiple));

            var request = _fixture.CreateRequest(_fixture.CreateStowRsContent(DicomMetadata.Multiple, files));

            //Act
            var result = await _fixture.HttpClient.SendAsync(request);
            _output.WriteLine(await result.Content.ReadAsAsync<string>());

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task StowRsSingleJpegWithMetadata()
        {
            //Arrange
            var request = _fixture.CreateRequest(_fixture.CreateMultipartJpegWithMetadataContent());

            //Act
            var result = await _fixture.HttpClient.SendAsync(request);
            _output.WriteLine(await result.Content.ReadAsAsync<string>());

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}