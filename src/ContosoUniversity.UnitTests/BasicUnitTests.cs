using System;
using System.Collections.Generic;
using System.Text;
using ContosoUniversity.Core;
using ContosoUniversity.Core.Students;
using ContosoUniversity.Core.Users;
using Shouldly;
using Xunit;

namespace ContosoUniversity.UnitTests
{
    public class BasicUnitTests
    {
        [Fact]
        public void Create_system_user()
        {
            //Arrange, Act
            var user = User.CreateSystemUser("Pece", "pece@example.com", "asdf");

            //Assert
            user.Fullname.ShouldBe("Pece");
            user.Email.ShouldBe("pece@example.com");
            user.Status.ShouldBe(UserStatus.Activated);
        }

        [Fact]
        public void Enroll_student_in_valid_enrollment_date()
        {
            var validEnrollmentDate = new DateTime(2018, 9, 15);
            Student.Enroll("Pece", validEnrollmentDate);    // no exception
        }

        [Fact]
        public void Cannot_enroll_student_in_future_date()
        {
            Assert.Throws<CoreException>(() =>
            {
                Student.Enroll("Pece", DateTime.Now.AddYears(1));
            });
        }
    }
}
