namespace DeadFileDetector
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Defines an <see cref="Exception"/> that provides an application exit code.
    /// </summary>
    internal class ApplicationException : Exception
    {
        /// <summary>
        /// The exit code.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ApplicationExitCode exitCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exitCode">The exit code.</param>
        public ApplicationException(string message, ApplicationExitCode exitCode)
            : base(message)
        {
            this.exitCode = exitCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ApplicationException(string message)
            : this(message, ApplicationExitCode.Failed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exitCode">The exit code.</param>
        /// <param name="innerException">The inner exception.</param>
        public ApplicationException(string message, ApplicationExitCode exitCode, Exception innerException)
            : base(message, innerException)
        {
            this.exitCode = exitCode;
        }

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        /// <value>
        /// The exit code.
        /// </value>
        public ApplicationExitCode ExitCode
        {
            get { return this.exitCode; }
        }
    }
}
