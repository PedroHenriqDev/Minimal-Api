using Catalogue.Application.DTOs;
using Catalogue.Application.DTOs.Responses;
using Catalogue.Application.Interfaces;
using Catalogue.Application.Pagination;
using Catalogue.Application.Products.Queries.Responses;
using Catalogue.Domain.Entities;
using Catalogue.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Catalogue.IntegrationTests.Endpoints
{
    [Collection(nameof(CustomWebAppFixture))]
    public class ProductEndpointsTests
    {
        private readonly CustomWebAppFixture _fixture;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public ProductEndpointsTests(CustomWebAppFixture fixture)
        {
            _fixture = fixture;
            _httpClient = _fixture.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7140/api/v1/products/");
            _options = new JsonSerializerOptions(){ PropertyNameCaseInsensitive = true };
        }

        /// <summary>
        /// Tests that 'get' request to the 'https://localhost:7140/api/v1/products/' endpoint
        /// returns a 200 OK status code response and the products paginated.
        /// </summary>
        [Fact]
        public async Task GetAllProducts_WhenQueryStringIsValid_ShouldReturn200OKAndProductsPaginated()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;

            string queryString = $"?pageNumber={pageNumber}&pageSize={pageSize}";
            string token = _fixture.GenerateTokenAdmin();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage httpResponse = await _httpClient.GetAsync(queryString);

            PaginationMetadata? metadata = _fixture.GetHeaderPagination(httpResponse);

            IPagedList<GetProductQueryResponse>? response = 
                await _fixture.ReadHttpResponseAsync<PagedList<GetProductQueryResponse>>
                (
                    httpResponse,
                     _options
                );
            
            // Assert 
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.NotEmpty(response);
            Assert.Equal(pageNumber, metadata.PageCurrent);
            Assert.Equal(pageSize, metadata.PageSize);
            Assert.Equal(pageSize, metadata.PageSize);
            Assert.False(metadata.HasPreviousPage);
            Assert.True(metadata.HasNextPage);
        }

        /// <summary>
        /// Tests that 'get' request to the 'https://localhost:7140/api/v1/products/{id}' endpoint when product
        /// id exists, should returns a 200 OK status code response the expected products.
        /// </summary>
        [Fact]
        public async Task GetByIdProduct_WhenProductIdExists_ShouldReturn200OKAndProductExpected()
        {
            // Arrange
            string token = _fixture.GenerateTokenAdmin();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Guid id = _fixture.DbContext.Products.First().Id;

            // Act
            HttpResponseMessage httpResponse = await _httpClient.GetAsync(id.ToString());

            GetProductQueryResponse? response = 
                await _fixture.ReadHttpResponseAsync<GetProductQueryResponse>
                (
                    httpResponse,
                     _options
                );
            
            // Assert 
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.NotNull(response);
            Assert.Equal(id, response.Id);
        }

        /// <summary>
        /// Tests that 'get' request to the 'https://localhost:7140/api/v1/products/{id}' endpoint when product
        /// id not exists, should returns a 404 Not Found status code response.
        /// </summary>
        [Fact]
        public async Task GetByIdProduct_WhenProductIdNotExists_ShouldReturn404NotFound()
        {
            // Arrange
            string token = _fixture.GenerateTokenAdmin();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Guid id = Guid.NewGuid();

            // Act
            HttpResponseMessage httpResponse = await _httpClient.GetAsync(id.ToString());

            ErrorResponse? response = 
                await _fixture.ReadHttpResponseAsync<ErrorResponse>
                (
                    httpResponse,
                     _options
                );
            
            // Assert 
            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
            Assert.NotNull(response);
        }

        /// <summary>
        /// Tests that 'get' request to the 'https://localhost:7140/api/v1/products/category/{id}' endpoint when product
        /// id exists, should returns a 200 OK status code response and product with your category.
        /// </summary>
        [Fact]
        public async Task GetByIdProductWithCategory_WhenProductIdExists_ShouldReturn200OK()
        {
            //Arrange
            string token = _fixture.GenerateTokenAdmin();
            Product productExpected = _fixture.DbContext.Products.First();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            HttpResponseMessage httpResponse = await _httpClient.GetAsync("category/" + productExpected.Id);
            GetProductWithCatQueryResponse? response =
                await _fixture.ReadHttpResponseAsync<GetProductWithCatQueryResponse>
                (
                    httpResponse,
                     _options
                );

            //Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.NotNull(response);
            Assert.NotNull(response.Category);
            Assert.Equal(productExpected.Id, response.Id);
            Assert.Equal(productExpected.Category.Name, response.Category.Name);
        }

        /// <summary>
        /// Tests that 'get' request to the 'https://localhost:7140/api/v1/products/category/{id}' endpoint when product
        /// id exists, should returns a 404 Not Found status code response.
        /// </summary>
        [Fact]
        public async Task GetByIdProductWithCategory_WhenProductIdNotExists_ShouldReturn404NotFound()
        {
            //Arrange
            string token = _fixture.GenerateTokenAdmin();
            Guid id = Guid.NewGuid();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            HttpResponseMessage httpResponse = await _httpClient.GetAsync("category/" + id);

            ErrorResponse? response =
                await _fixture.ReadHttpResponseAsync<ErrorResponse>
                (
                    httpResponse,
                     _options
                );

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
            Assert.NotNull(response);
        }

        /// <summary>
        /// Tests that 'get' request to the 'https://localhost:7140/api/v1/products/category' endpoint when exists
        /// products, should returns a 200 OK status code response and products with your associated categories.
        /// </summary>
        [Fact]
        public async Task GetProductsWithCategories_WhenQueryStringValid_ShouldReturn200OK()
        {
            //Arrange
            string token = _fixture.GenerateTokenAdmin();
            int pageNumber = 1;
            int pageSize = 10;

            string queryString = $"?pageNumber={pageNumber}&pageSize={pageSize}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Act
            HttpResponseMessage httpResponse = await _httpClient.GetAsync("category" + queryString);
            
            IPagedList<GetProductWithCatQueryResponse>? response =
                await _fixture.ReadHttpResponseAsync<PagedList<GetProductWithCatQueryResponse>>
                (
                    httpResponse,
                     _options
                );

            PaginationMetadata? metadata = _fixture.GetHeaderPagination(httpResponse);

            //Assert
            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.NotNull(response);
            Assert.NotEmpty(response);
            Assert.Equal(response.Count(), response.Select(p => p.Category != null).Count());
            Assert.Equal(pageNumber, metadata.PageCurrent);
            Assert.Equal(pageSize, metadata.PageSize);
            Assert.False(metadata.HasPreviousPage);
            Assert.True(metadata.HasNextPage);
        }
    }
}