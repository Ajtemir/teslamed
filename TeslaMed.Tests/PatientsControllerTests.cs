using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using Moq.EntityFrameworkCore;
using TeslaMed.Controllers;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using TeslaMed.ViewModels;
using Xunit;

namespace TeslaMed.Tests
{
    public class PatientsControllerTests
    {
        //[Fact]
        //public void IndexReturnsAViewResultWithAllListOfPatients()
        //{
        //    var mock = new Mock<IRepository>();
        //    mock.Setup(repo => repo.GetAll<Patient>("Patients")).ReturnsDbSet(GetTestPatients());
        //    var controller = new PatientsController(null, null, null, mock.Object);

        //    var result = controller.Index().Result;

        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var model = Assert.IsAssignableFrom<IEnumerable<Patient>>(viewResult.Model);
        //    Assert.Equal(GetTestPatients().Count(), model.Count());
        //}

        [Fact]
        public void AddPatienReturnsViewResultWithPatientModel()
        {
            var mock = new Mock<IRepository>();
            var controller = new PatientsController(null, null, null, mock.Object);
            controller.ModelState.AddModelError("Name", "Required");
            PatientCreateViewModel patient = new PatientCreateViewModel();

            var result = controller.Create(patient).Result;

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(patient, viewResult?.Model);
        }

        [Fact]
        public void AddPatientReturnsARedirectAndAddsPatient()
        {
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAll<Patient>("Patients")).ReturnsDbSet(GetTestPatients());
            var controller = new PatientsController(null, null, null, mock.Object);
            PatientCreateViewModel patient = new PatientCreateViewModel()
            {
                Name = "Vasya",
                Surname = "Pupkin",
                Patronymic = "Ivanovich",
                Gender = "Male",
                BirthDate = Convert.ToDateTime("12.01.1990"),
                PhoneNumber = "1",
                SecondPhoneNumber = "2",
                Comment = "qwe",
                CreateDiagnostics = false
            };

