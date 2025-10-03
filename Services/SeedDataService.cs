using ITSL_Administration.Data;
using ITSL_Administration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSL_Administration.Services
{
    public class SeedDataService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope= serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedDataService>>();

            try
            {
                //Ensure DB is created
                logger.LogInformation("Ensure the database is created...");
                await context.Database.EnsureCreatedAsync();

                logger.LogInformation("Seeding roles");

                //List of user roles expected for ITSLAdminDb
                string[] roles = { "Admin", "Donor", "Tutor", "Lecturer", "Participant" };
                //Add roles
                foreach (var item in roles)
                {
                    if(!await roleManager.RoleExistsAsync(item))
                    {
                        await roleManager.CreateAsync(new IdentityRole(item));
                    }
                }

                //Add Seed Users
                logger.LogInformation("Seeding users");

                //Seed Admin User
                var adminEmail = "sysAdmin@gmail.com";
                if(await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new User
                    {
                        FullName = "sysAdmin",
                        IDNumber = "0000202512",
                        UserName = adminEmail,
                        NormalizedUserName = adminEmail.ToUpper(),
                        Email = adminEmail,
                        NormalizedEmail = adminEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()

                    };

                    var result = await userManager.CreateAsync(adminUser,"sysAdmin@2025");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Admin user created with Admin role");
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description )));

                    }
                }

                //Seed Lecturer User
                var lecturerEmail = "erobinson@gmail.com";
                if (await userManager.FindByEmailAsync(lecturerEmail) == null)
                {
                    var lecturer = new User
                    {
                        FullName = "Edward Robinson",
                        IDNumber = "8110215140846",
                        UserName = lecturerEmail,
                        NormalizedUserName = lecturerEmail.ToUpper(),
                        Email = lecturerEmail,
                        NormalizedEmail = lecturerEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()

                    };

                    var result = await userManager.CreateAsync(lecturer, "sysLecturer@2025");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("lecturer user created with Lecturer role");
                        await userManager.AddToRoleAsync(lecturer, "Lecturer");
                    }
                    else
                    {
                        logger.LogError("Failed to create lecturer user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

                    }
                }

                //Seed Tutor User
                var tutorEmail = "kateMurphy@gmail.com";
                if (await userManager.FindByEmailAsync(tutorEmail) == null)
                {
                    var tutor = new User
                    {
                        FullName = "Kate Murphy",
                        IDNumber = "0210215140846",
                        UserName = tutorEmail,
                        CampusName="Bloemfontein Campus",
                        NormalizedUserName = tutorEmail.ToUpper(),
                        Email = tutorEmail,
                        NormalizedEmail = tutorEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()

                    };

                    var result = await userManager.CreateAsync(tutor, "sysTutor@2025");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Tutor user created with Tutor role");
                        await userManager.AddToRoleAsync(tutor, "Tutor");
                    }
                    else
                    {
                        logger.LogError("Failed to create tutor user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

                    }
                }

                //Seed Participant User
                var ParticipantEmail = "Larissa_Reyners@gmail.com";
                if (await userManager.FindByEmailAsync(ParticipantEmail) == null)
                {
                    var participant = new User
                    {
                        FullName = "Larissa Reyners",
                        IDNumber = "9504225140846",
                        Age = 30,
                        UserName = ParticipantEmail,
                        CampusName = "Bloemfontein Campus",
                        City ="Thaba Nchu",
                        NormalizedUserName = ParticipantEmail.ToUpper(),
                        Email = ParticipantEmail,
                        NormalizedEmail = ParticipantEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()

                    };

                    var result = await userManager.CreateAsync(participant, "sysPartiC@2025");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Participant user created with Participant role");
                        await userManager.AddToRoleAsync(participant, "Participant");
                    }
                    else
                    {
                        logger.LogError("Failed to create participant user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

                    }
                }

                //Seed Donor User
                var DonorEmail = "thabo_mdluli001@gmail.com";
                if (await userManager.FindByEmailAsync(DonorEmail) == null)
                {
                    var donor = new User
                    {
                        FullName = "Thabo Mdluli",
                        IDNumber = "9804225140846",
                        Age = 27,
                        UserName = DonorEmail,
                        isVolunteer = true,
                        AmountDonated= 137.50,
                        City = "Bloemfontein",
                        NormalizedUserName = DonorEmail.ToUpper(),
                        Email = DonorEmail,
                        NormalizedEmail = DonorEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()

                    };

                    var result = await userManager.CreateAsync(donor, "sysDonor@2025");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Donor user created with Donor role");
                        await userManager.AddToRoleAsync(donor, "Donor");
                    }
                    else
                    {
                        logger.LogError("Failed to create donor user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

                    }
                }

                //Seed Courses
                logger.LogInformation("Seeding courses");
                var courses = new Course[]
                {
                    new Course {
                        CourseCode="MSDOCX101",
                        CourseName="Introduction to MS Word",
                        CourseCredits=10,
                        CourseDescription="This module will serve to get participants up to speed with the basics of MS Word"
                    },
                    new Course {
                        CourseCode="MSXLSX101",
                        CourseName="Introduction to MS Excel",
                        CourseCredits=10,
                        CourseDescription="This module will serve to get participants up to speed with the basics of MS Excel"
                    },
                    new Course {
                        CourseCode="MSPPTX101",
                        CourseName="Introduction to MS PowerPoint",
                        CourseCredits=10,
                        CourseDescription="This module will serve to get participants up to speed with the basics of MS PowerPoint"
                    },
                };

                //Check if courses already exist before seeding
                foreach (Course _course in courses)
                {
                    var existingCourse = await context.Courses
                        .FirstOrDefaultAsync(c => c.CourseCode == _course.CourseCode);

                    if (existingCourse == null)
                    {
                        logger.LogInformation("Adding course: {CourseCode}", _course.CourseCode);
                        context.Courses.Add(_course);
                    }
                    else
                    {
                        logger.LogInformation("Course already exists: {CourseCode}", _course.CourseCode);
                    }
                }
                await context.SaveChangesAsync();

                //seeding events
                logger.LogInformation("Seeding sample events");

                var sampleEvents = new EventSchedule[]
                {
                    new EventSchedule {
                    Title = "MS Word Orientation",
                    Start = DateTime.Parse("2025-06-01T09:00:00"),
                    End = DateTime.Parse("2025-06-01T12:00:00"),
                    Description = "Introductory session for MS Word course",
                    BackgroundColor = "#3788d8",
                    IsAllDay = false
                    },
                    new EventSchedule {
                    Title = "MS Excel Orientation",
                    Start = DateTime.Parse("2025-06-02T09:00:00"),
                    End = DateTime.Parse("2025-06-02T12:00:00"),
                    Description = "Introductory session for MS excel course",
                    BackgroundColor = "#3788d8",
                    IsAllDay = false
                    },
                     new EventSchedule {
                    Title = "MS Access Orientation",
                    Start = DateTime.Parse("2025-06-01T09:00:00"),
                    End = DateTime.Parse("2025-06-01T12:00:00"),
                    Description = "Introductory session for MS Word course",
                    BackgroundColor = "#3788d8",
                    IsAllDay = false
                    },
                    new EventSchedule {
                    Title = "MS PowerPoint Orientation",
                    Start = DateTime.Parse("2025-05-28T09:00:00"),
                    End = DateTime.Parse("2025-05-29T12:00:00"),
                    Description = "Introductory session for MS Word course",
                    BackgroundColor = "#3788d8",
                    IsAllDay = false
                    }

                };

                foreach (var evt in sampleEvents)
                {
                    if (!context.Events.Any(e => e.Title == evt.Title && e.Start == evt.Start))
                    {
                        context.Events.Add(evt);
                    }
                }
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }

        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
