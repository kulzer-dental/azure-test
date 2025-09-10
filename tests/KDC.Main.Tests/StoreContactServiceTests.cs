using KDC.Main.Config;
using KDC.Main.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDC.Main.Tests
{
    public class StoreContactServiceTests
    {
        private readonly StoreContactService _storeContactService;
        private readonly StoresContactsConfig _testConfig;

        public StoreContactServiceTests()
        {
            // Arrange - Set up test data
            _testConfig = new StoresContactsConfig
            {
                Stores = new Dictionary<string, StoreContact>
                {
                    ["us"] = new StoreContact
                    {
                        Id = "kulzer_usa",
                        SupportEmail = "customerservice.NA@kulzer-dental.com"
                    },
                    ["de-default"] = new StoreContact
                    {
                        Id = "default",
                        SupportEmail = "shop-test@kulzer.de"
                    },
                    ["au"] = new StoreContact
                    {
                        Id = "kulzer_au",
                        SupportEmail = "shop@kulzer.com.au"
                    }
                }
            };

            var mockOptions = new Mock<IOptions<StoresContactsConfig>>();
            mockOptions.Setup(x => x.Value).Returns(_testConfig);

            _storeContactService = new StoreContactService(mockOptions.Object);
        }

        [Fact]
        public void GetSupportEmail_WithValidStoreCode_ReturnsCorrectEmail()
        {
            // Act
            var result = _storeContactService.GetSupportEmail("kulzer_usa");

            // Assert
            Assert.Equal("customerservice.NA@kulzer-dental.com", result);
        }

        [Fact]
        public void GetSupportEmail_WithInvalidStoreCode_ReturnsFallbackEmail()
        {
            // Act
            var result = _storeContactService.GetSupportEmail("invalid-store");

            // Assert
            Assert.Equal("support@kulzer.com", result);
        }

        [Fact]
        public void GetSupportEmail_WithNullStoreCode_ReturnsFallbackEmail()
        {
            // Act
            var result = _storeContactService.GetSupportEmail(null);

            // Assert
            Assert.Equal("support@kulzer.com", result);
        }

        [Fact]
        public void GetSupportEmail_WithEmptyStoreCode_ReturnsFallbackEmail()
        {
            // Act
            var result = _storeContactService.GetSupportEmail("");

            // Assert
            Assert.Equal("support@kulzer.com", result);
        }

        [Fact]
        public void GetStoreContacts_WithValidStoreCode_ReturnsCorrectStoreContact()
        {
            // Act
            var result = _storeContactService.GetStoreContacts("kulzer_usa");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("kulzer_usa", result.Id);
            Assert.Equal("customerservice.NA@kulzer-dental.com", result.SupportEmail);
        }

        [Fact]
        public void GetStoreContacts_WithValidStoreCodeButNullOptionalFields_ReturnsStoreContactWithNulls()
        {
            // Act
            var result = _storeContactService.GetStoreContacts("default");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("default", result.Id);
            Assert.Equal("shop-test@kulzer.de", result.SupportEmail);
        }

        [Fact]
        public void GetStoreContacts_WithInvalidStoreCode_ReturnsNull()
        {
            // Act
            var result = _storeContactService.GetStoreContacts("invalid-store");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreContacts_WithNullStoreCode_ReturnsNull()
        {
            // Act
            var result = _storeContactService.GetStoreContacts(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreContacts_WithEmptyStoreCode_ReturnsNull()
        {
            // Act
            var result = _storeContactService.GetStoreContacts("");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("kulzer_usa", "customerservice.NA@kulzer-dental.com")]
        [InlineData("default", "shop-test@kulzer.de")]
        [InlineData("kulzer_au", "shop@kulzer.com.au")]
        public void GetSupportEmail_WithVariousValidStoreCodes_ReturnsCorrectEmails(string storeCode, string expectedEmail)
        {
            // Act
            var result = _storeContactService.GetSupportEmail(storeCode);

            // Assert
            Assert.Equal(expectedEmail, result);
        }

        [Theory]
        [InlineData("nonexistent")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void GetSupportEmail_WithInvalidStoreCodes_ReturnsFallbackEmail(string storeCode)
        {
            // Act
            var result = _storeContactService.GetSupportEmail(storeCode);

            // Assert
            Assert.Equal("support@kulzer.com", result);
        }

        [Fact]
        public void Constructor_WithValidOptions_CreatesServiceSuccessfully()
        {
            // Arrange
            var mockOptions = new Mock<IOptions<StoresContactsConfig>>();
            mockOptions.Setup(x => x.Value).Returns(new StoresContactsConfig());

            // Act & Assert
            var service = new StoreContactService(mockOptions.Object);
            Assert.NotNull(service);
        }
        
    }
}