            var result = controller.Create(patient).Result;

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectToActionResult.ControllerName);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mock.Verify(r => r.DbSave());
        }

        [Fact]
        public void GetUserReturnsBadRequestResultWhenIdIsNull()
        {
            var mock = new Mock<IRepository>();
            var controller = new PatientsController(null, null, null, mock.Object);

            var result = controller.Details(null).Result;

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void GetUserReturnsNotFoundResultWhenUserNotFound()
        {
            int patientId = 5;
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAll<Patient>("Patients")).ReturnsDbSet(GetTestPatients());
            var controller = new PatientsController(null, null, null, mock.Object);

            var result = controller.Details(patientId).Result;

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetPatienWithEmptyDiagnosticsReturnsViewResultWithPatient()
        {
            int patientId = 3;
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAll<Patient>("Patients")).ReturnsDbSet(GetTestPatients());
            mock.Setup(repo => repo.GetAll<Diagnostics>("Diagnostics")).ReturnsDbSet(GetTestDiagnostics());
            mock.Setup(repo => repo.GetAll<TypeOfCashlessPayment>("TypesOfCachlessPayment")).ReturnsDbSet(GetTestEmptyTypesOfCashlessPayment());
            var controller = new PatientsController(null, null, null, mock.Object);

            var result = controller.Details(patientId).Result;

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PatientDetailsViewModel>(viewResult.Model);
            Assert.Equal("Sidor", model.Patient.Name);
            Assert.Equal(1993, model.Patient.BirthDate.Year);
            Assert.Equal(3, model.Patient.Id);
        }

        [Fact]
        public void EditPatientReturnsViewResultWithPatient()
        {
            int patientId = 2;
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAll<Patient>("Patients")).ReturnsDbSet(GetTestPatients());
            var controller = new PatientsController(null, null, null, mock.Object);
            var patient = mock.Object.GetAll<Patient>("Patients").FirstOrDefault(p => p.Id == patientId);
            var patientVM = new PatientCreateViewModel()
            {
                Name = "Vasilii",
                Surname = "Sidorov",
                Patronymic = "Ivanovich",
                BirthDate = Convert.ToDateTime("02.02.1992"),
                Gender = "Male",
                PhoneNumber = "123",
                SecondPhoneNumber = "134",
                Comment = "Top",
            };

            var result = controller.Edit(patientVM, patient.Code, patient.Id).Result;

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectToActionResult.ControllerName);
            Assert.Equal("Details", redirectToActionResult.ActionName);
            Assert.True(redirectToActionResult.RouteValues.Count == 1);
            Assert.Equal("id", redirectToActionResult.RouteValues.Keys.FirstOrDefault());
            Assert.Equal(patient.Id, redirectToActionResult.RouteValues.Values.FirstOrDefault());
            mock.Verify(r => r.DbUpdate(It.IsAny<Patient>()), Times.Once());
            mock.Verify(r => r.DbSave());
        }

        [Fact]
        public void GetListOfTypesOfDiagnosticWithSameOneResearchMethodReturnPartialViewWithViewModel()
        {
            int researchMethodId = 2;
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAll<TypeOfDiagnostics>("TypesOfDiagnostics")).ReturnsDbSet(GetTestTypesOfDiagnosics());
            mock.Setup(repo => repo.GetAll<ResearchMethod>("ResearchMethods")).ReturnsDbSet(GetTestResearchMethods());
            var controller = new PatientsController(null, null, null, mock.Object);

            var result = controller.TypesOfDiagnosticList(researchMethodId).Result;

            var viewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DiagnosticsCreateViewModel>(viewResult.Model);
            Assert.NotNull(model.TypesOfDiagnostics);
            Assert.Equal(model.TypesOfDiagnostics.Count, mock.Object.GetAll<TypeOfDiagnostics>("TypesOfDiagnostics").Where(r => r.ResearchMethodId == researchMethodId).Count());
        }
      
        public void GetPatientsReturnsPartialViewWitlListOfPatients()
        {
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAll<Patient>("Patients")).ReturnsDbSet(GetTestPatients());
            var controller = new PatientsController(null, null, null, mock.Object);
            var patient = mock.Object.GetAll<Patient>("Patients").FirstOrDefault(p => p.Id == 2);

            var result = controller.PatientExistenceCheck(patient.Name, patient.Surname, patient.Patronymic, patient.BirthDate.ToShortDateString()).Result;

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<List<Patient>>(partialViewResult.Model);
            Assert.Equal(1, model.Count);
            Assert.Equal("PartialViews/ExistingPatientsListPartialView", partialViewResult.ViewName);
        }

        [Fact]
        public void GetPatientsReturnsStatusCode()
        {
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAll<Patient>("Patients")).ReturnsDbSet(GetTestPatients());
            var controller = new PatientsController(null, null, null, mock.Object);

            var result = controller.PatientExistenceCheck("Name", "Surname", "Patronymic", "12-12-2012").Result;

            var partialViewResult = Assert.IsType<JsonResult>(result);
            var value = Assert.IsType<int>(partialViewResult.Value);
            Assert.Equal(300, partialViewResult.Value);
        }

        private DbSet<TypeOfDiagnostics> GetTestTypesOfDiagnosics()
        {
            var typesOfDiagnostics = new List<TypeOfDiagnostics>()
            {
                new TypeOfDiagnostics()
                {
                    Id = 1,
                    Name = "Test",
                    ResearchMethodId = 1
                },
                new TypeOfDiagnostics()
                {
                    Id = 2,
                    Name = "Test2",
                    ResearchMethodId = 2
                },
                new TypeOfDiagnostics()
                {
                    Id = 3,
                    Name= "Test3",
                    ResearchMethodId = 2
                }
            };
            return GetQueryableMockDbSet(typesOfDiagnostics);
        }

        private DbSet<ResearchMethod> GetTestResearchMethods()
        {
            var researchMethods = new List<ResearchMethod>()
            {
                new ResearchMethod()
                {
                    Id = 1,
                    Name = "Method_1",
                },
                new ResearchMethod()
                {
                    Id = 2,
                    Name = "Method_2"
                }
            };
            return GetQueryableMockDbSet(researchMethods);
        }

        private DbSet<Patient> GetTestPatients()
        {
            var patients = new List<Patient>()
            {
                new Patient()
                {
                    Id = 1,
                    Name = "Ivan",
                    Surname = "Petrov",
                    Patronymic = "Sidorovich",
                    BirthDate = Convert.ToDateTime("01.01.1991"),
                    Gender = "Male",
                    PhoneNumber = "12",
                    SecondPhoneNumber = "13",
                    Comment = "Top",
                    Code = "11"
                },
                new Patient()
                {
                    Id = 2,
                    Name = "Petr",
                    Surname = "Sidorov",
                    Patronymic = "Ivanovich",
                    BirthDate = Convert.ToDateTime("02.02.1992"),
                    Gender = "Male",
                    PhoneNumber = "12",
                    SecondPhoneNumber = "13",
                    Comment = "Top",
                    Code = "12"
                },
                new Patient()
                {
                    Id = 3,
                    Name = "Sidor",
                    Surname = "Ivanov",
                    Patronymic = "Petrovich",
                    BirthDate = Convert.ToDateTime("03.03.1993"),
                    Gender = "Male",
                    PhoneNumber = "12",
                    SecondPhoneNumber = "13",
                    Comment = "Top",
                    Code = "13"
                }
            };
            return GetQueryableMockDbSet(patients);
        }

        private DbSet<Diagnostics> GetTestDiagnostics()
        {
            var diagnostics = new List<Diagnostics>();
            return GetQueryableMockDbSet(diagnostics);
        }

        private DbSet<TypeOfCashlessPayment> GetTestEmptyTypesOfCashlessPayment()
        {
            var typesOfCP = new List<TypeOfCashlessPayment>();
            return GetQueryableMockDbSet(typesOfCP);
        }

        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

            return dbSet.Object;
        }
    }
}