using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace dataentry.stowrs.test
{
    public class DicomMetadata
    {
        public static readonly string Single = Path.GetFullPath("resources/metadata/metadata_single.json");
        public static readonly string Multiple = Path.GetFullPath("resources/metadata/metadata_multiple.json");
    }

    public class DicomFiles
    {
        public static readonly string Single = Path.GetFullPath("resources/blobs/blobs_single.json");
        public static readonly string Multiple = Path.GetFullPath("resources/blobs/blobs_multiple.json");
    }

    public class FileToStore
    {
        public string File { get; set; }
        public string BlobDataUri { get; set; }
    }


    public class ApiTestFixture : IDisposable
    {
        private const string BaseAddress = "{INSERT BASE URL HERE}";
        private const string ApiEndpoint = "api/stowrs/studies";
        private const string AuthToken = "{INSERT BEARER TOKEN HERE}";

        private const string DicomJsonMimeType = "application/dicom+json";
        private const string DicomMimeType = "application/dicom";

        private readonly string _jpegFile = Path.GetFullPath("resources/test.jpg");
        private readonly string _jpegMetadataFile = Path.GetFullPath("resources/metadata/metadata_single_jpeg.json");

        public ApiTestFixture()
        {
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress)
            };
        }

        public HttpClient HttpClient { get; }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public HttpRequestMessage CreateRequest(MultipartContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint)
            {
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

            return request;
        }

        public MultipartContent CreateStowRsContent(string metadataFile, IEnumerable<FileToStore> files)
        {
            var multipartContent = GetMultipartContent(DicomJsonMimeType);

            var jsonContent = new StreamContent(File.OpenRead(metadataFile));
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue(DicomJsonMimeType);
            multipartContent.Add(jsonContent);

            foreach (var file in files)
            {
                var sContent = new StreamContent(File.OpenRead(file.File));

                sContent.Headers.ContentType = new MediaTypeHeaderValue(DicomMimeType);
                sContent.Headers.ContentLocation = new Uri(file.BlobDataUri);

                multipartContent.Add(sContent);
            }

            return multipartContent;
        }

        public MultipartContent CreateUnsupportedStowRsContent(string metadataFile)
        {
            var mimeType = MediaTypeNames.Text.Plain;
            var multipartContent = GetMultipartContent(mimeType);

            var jsonContent = new StreamContent(File.OpenRead(metadataFile));
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            multipartContent.Add(jsonContent);

            return multipartContent;
        }

        public MultipartContent CreateMultipartJpegWithMetadataContent()
        {
            var multipartContent = GetMultipartContent(DicomJsonMimeType);

            var jsonContent = new StreamContent(File.OpenRead(_jpegMetadataFile));
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue(DicomJsonMimeType);
            multipartContent.Add(jsonContent);

            var mimeType = "image/jpeg";

            var sContent = new StreamContent(File.OpenRead(_jpegFile));

            sContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            sContent.Headers.ContentLocation = new Uri("urn:uuid:7673868d-231e-490d-9c4f-19288e7e668d");

            multipartContent.Add(sContent);


            return multipartContent;
        }

        /// <summary>
        ///     Get a valid multipart content.
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        private static MultipartContent GetMultipartContent(string mimeType)
        {
            var multiContent = new MultipartContent("related", "DICOM DATA BOUNDARY");

            multiContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", "\"" + mimeType + "\""));
            return multiContent;
        }
    }
}