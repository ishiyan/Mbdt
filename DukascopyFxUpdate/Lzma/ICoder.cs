// ICoder.h

using System;

namespace SevenZip
{
    /// <summary>
    /// The exception that is thrown when an error in input stream occurs during decoding.
    /// </summary>
    class DataErrorException : ApplicationException
    {
        public DataErrorException() : base("Data Error") { }
    }

    /// <summary>
    /// The exception that is thrown when the value of an argument is outside the allowable range.
    /// </summary>
    class InvalidParamException : ApplicationException
    {
        public InvalidParamException() : base("Invalid Parameter") { }
    }

    public interface ICodeProgress
    {
        /// <summary>
        /// Callback progress.
        /// </summary>
        /// <param name="inSize">
        /// input size. -1 if unknown.
        /// </param>
        /// <param name="outSize">
        /// output size. -1 if unknown.
        /// </param>
        // ReSharper disable once UnusedMember.Global
        void SetProgress(Int64 inSize, Int64 outSize);
    };

    public interface ICoder
    {
        /// <summary>
        /// Codes streams.
        /// </summary>
        /// <param name="inStream">
        /// input Stream.
        /// </param>
        /// <param name="outStream">
        /// output Stream.
        /// </param>
        /// <param name="inSize">
        /// input Size. -1 if unknown.
        /// </param>
        /// <param name="outSize">
        /// output Size. -1 if unknown.
        /// </param>
        /// <param name="progress">
        /// callback progress reference.
        /// </param>
        /// <exception cref="SevenZip.DataErrorException">
        /// if input stream is not valid
        /// </exception>
        // ReSharper disable once UnusedMemberInSuper.Global
        // ReSharper disable UnusedParameter.Global
        void Code(System.IO.Stream inStream, System.IO.Stream outStream,
            Int64 inSize, Int64 outSize, ICodeProgress progress);
        // ReSharper restore UnusedParameter.Global
    };

    public interface ISetDecoderProperties
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        void SetDecoderProperties(byte[] properties);
    }
}
