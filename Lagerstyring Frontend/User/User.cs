namespace LagerstyringsSystem.src.User {
    /// <summary>
    /// Minimal user DTO for frontend usage.
    /// </summary>
    /// <remarks>
    /// Mirrors backend public user projections relevant to the UI.
    /// </remarks>
    public class User {
        /// <summary>Identifier.</summary>
        public int Id { get; private set; }
        /// <summary>Username.</summary>
        public string Username { get; set; }
        /// <summary>Clear-text password placeholder for creation flows.</summary>
        public string PasswordClear { get; set; }
        /// <summary>Authorization role enumeration value.</summary>
        public int AuthEnum { get; set; }
    }
}
