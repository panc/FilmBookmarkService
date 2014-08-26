using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilmBookmarkService.Controllers;

namespace FilmBookmarkService.Tests.Controllers
{
    [TestClass]
    public class FilmControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            var controller = new FilmController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
