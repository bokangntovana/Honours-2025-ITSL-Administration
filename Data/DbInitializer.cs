using System;
using System.Linq;
using ITSL_Administration.Models;

namespace ITSL_Administration.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ITSLAdminDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any Participants
            if (context.Participants.Any())
            {
                return;   // DB has been seeded
            }

            var lecturers = new Lecturer[]
            {
                new Lecturer {
                    LecturerID="8512235130084",
                    Name="Rouxan",
                    Surname = "Fouche",
                    Email="foucherc@ufs.ac.za",
                    PhoneNumber="0512300800"
                }
            };
            foreach (Lecturer lecturer in lecturers)
            {
                context.Lecturers.Add(lecturer);
            }
            context.SaveChanges();

            var courses = new Course[]
            {
                new Course {
                    ModuleID="MSDOCX101",
                    ModuleName="Introduction to MS Word",
                    ModuleCredits=10,
                    ModuleDescription="This module will serve to get participants up to speed with the basics of MS Word"
                },
                new Course {
                    ModuleID="MSXLSX101",
                    ModuleName="Introduction to MS Excel",
                    ModuleCredits=10,
                    ModuleDescription="This module will serve to get participants up to speed with the basics of MS Excel"
                },
                new Course {
                    ModuleID="MSPPTX101",
                    ModuleName="Introduction to MS PowerPoint",
                    ModuleCredits=10,
                    ModuleDescription="This module will serve to get participants up to speed with the basics of MS PowerPoint"
                },
            };
            foreach (Course course in courses)
            {
                context.Courses.Add(course);
            }
            context.SaveChanges();

            var tutors = new Tutor[]
            {
                new Tutor {
                    TutorID="0107205130084",
                    Name="Bokang",
                    Surname = "Ntovana",
                    Email="2019702923@ufs.ac.za",
                    PhoneNumber="0814616373",
                    IsRegisteredForITSL = true
                },
                new Tutor {
                    TutorID="0110205130084",
                    Name="Silas Pule",
                    Surname = "Mokoena",
                    Email="2018702923@ufs.ac.za",
                    PhoneNumber="0814806373",
                    IsRegisteredForITSL = true
                },
                new Tutor {
                    TutorID="0009165150084",
                    Name="Keith",
                    Surname = "Jenkins",
                    Email="2020702923@ufs.ac.za",
                    PhoneNumber="0714806379",
                    IsRegisteredForITSL = true
                },
            };
            foreach (Tutor tutor in tutors)
            {
                context.Tutors.Add(tutor);
            }
            context.SaveChanges();

            var participants = new Participant[]
            {
                new Participant {
                    ParticipantID="0107205120083",
                    Name="Lisa",
                    Surname = "Mkhize",
                    Age=24,
                    Email="2019702923@ufs.ac.za",
                    PhoneNumber="0814616373",
                    City="Bloemfontein"
                },
                new Participant {
                    ParticipantID="0108205140085",
                    Name="Khanya",
                    Surname = "Mbule",
                    Age=27,
                    Email="2019702923@ufs.ac.za",
                    PhoneNumber="0814616373",
                    City="Botshabelo"
                },
                new Participant {
                    ParticipantID="0109205160087",
                    Name="Larissa",
                    Surname = "Reyners",
                    Age=30,
                    Email="2019702923@ufs.ac.za",
                    PhoneNumber="0814616373",
                    City="Thaba Nchu"
                }
            };
            foreach (Participant participant in participants)
            {
                context.Participants.Add(participant);
            }
            context.SaveChanges();

            var enrollments = new Enrollment[]
            {
                new Enrollment {
                    EnrollmentID = "ENR001", 
                    ParticipantID = "0107205120083",
                    ModuleID = "MSDOCX101",
                    TutorID = "0107205130084",
                    LecturerID = "8512235130084",
                    Grade = 64.20,
                    IsPassed = true
                },
                new Enrollment {
                    EnrollmentID = "ENR002", 
                    ParticipantID = "0108205140085",
                    ModuleID = "MSXLSX101",
                    TutorID = "0009165150084",
                    LecturerID = "8512235130084",
                    Grade = 80.20,
                    IsPassed = true
                },
                new Enrollment {
                    EnrollmentID = "ENR003", 
                    ParticipantID = "0109205160087",
                    ModuleID = "MSPPTX101",
                    TutorID = "0110205130084",
                    LecturerID = "8512235130084",
                    Grade = 91.20,
                    IsPassed = true
                }
            };
            foreach (Enrollment enrollment in enrollments)
            {
                context.Enrollments.Add(enrollment);
            }
            context.SaveChanges();
        }
    }
}