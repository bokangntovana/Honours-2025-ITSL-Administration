namespace ITSL_Administration.Models
{
   public enum FileContentType
    {
        // Enum for file content types
        RegistrationDocument, // For user personal info during registration
        CourseContent, // For course content and resourses files
        AssignmentInstruction, // For assignment instructions
        AssignmentSubmission // For submitted assignments
   }

   public enum  AssignmentType
   {
        WrittenAssignment,
        Project,
        Quiz,
        Examination
   }

   public enum CourseRole
   {
        Admin,
        Lecturer,
        Tutor,
        Participant
   }

}
