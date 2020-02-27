
using System.ComponentModel.DataAnnotations;

namespace Tasks.DataTransferObjects
{
    /// <summary>
    /// Error response structure
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The number representing the error
        /// </summary>
        [Required]
        public int errorNumber { get; set; }
       
        /// <summary>
        /// Name of the parameter where the error occured
        /// </summary>
        public string parameterName { get; set; }

        /// <summary>
        /// The value that caused the error in the parameter
        /// </summary>
        public string parameterValue { get; set; }
       
        /// <summary>
        /// The descreption of the error
        /// </summary>
        [Required]
        public string errorDescription { get; set; }
    
    /// <summary>
    /// Converts an error number inside an encoded error description, to the standard error number
    /// </summary>
    /// <param name="encodedErrorDescription">The error description</param>
    /// <returns>The decoded error number</returns>
    public static int GetErrorNumberFromDescription(string encodedErrorDescription)
    {
        if (int.TryParse(encodedErrorDescription, out int errorNumber))
        {
            return errorNumber;
        }
        return 0;
    }

    /// <summary>
    /// Converts an error number inside an encoded error description, to the standard error response
    /// </summary>
    /// <param name="encodedErrorDescription">The error description</param>
    /// <returns>The decoded error message and number</returns>
    public static (string decodedErrorMessage, int decodedErrorNumber) GetErrorMessage(string encodedErrorDescription)
    {
        int errorNumber = GetErrorNumberFromDescription(encodedErrorDescription);

            switch (errorNumber)
            {
                case 1:
                    {
                        return ("The entity already exists", errorNumber);
                    }
                case 2:
                    {
                        return ("The parameter value is too large", errorNumber);
                    }
                case 3:
                    {
                        return ("The parameter is required", errorNumber);
                    }
                case 4:
                    {
                        return ("The maximum number of entities have been created." +
                            "No further entities can be created at this time.", errorNumber);
                    }
                case 5:
                    {
                        return ("The entity could not be found", errorNumber);
                    }
                case 6:
                    {
                        return ("The parameter value is too small", errorNumber);
                    }
                case 7:
                    {
                        return ("The parameter value is not valid", errorNumber);
                    }
                default:
                    {
                        return ($"Raw Error: {encodedErrorDescription}", errorNumber);
                    }
            }
        }
    }
}


