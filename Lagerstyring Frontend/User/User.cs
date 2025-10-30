namespace LagerstyringsSystem.src.User {
    public class User {
        public int Id { get; private set; }
        public string Username { get; set; }
        public string PasswordClear { get; set; }
        public int AuthEnum { get; set; }
    }
}
