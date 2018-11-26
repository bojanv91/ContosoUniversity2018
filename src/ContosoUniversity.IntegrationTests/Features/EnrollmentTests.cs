using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContosoUniversity.Services.Features.Courses;
using ContosoUniversity.Services.Features.Enrollments;
using ContosoUniversity.Services.Features.Students;
using ContosoUniversity.Services.Features.Users;
using Flurl.Http;
using Shouldly;
using Xunit;

namespace ContosoUniversity.IntegrationTests.Features
{
    [Collection("WebCollection")]
    public class EnrollmentTests
    {
        private readonly FlurlClient _api;
        private readonly FixtureTestUtil _util;

        public EnrollmentTests(WebFixture fixture)
        {
            _api = fixture.FlurlClient;
            _util = fixture.Util;
        }

        [Fact]
        public async Task Enroll_student_in_course()
        {
            // Arrange
            var tokenResponse = await _util.RegisterUserAndGetTokenAsync();

            var responseStudent = await _api.Request("api/students")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PostJsonAsync(new EnrollStudent.Request
                {
                    Fullname = "Pece",
                    EnrollmentDate = new DateTime(2010, 9, 15)
                })
                .ReceiveJson<EnrollStudent.Response>();

            var responseCourse = await _api.Request("api/courses")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PostJsonAsync(new CreateOrUpdateCourse.Request
                {
                    Title = "Programming 101",
                    Credits = 5
                })
                .ReceiveJson<CreateOrUpdateCourse.Response>();

            var studentId = responseStudent.Id;
            var courseId = responseCourse.Id;

            // Act
            var response = await _api.Request("api/enrollments")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .AllowAnyHttpStatus()
                .PostJsonAsync(new EnrollStudentInCourse.Request
                {
                    StudentId = studentId,
                    CourseId = courseId
                });

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_student_is_enrolled_in_course_ensure_affected_queries_hold_that_new_info()
        {
            // Arrange
            var tokenResponse = await _util.RegisterUserAndGetTokenAsync();

            var responseStudent = await _api.Request("api/students")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PostJsonAsync(new EnrollStudent.Request
                {
                    Fullname = "Deteto",
                    EnrollmentDate = new DateTime(2010, 9, 15)
                })
                .ReceiveJson<EnrollStudent.Response>();

            var responseCourse = await _api.Request("api/courses")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PostJsonAsync(new CreateOrUpdateCourse.Request
                {
                    Title = "Data Mining",
                    Credits = 4
                })
                .ReceiveJson<CreateOrUpdateCourse.Response>();

            var responseEnrollment = await _api.Request("api/enrollments")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PostJsonAsync(new EnrollStudentInCourse.Request
                {
                    StudentId = responseStudent.Id,
                    CourseId = responseCourse.Id
                });

            // Act
            var responseCourseDetails = await _api.Request($"api/courses/{responseCourse.Id}")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .GetJsonAsync<QueryCourse.Response>();

            var responseStudentDetails = await _api.Request($"api/students/{responseStudent.Id}")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .GetJsonAsync<QueryStudent.Response>();

            // Assert
            responseCourseDetails.Enrollments.Count().ShouldBe(1);
            responseCourseDetails.Enrollments.ShouldContain(x => x.StudentFullname == "Deteto");
            responseCourseDetails.TotalStudentsEnrolled.ShouldBe(1);

            responseStudentDetails.Enrollments.Count().ShouldBe(1);
            responseStudentDetails.Enrollments.ShouldContain(x => x.CourseTitle == "Data Mining");
            responseStudentDetails.TotalCoursesEnrolled.ShouldBe(1);
        }

        [Fact]
        public async Task Assign_grade_to_student_for_course()
        {
            // Arrange
            var tokenResponse = await _util.RegisterUserAndGetTokenAsync();

            var responseStudent = await _api.Request("api/students")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PostJsonAsync(new EnrollStudent.Request
                {
                    Fullname = "Ana",
                    EnrollmentDate = new DateTime(2010, 9, 15)
                })
                .ReceiveJson<EnrollStudent.Response>();

            var responseCourse = await _api.Request("api/courses")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PostJsonAsync(new CreateOrUpdateCourse.Request
                {
                    Title = "Programming 404",
                    Credits = 8
                })
                .ReceiveJson<CreateOrUpdateCourse.Response>();

            var responseEnrollment = await _api.Request("api/enrollments")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PostJsonAsync(new EnrollStudentInCourse.Request
                {
                    StudentId = responseStudent.Id,
                    CourseId = responseCourse.Id
                })
                .ReceiveJson<EnrollStudentInCourse.Response>();

            await _api.Request($"api/enrollments/{responseEnrollment.EnrollmentId}/grade")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .PutJsonAsync(new AssignGrade.Request
                {
                    Grade = 10
                });

            // Act
            var responseStudentDetails = await _api.Request($"api/students/{responseStudent.Id}")
                .WithOAuthBearerToken(tokenResponse.Access_token)
                .GetJsonAsync<QueryStudent.Response>();

            // Assert
            responseStudentDetails.Enrollments.ShouldContain(x => x.CourseTitle == "Programming 404" && x.Grade == 10);
        }
    }
}
