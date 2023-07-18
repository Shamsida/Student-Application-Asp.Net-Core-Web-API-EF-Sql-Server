using Test.Models;

namespace Test.DTOs
{
    public class ResponseDTO
    {
        public int Page { get; set; }
        public int CurrentPage { get; set; }

        public List<Student> Students { get; set; } = new List<Student>();
    }
}
