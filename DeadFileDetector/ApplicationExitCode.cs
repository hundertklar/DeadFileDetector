namespace DeadFileDetector
{
    /// <summary>
    /// Enumeration with the different application exit codes.
    /// </summary>
    internal enum ApplicationExitCode
    {
        /// <summary>
        /// The application finished successfully.
        /// </summary>
        Succeeded = 0,

        /// <summary>
        /// The application failed without expected reason.
        /// </summary>
        Failed = unchecked((int)0x80004005),

        /// <summary>
        /// The application received one or more invalid argument/s.
        /// </summary>
        InvalidArguments = unchecked((int)0x80070057),

        /// <summary>
        /// The received file was not found.
        /// </summary>
        FileNotFound = unchecked((int)0x80070002),


        UnreferencedFilesFound = unchecked((int)0x80070003),
    }
}
